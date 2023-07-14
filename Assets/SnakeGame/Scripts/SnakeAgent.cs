#region

using System;
using System.Collections.Generic;
using TMPro;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace SnakeGame.Scripts
{
    /// <summary>
    /// The SnakeAgent class is responsible for controlling the behavior of the <see cref="Snake"/> in the game.
    /// It contains methods for finding the longest path,
    /// getting the relative direction of the input direction relative to the current direction,
    /// handling the snake's direction based on the given action, initializing the game by creating snakes,
    /// resetting the <see cref="Scripts.Board"/>, and spawning food, determining if a given position on the game <see cref="Scripts.Board"/> is safe,
    /// processing snake movement, and rotating the snake's direction clockwise and counterclockwise.
    /// </summary>
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
        [Range(5, 100)] [SerializeField] private int width;
        [Range(5, 100)] [SerializeField] private int height;
        [Range(1, 10)] [SerializeField] private int maxPathLength;
        [SerializeField] private int numberOfSnakes;
        [SerializeField] private int startSize;
        [SerializeField] private int maxHungryTime = 64;
        [SerializeField] private int resetAverageCount = 250;


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
        private double currentReward = 0.0;
        private double totalScore = 0.0;
        private double totalReward = 0.0;
        [SerializeField] private int episodeCount;

        private double averageReward;
        private double averageScore;
        [SerializeField] private int _eatTimer = 0;
        private float currentDistance;
        [SerializeField] private float previousDistanceEditor;


        /// <summary>
        /// Initializes a new instance of the <see cref="SnakeAgent"/> class.
        /// </summary>
        public SnakeAgent()
        {
            InitializeGame();
            _actionLongestPathArray = new int[3];
            _shouldRecalculatePaths = true;
            MaxDistance = Mathf.Sqrt((Board.Height * Board.Height) + (Board.Width * Board.Width));
        }

        private float PreviousDistance { get; set; }

        #region Event Functions

        /// <summary>
        /// Initializes the game and sets up the necessary variables.
        /// </summary>
        private void Start()
        {
            InitializeGame();
            _actionLongestPathArray = new int[3];
            _shouldRecalculatePaths = true;
        }


        /// <summary>
        /// Updates the input direction based on the user's keyboard input.
        /// Recalculates the longest paths for all directions if needed.
        /// </summary>
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

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Time.timeScale = Time.timeScale == 0 ? 1 : 0; // Pause/unpause the game
            }

            // if (_shouldRecalculatePaths)
            // {
            //     var currentVisitedPath = new List<Vector2Int>();
            //     var currentVisitedPositions = new HashSet<Vector2Int>();
            //     CalculateLongestPathsForAllDirections(currentVisitedPath, currentVisitedPositions);
            //
            //
            //     _shouldRecalculatePaths = true;
            // }
        }

        #endregion

        /// <summary>
        /// Collects observations for the agent based on the current state of the game board.
        /// </summary>
        /// <param name="sensor">The sensor used to collect observations.</param>
        public override void CollectObservations(VectorSensor sensor)
        {
            // MaxPathLength = Mathf.Max(MaxPathLength, Board.Snakes[0].Length);
            const int view = 8; // Define view size as per your needs

            // Assuming that there is always at least one snake and one food in the game
            Snake snake = Board.Snakes[0];
            Vector2Int food = Board.FoodPositions[0];

            // Calculate direction to the food relative to the snake's heading
            Vector2 directionToFood = food - snake.Position;

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

            // CalculateLongestPathsForAllDirections();

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
                    Tile tile = Board.GetTile(i, j);

                    int cellValue;

                    if (cellPos.x < 0 || cellPos.y < 0 || cellPos.x >= Board.Width ||
                        cellPos.y >= Board.Height)
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


        /// <summary>
        /// Generates an action based on the user's input direction.
        /// </summary>
        /// <param name="actionsOut">The generated actions.</param>
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            Vector2Int currentDirection = Board.Snakes[0].Direction;
            int relativeDirection = GetRelativeDirection(currentDirection, _inputDirection);
            ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = relativeDirection;
        }


        /// <summary>
        /// Called when the agent receives an action.
        /// Handles the snake's movement and direction based on the received action.
        /// Clears the board, draws the food and snake, and checks for a new high score.
        /// Calculates the distance to the food and applies a reward or punishment based on the distance.
        /// Checks the snake's status and sets the flag to recalculate paths.
        /// </summary>
        /// <param name="actions">The received actions.</param>
        public override void OnActionReceived(ActionBuffers actions)
        {
            int action = actions.DiscreteActions[0];

            if (Board.Snakes.Length == 0)
            {
                return;
            }

            _eatTimer += 1;

            Snake snake = Board.Snakes[0];

            HandleSnakeDirection(action, snake);

            ProcessSnakeMovement(snake);

            Board.ClearBoard();
            Board.DrawFood(Board.FoodPositions);

            if (IsSnakeAlive(snake))
            {
                Board.DrawSnake(Board.Snakes[0]);
            }

            // show direction
            if (isDisplayOn)
            {
                boardDisplay.DrawBoard(Board);
            }

            CheckForNewHighScore(snake);


            // Get action's longest path from the array
            int actionLongestPath = _actionLongestPathArray[action];


            // If the longest path is shorter than the maximum, it's a bad action (blocked), apply punishment
            for (int i = 0; i < 3; i++)
            {
                var longestPath = _actionLongestPathArray[i];

                /*switch (i)
                {
                    case 0:
                        Debug.Log("Left action longest path: " + longestPath);
                        break;
                    case 1:
                        Debug.Log("Forward action longest path: " + longestPath);
                        break;
                    case 2:
                        Debug.Log("Right action longest path: " + longestPath);
                        break;
                }*/
            }


            if (actionLongestPath < MaxPathLength)
            {
                // Debug.Log("Bad action");
                AddReward(-1f);
                currentReward -= 1f;
            }
            // Good action, apply reward
            else
            {
                // Debug.Log("Good action");
                AddReward(0.1f);
                currentReward += 0.1f;
            }

            if (Board.FoodPositions.Count > 0)
            {
                Vector2Int currentTilePos = snake.Position;
                Vector2 foodPosition = Board.FoodPositions[0];

                //Euclidean distance to food from current position
                currentDistance = Vector2.Distance(currentTilePos, foodPosition);

                previousDistanceEditor = PreviousDistance;

                // Calculate the actual distance to the food

                // If the current distance is within the reward radius, apply the reward
                float normalizedCurrentDistance = currentDistance /
                                                  Mathf.Sqrt(
                                                          Board.Width * Board.Width +
                                                          Board.Height * Board.Height);
                float normalizedPreviousDistance = PreviousDistance /
                                                   Mathf.Sqrt(Board.Width * Board.Width +
                                                              Board.Height * Board.Height);
                // Compute the reward based on the current distance
                float reward = Mathf.Log((snake.Length + normalizedPreviousDistance) /
                                         (snake.Length + normalizedCurrentDistance));

                // normalize the reward between -1 and 1

                // Apply the reward
                AddReward(reward);
                // Debug.Log("Distance Reward :" + reward);
                currentReward += reward;


                /*if (currentDistance < PreviousDistance)
                {
                    AddReward(0.1f);
                    Debug.Log("Closer :" + 0.1f);
                    currentReward += 0.1f;
                }
                else if (currentDistance > PreviousDistance)
                {
                    AddReward(-0.1f);
                    currentReward -= 0.1f;
                    Debug.Log("Further :" + -0.1f);
                }*/

                PreviousDistance = currentDistance;
            }

            CheckSnakeStatus(snake);

            _shouldRecalculatePaths = true;
        }

        /// <summary>
        /// Called when the agent starts a new episode.
        /// Resets the game and initializes the current reward to 0.
        /// If the number of episodes exceeds the reset average count, the total score and reward are reset.
        /// </summary>
        public override void OnEpisodeBegin()
        {
            if (episodeCount > resetAverageCount)
            {
                totalScore = 0.0;
                totalReward = 0.0;
                episodeCount = 0;
            }

            // MaxPathLength = 0;
            Debug.Log("MaxPathLength: " + MaxPathLength);
            // _shouldRecalculatePaths = true;

            _eatTimer = 0;
            episodeCount++;
            InitializeGame();
            currentReward = 0;
        }

        /*private void CalculateLongestPathsForAllDirections()
        {
            Snake snake = Board.Snakes[0];

            for (int action = 0; action < 3; ++action)
            {
                Vector2Int actionDirection = action switch
                {
                    0 => RotateCounterClockwise(snake.Direction),
                    1 => snake.Direction,
                    2 => RotateClockwise(snake.Direction),
                    _ => snake.Direction,
                };

                var position = snake.Position;
                var visited = new HashSet<Vector2Int>();
                var currentPath = new List<Vector2Int>();
                var currentPathLength = 0;
                int longestPath = BreadthFirstSearch(position, visited, MaxPathLength,
                                                     currentPath, currentPathLength);
                _actionLongestPathArray[action] = longestPath;
            }

            // Debug.Log("Longest path for left action: " + _actionLongestPathArray[0]);
        }*/

        /// <summary>
        /// Checks if the current score is higher than the high score and updates it if necessary.
        /// </summary>
        /// <param name="snake">The snake to check the score for.</param>
        private void CheckForNewHighScore(Snake snake)
        {
            // update high score if current score is higher
            if (Board.Snakes[0].Score > _highScore)
            {
                _highScore = snake.Score;
            }

            highScoreText.text = "High Score: " + _highScore;

            scoreText.text = "Score: " + Board.Snakes[0].Score;
        }


        /// <summary>
        /// Checks the status of the given snake and updates the reward accordingly.
        /// </summary>
        /// <param name="snake">The snake to check.</param>
        private void CheckSnakeStatus(Snake snake)
        {
            if (snake.AteFood)
            {
                Debug.Log("Eat : " + 1f);
                currentReward += 1f;
                AddReward(1f);
                snake.AteFood = false;
                _eatTimer = 0;
            }

            // AddReward(-0.1f);
            // currentReward += -0.1f;

            if (_eatTimer > maxHungryTime)
            {
                Debug.Log("Starved");
                snake.IsAlive = false;
            }

            if (!IsSnakeAlive(snake))
            {
                Debug.Log("Dead : " + -1f);
                currentReward -= 1f;
                AddReward(-1f);
                totalScore += Board.Snakes[0].Score;
                totalReward += currentReward;
                averageScore = totalScore / episodeCount;
                averageReward = totalReward / episodeCount;
                EndEpisode();
            }

            currentRewardText.text = "Current Reward:\n" + currentReward.ToString("F2");
            averageRewardText.text = "Average Reward:\n" + averageReward.ToString("F2");
            averageScoreText.text = "Average Score:\n" + averageScore.ToString("F2");
        }


        /// <summary>
        /// Gets the relative direction of the input direction relative to the current direction.
        /// </summary>
        /// <param name="currentDirection">The current direction.</param>
        /// <param name="inputDirection">The input direction.</param>
        /// <returns>The relative direction of the input direction relative to the current direction.</returns>
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
        /// <summary>
        /// Handles the snake's direction based on the given action.
        /// </summary>
        /// <param name="action">The action to take.</param>
        /// <param name="snake">The snake to handle.</param>
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


        /// <summary>
        /// Initializes the game by creating snakes, resetting the board, and spawning food.
        /// </summary>
        private void InitializeGame()
        {
            _snakeController = new SnakeController();
            Snake[] snakes =
                    _snakeController.CreateSnakes(width, height, numberOfSnakes, startSize);

            if (Board != null)
            {
                Board.Snakes = snakes;
                Board.Reset(snakes, width, height);

                if (isDisplayOn)
                {
                    boardDisplay.Reset();
                }
            }
            else
            {
                Board = new Board(width, height, snakes);
            }

            PreviousDistance = MaxDistance;

            // Spawn food
            for (int i = 0; i < foodCount; i++)
            {
                Board.SpawnFood();
            }
        }
        public float MaxDistance { get; set; }
        public int MaxPathLength
        {
            get => maxPathLength;
            set => maxPathLength = value;
            // {
            //     var snake = Board.Snakes[0];
            //     maxPathLength = Mathf.Max(value, snake.Length);
            // }
        }
        public Board Board
        {
            get => board;
            set => board = value;
        }

        /// <summary>
        ///     Determines if a given position on the game board is safe.
        /// </summary>
        /// <param name="position" cref="Scripts.Board">The position to check.</param>
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

            if (Board.Tiles[position.x, position.y].Type == TileType.Snake)
            {
                return false;
            }


            // If passed both conditions, the position is safe.
            return true;
        }

        private static bool IsSnakeAlive(Snake snake) => snake.IsAlive;

        private void ProcessSnakeMovement(Snake snake)
        {
            _snakeController.FinalizeDirection(snake);
            _snakeController.CheckCollisions(Board);

            _snakeController.Move(Board, Board.Snakes[0], false);
        }

        private Vector2Int RotateClockwise(Vector2Int direction) => new(direction.y, -direction.x);

        private Vector2Int RotateCounterClockwise(Vector2Int direction) =>
                new(-direction.y, direction.x);
    }
}
