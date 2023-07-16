#region

using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace SnakeGame.Scripts
{
    /// <summary>
    ///     The SnakeAgent class is responsible for controlling the behavior of the <see cref="Snake" /> in
    ///     the game.
    ///     It contains methods for finding the longest path,
    ///     getting the relative direction of the input direction relative to the current direction,
    ///     handling the snake's direction based on the given action, initializing the game by creating
    ///     snakes,
    ///     resetting the <see cref="Scripts.Board" />, and spawning food, determining if a given position
    ///     on the game <see cref="Scripts.Board" /> is safe,
    ///     processing snake movement, and rotating the snake's direction clockwise and counterclockwise.
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
        [Range(2, 100)] [SerializeField] private int width;
        [Range(2, 100)] [SerializeField] private int height;
        [Range(1, 10)] [SerializeField] private int maxPathLength;
        [SerializeField] private int numberOfSnakes;
        [SerializeField] private int startSize;
        [SerializeField] private int maxHungryTime = 64;
        [SerializeField] private int resetAverageCount = 250;
        [SerializeField] private int eatTimer;
        [SerializeField] private int episodeCount;

        #endregion

        private readonly Vector2Int[] _directions =
        {
            new(0, 1),  // up
            new(1, 0),  // right
            new(0, -1), // down
            new(-1, 0), // left
        };


        private float _currentTimer;
        private int _highScore;

        private Vector2Int
                _inputDirection = Vector2Int.up; // keeps track of the last input direction

        private int _longestPath;
        private SnakeController _snakeController;

        private double _averageReward;
        private double _averageScore;
        private float _currentDistance;
        private double _currentReward;
        private double _totalReward;
        private double _totalScore;

        private StatsRecorder stats;
        private float _episodeStarted;

        private float PreviousDistance { get; set; }
        public float MaxDistance { get; set; }
        public int MaxPathLength
        {
            get => maxPathLength;
            set => maxPathLength = value;
        }
        public Board Board
        {
            get => board;
            set => board = value;
        }
        public int HighScore
        {
            set { _highScore = value; }
            get { return _highScore; }
        }
        public StatsRecorder Stats
        {
            set { stats = value; }
            get { return stats; }
        }
        public TMP_Text HighScoreText
        {
            set { highScoreText = value; }
            get { return highScoreText; }
        }
        public TMP_Text ScoreText
        {
            set { scoreText = value; }
            get { return scoreText; }
        }
        public double CurrentReward
        {
            set { _currentReward = value; }
            get { return _currentReward; }
        }
        public StatsRecorder Stats1
        {
            set { stats = value; }
            get { return stats; }
        }
        public float EpisodeStarted
        {
            set { _episodeStarted = value; }
            get { return _episodeStarted; }
        }
        public int EatTimer
        {
            set { eatTimer = value; }
            get { return eatTimer; }
        }
        public int MaxHungryTime
        {
            set { maxHungryTime = value; }
            get { return maxHungryTime; }
        }
        public double TotalScore
        {
            set { _totalScore = value; }
            get { return _totalScore; }
        }
        public double TotalReward
        {
            set { _totalReward = value; }
            get { return _totalReward; }
        }
        public double AverageScore
        {
            set { _averageScore = value; }
            get { return _averageScore; }
        }
        public int EpisodeCount
        {
            set { episodeCount = value; }
            get { return episodeCount; }
        }
        public double AverageReward
        {
            set { _averageReward = value; }
            get { return _averageReward; }
        }
        public TMP_Text CurrentRewardText
        {
            set { currentRewardText = value; }
            get { return currentRewardText; }
        }
        public TMP_Text AverageRewardText
        {
            set { averageRewardText = value; }
            get { return averageRewardText; }
        }
        public TMP_Text AverageScoreText
        {
            set { averageScoreText = value; }
            get { return averageScoreText; }
        }
        public Board Board1
        {
            set { board = value; }
            get { return board; }
        }
        public int EatTimer1
        {
            set { eatTimer = value; }
            get { return eatTimer; }
        }
        public int Height
        {
            set { height = value; }
            get { return height; }
        }
        public int Width
        {
            set { width = value; }
            get { return width; }
        }



        #region Event Functions

        /// <summary>
        ///     Initializes the game and sets up the necessary variables.
        /// </summary>
        public override void Initialize()
        {
            InitializeGame();
            stats = Academy.Instance.StatsRecorder;


            MaxDistance = Mathf.Sqrt(Board.Height * Board.Height + Board.Width * Board.Width);
        }

        public void InitializeGame()
        {
            RandomizeBoardSize();
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

        private void RandomizeBoardSize()
        {
            int minSize = 8 / 2;
            int maxSize =
                    18 / 2; // go up to 34 because the Random.Range method is exclusive of the upper bound

            // width = Random.Range(minSize, maxSize) * 2;

            int size = Random.Range(minSize, maxSize) * 2;
            Height = size;
            Width = size;
        }


        /// <summary>
        ///     Updates the input direction based on the user's keyboard input.
        ///     Recalculates the longest paths for all directions if needed.
        /// </summary>
        // Disabled for Training

#if UNITY_EDITOR
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
        }
#endif

        #endregion

        /// <summary>
        ///     Collects observations for the agent based on the current state of the game board.
        /// </summary>
        /// <param name="sensor">The sensor used to collect observations.</param>
        public override void CollectObservations(VectorSensor sensor)
        {
            int view = 8;

            Snake snake = Board.Snakes[0];
            Vector2Int food = snake.Position;

            if (Board.FoodPositions.Count >= 1)
            {
                food = Board.FoodPositions[0];
            }


            Vector2 directionToFood = food - snake.Position;
            directionToFood = directionToFood.normalized;

            sensor.AddObservation(directionToFood);
            Vector2Int windowStart = snake.Position - new Vector2Int(view / 2, view / 2);

            Vector2Int forwardPosition = snake.Position + snake.Direction;
            Vector2Int leftPosition = snake.Position + RotateCounterClockwise(snake.Direction);
            Vector2Int rightPosition = snake.Position + RotateClockwise(snake.Direction);

            sensor.AddObservation(Board.IsPositionSafe(forwardPosition));
            sensor.AddObservation(Board.IsPositionSafe(leftPosition));
            sensor.AddObservation(Board.IsPositionSafe(rightPosition));


            for (int i = 0; i < view; i++)
            {
                for (int j = 0; j < view; j++)
                {
                    Vector2Int cellPos = windowStart + new Vector2Int(i, j);

                    if (!Board.IsOutOfBounds(cellPos))
                    {
                        Tile tile = Board.GetTile(cellPos.x, cellPos.y);
                        sensor.AddObservation((int)tile.Type);
                    }
                    else
                    {
                        sensor.AddObservation((int)TileType.Wall);
                    }
                }
            }
        }


        /// <summary>
        ///     Generates an action based on the user's input direction.
        /// </summary>
        /// <param name="actionsOut">The generated actions.</param>
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            Vector2Int currentDirection = Board.Snakes[0].Direction;
            int relativeDirection =
                    _snakeController.GetRelativeDirection(currentDirection, _inputDirection, this);
            ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = relativeDirection;
        }


        /// <summary>
        ///     Called when the agent receives an action.
        ///     Handles the snake's movement and direction based on the received action.
        ///     Clears the board, draws the food and snake, and checks for a new high score.
        ///     Calculates the distance to the food and applies a reward or punishment based on the distance.
        ///     Checks the snake's status and sets the flag to recalculate paths.
        /// </summary>
        /// <param name="actions">The received actions.</param>
        public override void OnActionReceived(ActionBuffers actions)
        {
            int action = actions.DiscreteActions[0];

            if (Board.Snakes.Length == 0)
            {
                return;
            }

            eatTimer += 1;

            Snake snake = Board.Snakes[0];
            _snakeController.HandleSnakeDirection(action, snake, this);

            _snakeController.ProcessSnakeMovement(snake, this);

            Board.ClearBoard();
            Board.DrawFood(Board.FoodPositions);

            if (Board.IsSnakeAlive(snake))
            {
                Board.DrawSnake(Board.Snakes[0]);
            }

            // show direction
            if (isDisplayOn)
            {
                boardDisplay.DrawBoard(Board);
            }


            _snakeController.CheckForNewHighScore(snake, this);


            if (Board.FoodPositions.Count > 0)
            {
                Vector2Int currentTilePos = snake.Position;
                Vector2 foodPosition = Board.FoodPositions[0];

                //Euclidean distance to food from current position
                _currentDistance = Vector2.Distance(currentTilePos, foodPosition);


                // Calculate the actual distance to the food

                // If the current distance is within the reward radius, apply the reward
                float normalizedCurrentDistance = _currentDistance /
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
                _currentReward += reward;


                PreviousDistance = _currentDistance;
            }

            _snakeController.CheckSnakeStatus(snake, this);
        }

        /// <summary>
        ///     Called when the agent starts a new episode.
        ///     Resets the game and initializes the current reward to 0.
        ///     If the number of episodes exceeds the reset average count, the total score and reward are
        ///     reset.
        /// </summary>
        public override void OnEpisodeBegin()
        {
            _episodeStarted = Time.fixedUnscaledTime;

            if (episodeCount > resetAverageCount)
            {
                _totalScore = 0.0;
                _totalReward = 0.0;
                episodeCount = 0;
            }


            eatTimer = 0;
            episodeCount++;
            InitializeGame();
            _currentReward = 0;
        }

        /// <summary>
        ///     Recursively finds the longest path from the given position, without visiting any position more
        ///     than once.
        /// </summary>
        /// <param name="position">The starting position.</param>
        /// <param name="visitedPositions">The set of visited positions.</param>
        /// <param name="maxLength">The maximum length of the path.</param>
        /// <param name="currentPath"></param>
        /// <param name="currentLength">The current length of the path.</param>
        /// <returns>The length of the longest path.</returns>
        private int BreadthFirstSearch(Vector2Int position, HashSet<Vector2Int> visitedPositions,
                                       int maxLength, List<Vector2Int> currentPath,
                                       int currentLength = 0)
        {
            if (!Board.IsPositionSafe(position) || currentLength > maxLength ||
                visitedPositions.Contains(position))
            {
                return -1;
            }

            visitedPositions.Add(position);
            currentPath.Add(position);

            // If this tile is empty, set it to be a path tile
            if (Board.Tiles[position.x, position.y].Type == TileType.Empty)
            {
                Board.Tiles[position.x, position.y].Type = TileType.Path;
            }

            // Exit if the path length equals maxPathLength
            if (currentLength == maxLength)
            {
                visitedPositions.Remove(position);
                currentPath.RemoveAt(currentPath.Count - 1);
                return currentLength;
            }

            int longestPathLength = currentLength;
            int possiblePaths = 0;


            foreach (Vector2Int direction in _directions)
            {
                Vector2Int nextPosition = position + direction;

                int nextPathLength = BreadthFirstSearch(nextPosition, visitedPositions, maxLength,
                                                        currentPath, currentLength + 1);

                if (nextPathLength > longestPathLength)
                {
                    longestPathLength = nextPathLength;
                }

                if (nextPathLength >= 0)
                {
                    possiblePaths++;
                }
            }

            visitedPositions.Remove(position);
            currentPath.RemoveAt(currentPath.Count - 1);

            if (possiblePaths == 0 && longestPathLength < maxLength)
            {
                SetEnclosedSpacesAsBlocked(visitedPositions);
            }

            return longestPathLength;
        }
        /// <summary>
        ///     Calculates the longest paths for all possible directions the snake can move in.
        /// </summary>
        /// <summary>
        ///     Calculates the longest paths for all possible directions the snake can move in.
        /// </summary>
        private void CalculateLongestPaths(List<Vector2Int> currentVisitedPath,
                                           HashSet<Vector2Int> currentVisitedPositions)
        {
            foreach (Vector2Int direction in _directions)
            {
                Vector2Int nextPosition = Board.Snakes[0].Position + direction;

                // Check if the next position is safe
                if (!Board.IsPositionSafe(nextPosition))
                {
                    continue;
                }

                HashSet<Vector2Int> visitedPositions = new(currentVisitedPositions);
                List<Vector2Int> currentPath = new(currentVisitedPath);

                int pathLength =
                        BreadthFirstSearch(nextPosition, visitedPositions, MaxPathLength,
                                           currentPath);

                // Check if the path is valid
                if (pathLength >= 0)
                {
                    // Update the grid with the path
                    UpdateGridWithPath(currentPath);

                    // Check if the direction moves into an enclosed space
                    int openSpaceCount = 0;

                    openSpaceCount = CountOpenSpaces(visitedPositions, openSpaceCount);

                    int threshold = 3;

                    if (openSpaceCount < threshold)
                    {
                        SetEnclosedSpacesAsBlocked(visitedPositions);
                    }
                }
            }
        }


        private int CountOpenSpaces(HashSet<Vector2Int> visitedPositions, int openSpaceCount)
        {
            foreach (Vector2Int position in visitedPositions)
            {
                if (Board.Tiles[position.x, position.y].Type == TileType.Path)
                {
                    openSpaceCount++;
                }
            }

            return openSpaceCount;
        }


        public Vector2Int RotateClockwise(Vector2Int direction) => new(direction.y, -direction.x);

        public Vector2Int RotateCounterClockwise(Vector2Int direction) =>
                new(-direction.y, direction.x);
        private void SetEnclosedSpacesAsBlocked(HashSet<Vector2Int> visitedPositions)
        {
            foreach (Vector2Int position in visitedPositions)
            {
                if (Board.Tiles[position.x, position.y].Type == TileType.Path)
                {
                    Board.Tiles[position.x, position.y].Type = TileType.Blocked;
                }
            }
        }
        private void UpdateGridWithPath(List<Vector2Int> currentPath)
        {
            foreach (Vector2Int pathPosition in currentPath)
            {
                if (Board.IsFood(pathPosition.x, pathPosition.y)) // Do not override Food tile type
                {
                    Board.SetTile(pathPosition.x, pathPosition.y, TileType.Path);
                }
            }
        }
    }
}
