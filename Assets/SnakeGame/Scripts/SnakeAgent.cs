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

namespace SnakeGame.Scripts
{
    public class SnakeAgent : Agent
    {
        #region Serialized Fields

        [SerializeField] private Board board;

        [SerializeField] private BoardDisplay boardDisplay;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text averageScoreText;
        [SerializeField] private TMP_Text currentRewardText;
        [SerializeField] private TMP_Text averageRewardText;

        [SerializeField] private bool isDisplayOn;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private int foodCount;
        [Range(5, 100)] [SerializeField] private int height;
        [Range(1, 10)] [SerializeField] private int maxPathLength;
        [SerializeField] private int numberOfSnakes;
        [SerializeField] private int startSize;
        [Range(5, 100)] [SerializeField] private int width;

        #endregion

        private readonly Vector2Int[] _directions =
        {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0),
        };

        private int[] _actionLongestPathArray;


        private float _currentTimer;
        private int _highScore;

        private Vector2Int
                _inputDirection = Vector2Int.up; // keeps track of the last input direction

        private int _longestPath;
        private bool _shouldRecalculatePaths;
        private SnakeController _snakeController;
        private float currentReward;
        private float averageReward;
        private double averageScore;
        [SerializeField] private List<float> _episodeRewards = new();
        [SerializeField] private List<int> _episodeScores = new();

        public SnakeAgent()
        {
            InitializeGame();
            _actionLongestPathArray = new int[3];
            _shouldRecalculatePaths = true;
            MaxDistance = Mathf.Sqrt((board.Height * board.Height) + (board.Width * board.Width));
        }

        private float PreviousDistance { get; set; }

        #region Event Functions

        private void Start()
        {
            InitializeGame();
            _actionLongestPathArray = new int[3];
            _shouldRecalculatePaths = true;
        }

        private void Update()
        {
            Vector2Int tempDirection = Vector2Int.zero;

            if (Keyboard.current.wKey.isPressed)
            {
                tempDirection = new Vector2Int(0, 1);
            }
            else if (Keyboard.current.dKey.isPressed)
            {
                tempDirection = new Vector2Int(1, 0);
            }
            else if (Keyboard.current.sKey.isPressed)
            {
                tempDirection = new Vector2Int(0, -1);
            }
            else if (Keyboard.current.aKey.isPressed)
            {
                tempDirection = new Vector2Int(-1, 0);
            }

            if (tempDirection != Vector2Int.zero)
            {
                _inputDirection = tempDirection;
            }

            if (_shouldRecalculatePaths)
            {
                CalculateLongestPathsForAllDirections();

                _shouldRecalculatePaths = false;
            }
        }

        #endregion

        public override void CollectObservations(VectorSensor sensor)
        {
            const int view = 8; // Define view size as per your needs

            // Assuming that there is always at least one snake and one food in the game
            Snake snake = board.Snakes[0];
            Vector2Int food = board.FoodPositions[0];

            // Calculate direction to the food relative to the snake's heading
            Vector2 directionToFood = food - snake.Position;
            // Normalize directionToFood
            directionToFood = directionToFood.normalized;

            // Add the direction to the food (2 floats)
            sensor.AddObservation(directionToFood);

            // Check for immediate dangers: front, left, right relative to the snake's current direction
            Vector2Int forwardPosition = snake.Position + snake.Direction;
            Vector2Int leftPosition = snake.Position + RotateCounterClockwise(snake.Direction);
            Vector2Int rightPosition = snake.Position + RotateClockwise(snake.Direction);
            // Check if next positions are inside the board and not occupied by the snake's body
            sensor.AddObservation(IsPositionSafe(forwardPosition));
            sensor.AddObservation(IsPositionSafe(leftPosition));
            sensor.AddObservation(IsPositionSafe(rightPosition));

            CalculateLongestPathsForAllDirections();

            sensor.AddObservation(_actionLongestPathArray[1]);
            sensor.AddObservation(_actionLongestPathArray[0]);
            sensor.AddObservation(_actionLongestPathArray[2]);

            // Assuming that there is always at least one snake in the game
            // Vector2Int snakeHeadPosition = board.Snakes[0].Position;

            // Position of the top left corner of the window
            // Vector2Int windowStart = snakeHeadPosition - new Vector2Int(view / 2, view / 2);
            Vector2Int windowStart = Vector2Int.zero;

            // Loop over each cell in the window
            for (int i = 0; i < view; i++)
            {
                for (int j = 0; j < view; j++)
                {
                    Vector2Int cellPos = windowStart + new Vector2Int(i, j);
                    Tile tile = board.GetTile(i, j);

                    int cellValue;

                    if (cellPos.x < 0 || cellPos.y < 0 || cellPos.x >= board.Width ||
                        cellPos.y >= board.Height)
                            // Cell is out of bounds
                    {
                        cellValue = -1;
                    }
                    else if (tile.Type == TileType.Snake)
                            // Cell is occupied by the snake
                    {
                        cellValue = 1;
                    }
                    else if (tile.Type == TileType.Food)
                            // Cell contains food
                    {
                        cellValue = 2;
                    }
                    else
                            // Cell is empty
                    {
                        cellValue = 0;
                    }

                    sensor.AddObservation(cellValue);
                }
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            Vector2Int currentDirection = board.Snakes[0].Direction;
            int relativeDirection = GetRelativeDirection(currentDirection, _inputDirection);
            ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = relativeDirection;
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            int action = actions.DiscreteActions[0];

            if (board.Snakes.Length == 0)
            {
                return;
            }

            Snake snake = board.Snakes[0];

            HandleSnakeDirection(action, snake);

            ProcessSnakeMovement(snake);

            board.ClearBoard();
            board.DrawFood(board.FoodPositions);

            if (IsSnakeAlive(snake))
            {
                board.DrawSnake(board.Snakes[0]);
            }

            // show direction
            if (isDisplayOn)
            {
                boardDisplay.DrawBoard(board);
            }

            CheckForNewHighScore(snake);


            // Get action's longest path from the array
            /*int actionLongestPath = _actionLongestPathArray[action];


            // If the longest path is shorter than the maximum, it's a bad action (blocked), apply punishment
            if (actionLongestPath < maxPathLength)
            {
                Debug.Log("Bad action");
                AddReward(-1f);
                currentReward -= 1f;
            }*/
            // Good action, apply reward
            /*else
            {
                Debug.Log("Good action");
                AddReward(0.1f);
                currentReward += 0.1f;
            }*/

            if (board.FoodPositions.Count > 0)
            {
                Vector2Int currentTilePos = snake.Position;
                Vector2 foodPosition = board.FoodPositions[0];

                //Euclidean distance to food from current position
                float currentDistance = Vector2.Distance(currentTilePos, foodPosition);

                // Calculate the actual distance to the food

                // If the current distance is within the reward radius, apply the reward
                /*float normalizedCurrentDistance = currentDistance /
                                                  Mathf.Sqrt(
                                                          board.Width * board.Width +
                                                          board.Height * board.Height);
                float normalizedPreviousDistance = PreviousDistance /
                                                   Mathf.Sqrt(board.Width * board.Width +
                                                              board.Height * board.Height);
                // Compute the reward based on the current distance
                float reward = Mathf.Log((snake.Length + normalizedPreviousDistance) /
                                         (snake.Length + normalizedCurrentDistance));

                // normalize the reward between -1 and 1

                // Apply the reward
                AddReward(reward);
                Debug.Log("Distance Reward :" + reward);
                currentReward += reward;*/

                PreviousDistance = currentDistance;

                if (currentDistance < PreviousDistance)
                {
                    // AddReward(0.01f);
                    // Debug.Log("Closer :" + 0.01f);
                    // currentReward += 0.01f;
                }
                else if (currentDistance > PreviousDistance)
                {
                    // AddReward(-0.01f);
                    // currentReward -= 0.01f;
                    // Debug.Log("Further :" + -0.01f);
                }

                PreviousDistance = currentDistance;
            }

            CheckSnakeStatus(snake);

            _shouldRecalculatePaths = true;
        }

        public override void OnEpisodeBegin()
        {
            InitializeGame();
            currentReward = 0;
        }

        private void CalculateLongestPathsForAllDirections()
        {
            Snake snake = board.Snakes[0];

            for (int action = 0; action < 3; ++action)
            {
                Vector2Int actionDirection = action switch
                {
                    0 => RotateCounterClockwise(snake.Direction),
                    1 => snake.Direction,
                    2 => RotateClockwise(snake.Direction),
                    _ => snake.Direction,
                };

                int longestPath = FindLongestPath(snake.Position + actionDirection,
                                                  new HashSet<Vector2Int>(), maxPathLength);
                _actionLongestPathArray[action] = longestPath;
            }
        }

        private void CheckForNewHighScore(Snake snake)
        {
            // update high score if current score is higher
            if (board.Snakes[0].Score > _highScore)
            {
                _highScore = snake.Score;
            }

            highScoreText.text = "High Score: " + _highScore;

            scoreText.text = "Score: " + board.Snakes[0].Score;
        }

        private void CheckSnakeStatus(Snake snake)
        {
            if (snake.AteFood)
            {
                Debug.Log("Eat : " + 10f);
                currentReward += 10f;
                AddReward(10f);
                snake.AteFood = false;
            }

            AddReward(-0.1f);
            currentReward += -0.1f;

            if (!IsSnakeAlive(snake))
            {
                Debug.Log("Dead : " + -10f);
                currentReward -= 10f;
                AddReward(-10f);
                _episodeRewards.Add(currentReward);
                _episodeScores.Add(board.Snakes[0].Score);
            }

            if (_episodeRewards.Any())
            {
                averageReward = _episodeRewards.Average();
            }

            if (_episodeScores.Any())
            {
                averageScore = _episodeScores.Average();
            }

            currentRewardText.text = "Current Reward: " + currentReward.ToString("F2");
            averageRewardText.text = "Average Reward: " + averageReward.ToString("F2");
            averageScoreText.text = "Average Score: " + averageScore.ToString("F2");

            if (!IsSnakeAlive(snake))
            {
                EndEpisode();
            }
        }

        private int FindLongestPath(Vector2Int position, HashSet<Vector2Int> visitedPositions,
                                    int maxLength, int currentLength = 0)
        {
            visitedPositions.Add(position);

            // Exit if the path length equals maxPathLength
            if (currentLength == maxLength)
            {
                return currentLength;
            }

            int longestPath = 0;

            foreach (Vector2Int dir in _directions)
            {
                Vector2Int nextPosition = position + dir;

                if (IsPositionSafe(nextPosition) && !visitedPositions.Contains(nextPosition))
                {
                    longestPath = Math.Max(longestPath,
                                           FindLongestPath(nextPosition, visitedPositions,
                                                           maxLength, currentLength + 1));
                }
            }

            visitedPositions.Remove(position);

            return longestPath;
        }

        private int GetRelativeDirection(Vector2Int currentDirection, Vector2Int inputDirection)
        {
            Vector2Int clockwiseDirection = RotateClockwise(currentDirection);
            Vector2Int counterClockwiseDirection = RotateCounterClockwise(currentDirection);

            if (inputDirection == counterClockwiseDirection)
            {
                return 0; // turn left
            }

            if (inputDirection == currentDirection)
            {
                return 1; // go straight
            }

            if (inputDirection == clockwiseDirection)
            {
                return 2; // turn right
            }

            return 1;
        }

        private void HandleSnakeDirection(int action, Snake snake)
        {
            switch (action)
            {
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
        }

        private void InitializeGame()
        {
            _snakeController = new SnakeController();
            Snake[] snakes =
                    _snakeController.CreateSnakes(width, height, numberOfSnakes, startSize);

            if (board != null)
            {
                board.Snakes = snakes;
                board.Reset(snakes, width, height);

                if (isDisplayOn)
                {
                    boardDisplay.Reset();
                }
            }
            else
            {
                board = new Board(width, height, snakes);
            }

            PreviousDistance = MaxDistance;

            // food
            for (int i = 0; i < foodCount; i++)
            {
                board.FoodPositions.Add(board.SpawnFood());
            }
        }
        public float MaxDistance { get; set; }

        /// <summary>
        ///     Determines if a given position on the game board is safe.
        /// </summary>
        /// <param name="position" cref="Board">The position to check.</param>
        /// <returns>
        ///     True if the position is within the bounds of the board, defined by the ‘width’ and ‘height’
        ///     properties, and neither collides with any body part of a snake in the Snake collection of the
        ///     Board class.
        ///     False otherwise.
        /// </returns>
        private bool IsPositionSafe(Vector2Int position)
        {
            // Define the condition for the position to be safe. The following is just a basic example.
            // You might need to add more conditions or modify it according to your game rules.

            // Check if out of bounds
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
            {
                return false;
            }

            // Check if would collide with the snake
            foreach (Snake snake in board.Snakes)
            {
                if (snake.Body.Contains(position))
                {
                    return false;
                }
            }

            // If passed both conditions, the position is safe.
            return true;
        }

        private static bool IsSnakeAlive(Snake snake) => snake.IsAlive;

        private void ProcessSnakeMovement(Snake snake)
        {
            _snakeController.FinalizeDirection(snake);
            _snakeController.CheckCollisions(board);

            _snakeController.Move(board, board.Snakes[0], false);
        }

        private Vector2Int RotateClockwise(Vector2Int direction) => new(direction.y, -direction.x);

        private Vector2Int RotateCounterClockwise(Vector2Int direction) =>
                new(-direction.y, direction.x);
    }
}
