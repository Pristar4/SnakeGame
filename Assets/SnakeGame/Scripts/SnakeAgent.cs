using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

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

        #endregion

        private float _currentTimer;
        private int _highScore;


        private SnakeController _snakeController;

        #region Event Functions

        private void Start() {
            InitializeGame();
        }

        #endregion

        private void InitializeGame() {
            _snakeController = new SnakeController();
            var snakes = _snakeController.CreateSnakes(width, height, numberOfSnakes, startSize);

            if (board != null) {
                board.Snakes = snakes;
                board.Reset(snakes, width, height);
                boardDisplay.Reset();
            } else {
                board = new Board(width, height, snakes);
            }


            // food
            for (int i = 0; i < foodCount; i++) {
                board.FoodPositions.Add(board.SpawnFood());
            }
        }


        public override void OnEpisodeBegin() {
            Debug.Log("Episode Begin");
            InitializeGame();
        }


        private static bool IsSnakeAlive(Snake snake) {
            return snake.IsAlive;
        }


        public override void CollectObservations(VectorSensor sensor) {
            if (board.Snakes.Length > 0) {
                sensor.AddObservation(board.Snakes[0].Direction);
                sensor.AddObservation(board.Snakes[0].Position);
                sensor.AddObservation(board.FoodPositions[0]);
                var array = board.GetBoardAsArray();

                for (int i = 0; i < array.Length; i++) {
                    sensor.AddObservation(array[i]);
                }
            } else {
                sensor.AddObservation(Vector2.zero);
                sensor.AddObservation(Vector2.zero);
                sensor.AddObservation(Vector2.zero);

                var array = board.GetBoardAsArray();

                for (int i = 0; i < array.Length; i++) {
                    sensor.AddObservation(-1);
                }
            }

            // 1d array of the board
            // var boardArray = _board.GetBoardArray();
        }

        public override void OnActionReceived(ActionBuffers actions) {
            // Debug.Log("Action Received");
            // up = 0, right = 1, down = 2, left = 3
            int action = actions.DiscreteActions[0];

            if (board.Snakes.Length == 0) {
                return;
            }

            var snake = board.Snakes[0];



            Debug.Log(action);

            switch (action) {
                case 0:
                    snake.NextDirection = new Vector2Int(0, 1);
                    break;
                case 1:
                    snake.NextDirection = new Vector2Int(1, 0);
                    break;
                case 2:
                    snake.NextDirection = new Vector2Int(0, -1);
                    break;
                case 3:
                    snake.NextDirection = new Vector2Int(-1, 0);
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


            boardDisplay.DrawBoard(board);

            // update high score if current score is higher
            if (board.Snakes[0].Score > _highScore) {
                _highScore = snake.Score;
            }

            highScoreText.text = "High Score: " + _highScore;


            scoreText.text = "Score: " + board.Snakes[0].Score;

            if (board.Snakes[0].AteFood) {
                Debug.Log("Reward : " + 10f);
                AddReward(10f);
                board.Snakes[0].AteFood = false;
            }

            if (IsSnakeAlive(snake)) {
                Debug.Log("Reward : " + -0.01f);
                AddReward(-0.01f);
            } else if (!IsSnakeAlive(snake)) {
                Debug.Log("Reward : " + -5f);
                AddReward(-10f);
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            // Debug.Log("Heuristic");
            // wasd controls

            var discreteActionsOut = actionsOut.DiscreteActions;

            discreteActionsOut[0] = 0;

            if (Keyboard.current.wKey.wasPressedThisFrame) {
                discreteActionsOut[0] = 0;
            } else if (Keyboard.current.dKey.wasPressedThisFrame) {
                discreteActionsOut[0] = 1;
            } else if (Keyboard.current.sKey.wasPressedThisFrame) {
                discreteActionsOut[0] = 2;
            } else if (Keyboard.current.aKey.wasPressedThisFrame) {
                discreteActionsOut[0] = 3;
            }
        }
    }
}