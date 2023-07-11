#region

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace SnakeGame.Scripts {
    public class SnakeAgent : Agent {
        #region Serialized Fields

        [SerializeField] private BoardDisplay boardDisplay;
        [Range(5, 100)] [SerializeField] private int width = 10;
        [Range(5, 100)] [SerializeField] private int height = 10;
        [SerializeField] private List<Player> players;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private bool wrapIsEnabled;
        [SerializeField] private int foodCount = 1;
        [SerializeField] private Board board;
        [SerializeField] private int numberOfSnakes = 1;
        [SerializeField] private int startSize = 1;
        [SerializeField] private bool isDisplayOn;

        #endregion

        private float currentReward;


        private readonly Vector2Int[] _actionDirections = {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0),
        };
        private Vector2Int _inputDirection = Vector2Int.up; // keeps track of the last input direction

        private float _currentTimer;
        private int _highScore;
        private float _previousDistance;
        private SnakeController _snakeController;
        public float PreviousDistance
        {
            get => _previousDistance;
            set => _previousDistance = value;
        }

        #region Event Functions

        private void Start() {
            InitializeGame();
        }

        private void Update() {
            var tempDirection = Vector2Int.zero;

            if (Keyboard.current.wKey.isPressed) {
                tempDirection = new Vector2Int(0, 1);
            } else if (Keyboard.current.dKey.isPressed) {
                tempDirection = new Vector2Int(1, 0);
            } else if (Keyboard.current.sKey.isPressed) {
                tempDirection = new Vector2Int(0, -1);
            } else if (Keyboard.current.aKey.isPressed) {
                tempDirection = new Vector2Int(-1, 0);
            }

            if (tempDirection != Vector2Int.zero) _inputDirection = tempDirection;
        }

        #endregion

        private int ActionIndexForDirection(Vector2Int direction) {
            int index = Array.IndexOf(_actionDirections, direction);


            if (index == -1) {
                Debug.LogError("Invalid direction");
                return 0; // This should be reasonable default value.
            }

            return index;
        }

        private Vector2Int GetDirectionFromAction(int actionIndex) {
            return _actionDirections[actionIndex];
        }

        private void InitializeGame() {
            _snakeController = new SnakeController();
            var snakes = _snakeController.CreateSnakes(width, height, numberOfSnakes, startSize);

            if (board != null) {
                board.Snakes = snakes;
                board.Reset(snakes, width, height);

                if (isDisplayOn) {
                    boardDisplay.Reset();
                }
            } else {
                board = new Board(width, height, snakes);
            }


            // food
            for (int i = 0; i < foodCount; i++) {
                board.FoodPositions.Add(board.SpawnFood());
            }
        }


        public override void OnEpisodeBegin() {
            InitializeGame();
        }


        private static bool IsSnakeAlive(Snake snake) {
            return snake.IsAlive;
        }


        public override void CollectObservations(VectorSensor sensor) {
            const int view = 5; // Define view size as per your needs

            // Assuming that there is always at least one snake and one food in the game
            var snake = board.Snakes[0];
            var food = board.FoodPositions[0];

            // Calculate direction to the food relative to the snake's heading
            Vector2 directionToFood = (food - snake.Position);
            // Normalize directionToFood
            directionToFood = directionToFood.normalized;


            // Add the direction to the food (2 floats)
            sensor.AddObservation(directionToFood);

            // Check for immediate dangers: front, left, right relative to the snake's current direction
            var forwardPosition = snake.Position + snake.Direction;
            var leftPosition = snake.Position + RotateCounterClockwise(snake.Direction);
            var rightPosition = snake.Position + RotateClockwise(snake.Direction);

            // Check if next positions are inside the board and not occupied by the snake's body
            sensor.AddObservation(IsPositionSafe(forwardPosition));
            sensor.AddObservation(IsPositionSafe(leftPosition));
            sensor.AddObservation(IsPositionSafe(rightPosition));

            // Assuming that there is always at least one snake in the game
            var snakeHeadPosition = board.Snakes[0].Position;

            // Position of the top left corner of the window
            var windowStart = snakeHeadPosition - new Vector2Int(view / 2, view / 2);

            // Loop over each cell in the window
            for (int i = 0; i < view; i++) {
                for (int j = 0; j < view; j++) {
                    var cellPos = windowStart + new Vector2Int(i, j);
                    var tile = board.GetTile(i, j);

                    int cellValue;

                    if (cellPos.x < 0 || cellPos.y < 0 || cellPos.x >= board.Width || cellPos.y >= board.Height) {
                        // Cell is out of bounds
                        cellValue = -1;
                    } else if (tile.Type == TileType.Snake) {
                        // Cell is occupied by the snake
                        cellValue = 1;
                    } else if (tile.Type == TileType.Food) {
                        // Cell contains food
                        cellValue = 2;
                    } else {
                        // Cell is empty
                        cellValue = 0;
                    }

                    sensor.AddObservation(cellValue);
                }
            }
        }

        private bool IsPositionSafe(Vector2Int position) {
            // Define the condition for the position to be safe. The following is just a basic example.
            // You might need to add more conditions or modify it according to your game rules.

            // Check if out of bounds
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
                return false;

            // Check if would collide with the snake
            foreach (var snake in board.Snakes) {
                if (snake.Body.Contains(position)) {
                    return false;
                }
            }

            // If passed both conditions, the position is safe.
            return true;
        }

        private Vector2Int RotateClockwise(Vector2Int direction) {
            return new Vector2Int(direction.y, -direction.x);
        }

        private Vector2Int RotateCounterClockwise(Vector2Int direction) {
            return new Vector2Int(-direction.y, direction.x);
        }

        public override void OnActionReceived(ActionBuffers actions) {
            int action = actions.DiscreteActions[0];

            if (board.Snakes.Length == 0) {
                return;
            }

            var snake = board.Snakes[0];
            Vector2 FoodPosition, previousFoodPosition = Vector2.zero;

            if (board.FoodPositions.Count > 0) {
                FoodPosition = board.FoodPositions[0];
                previousFoodPosition = FoodPosition;
            }

            switch (action) {
                case 0: //turn left
                    snake.NextDirection = RotateCounterClockwise(snake.Direction);
                    break;
                case 1: //go straight, no need to change the direction
                    snake.NextDirection = snake.Direction;
                    break;
                case 2: //turn right
                    snake.NextDirection = RotateClockwise(snake.Direction);
                    break;
            }

            _snakeController.FinalizeDirection(snake);
            _snakeController.CheckCollisions(board);

            _snakeController.Move(board, board.Snakes[0], false);

            board.ClearBoard();
            board.DrawFood(board.FoodPositions);

            if (IsSnakeAlive(snake)) {
                board.DrawSnake(board.Snakes[0]);
                // show direction
            }



            if (isDisplayOn) {
                boardDisplay.DrawBoard(board);
            }

            // update high score if current score is higher
            if (board.Snakes[0].Score > _highScore) {
                _highScore = snake.Score;
            }

            highScoreText.text = "High Score: " + _highScore;


            scoreText.text = "Score: " + board.Snakes[0].Score;


            // Distance Reward
            const int rewardRadius = 4;

            if (board.FoodPositions.Count > 0) {
                var currentTilePos = snake.Position;
                    FoodPosition = board.FoodPositions[0];
                     
                    //Euclidean distance to food from current position
                float currentDistance = Vector2.Distance(currentTilePos, FoodPosition);
                    // Normalize the current distance, max distance will be the sqrt of (height^2 + width^2)
                float normalizedCurrentDistance = currentDistance /
                                                  Mathf.Sqrt((board.Width * board.Width) +
                                                             (board.Height * board.Height));

                if (normalizedCurrentDistance <= rewardRadius) {
                    // Compute the reward based on the Normalized current distance
                    // float reward = Mathf.Log((snake.Length + PreviousDistance) / (snake.Length + currentDistance));


                    // Define the reward
                    float reward = Mathf.Log((snake.Length + PreviousDistance)
                                             / (snake.Length + normalizedCurrentDistance));

                    // Debug.Log("Distance Reward:" + reward);

                    // Apply the reward
                    // AddReward(reward);
                    PreviousDistance = currentDistance;
                }
            }


            if (board.Snakes[0].AteFood) {
                Debug.Log("Eat : " + 10f);
                AddReward(10f);
                board.Snakes[0].AteFood = false;
            }

            AddReward(-0.1f);

            if (!IsSnakeAlive(snake)) {
                Debug.Log("Dead : " + -10f);
                AddReward(-10f);
                EndEpisode();
            }
        }


        public override void Heuristic(in ActionBuffers actionsOut) {
            // Debug.Log("Heuristic");
            Vector2Int currentDirection = board.Snakes[0].Direction;
            int relativeDirection = GetRelativeDirection(currentDirection, _inputDirection);
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = relativeDirection;
        }

        private int GetRelativeDirection(Vector2Int currentDirection, Vector2Int inputDirection) {
            var clockwiseDirection = RotateClockwise(currentDirection);
            var counterClockwiseDirection = RotateCounterClockwise(currentDirection);




            if (inputDirection == counterClockwiseDirection)
                return 0; // turn left
            if (inputDirection == currentDirection)
                return 1; // go straight
            if (inputDirection == clockwiseDirection)
                return 2; // turn right


            return 1;
        }
    }
}