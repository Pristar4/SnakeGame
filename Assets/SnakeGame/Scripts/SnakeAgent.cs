using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SnakeGame.Scripts {
    public class SnakeAgent : Agent {
        #region Serialized Fields

        [SerializeField] private BoardDisplay boardDisplay;
        [Range(5, 100)] [SerializeField] private int width = 10;
        [Range(5, 100)] [SerializeField] private int height = 10;

        [Tooltip("The time in seconds between each move")] [SerializeField]
        private float turnDuration = 1;

        [SerializeField] private SnakeController[] snakes;

        [SerializeField] private int snakeNumber = 2;
        [SerializeField] private GameObject snakeControllerPrefab;

        [SerializeField] private List<Player> players;



        [SerializeField] private TMP_Text scoreText;

        #endregion

        private Board _board;
        private float _currentTimer;
        [SerializeField] private bool wrapIsEnabled;
        [SerializeField] private int foodCount = 1;

        private void Start() {
            _board = new Board(width, height);
            snakes = new SnakeController[snakeNumber];
            CreateSnakeControllers(snakeNumber);

            // food
            for (int i = 0; i < foodCount; i++) {
                _board.FoodPositions.Add(_board.SpawnFood());
                
            }
        }


        public override void OnEpisodeBegin() {
            Debug.Log("Episode Begin");
        }
        
        private void Update() {
            _currentTimer += Time.deltaTime;

            if (!IsSnakeAlive(snakes[0])) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                
                
            }


            foreach (var player in players) {
                if (snakes.Length <= player.snakeId) {
                    Debug.LogError("Player snake id is out of range");
                    continue;
                }

                var snake = snakes[player.snakeId];

                var inputDirection = InputController.HandleInput(snake.Snake.Direction, player.inputSchemer);

                if (inputDirection != Vector2Int.zero)
                    snake.NextDirection = inputDirection;
            }


            if (_currentTimer < turnDuration) return;
            _currentTimer = 0;


            // check for collisions
            foreach (var snakeController in snakes) {
                if (IsSnakeAlive(snakeController)) {
                    snakeController.FinalizeDirection();
                    snakeController.CheckCollisions(_board);
                }
            }

            _board.ClearBoard();
            _board.DrawFood(_board.FoodPositions);

            foreach (var snakeController in snakes) {
                if (IsSnakeAlive(snakeController)) {
                    snakeController.Move(_board, snakeController.Snake, wrapIsEnabled);
                    _board.DrawSnake(snakeController.Snake);
                }
            }

            // draw the board
            boardDisplay.DrawBoard(_board);
            //update score
            scoreText.text = "Player 1 Score: " + snakes[0].Snake.Score;
        }

        private static bool IsSnakeAlive(SnakeController snakeController) {
            return snakeController.Snake.IsAlive;
        }

        private void CreateSnakeControllers(int number) {
            for (int i = 0; i < number; i++) {
                var snakeControllerObj = Instantiate(snakeControllerPrefab);
                var snakeController = snakeControllerObj.GetComponent<SnakeController>();

                var startSpawnPosition = new Vector2Int(width / 2, height / 2);
                startSpawnPosition += Vector2Int.right * i * 10;

                snakes[i] = snakeController;
                snakeController.InitializeSnakeProperties(startSpawnPosition, Vector2Int.up, 5, i);
            }
        }


        public override void CollectObservations(VectorSensor sensor) {
            Debug.Log("Collecting Observations");
            var testVector = new Vector2Int(0, 0);
            sensor.AddObservation(testVector);
          
        }

        public override void OnActionReceived(ActionBuffers actions) {
            Debug.Log("Action Received");
            // up = 0, right = 1, down = 2, left = 3
            var action = actions.DiscreteActions[0];

            switch (action) {
                case 0:
                    snakes[0].NextDirection = new Vector2Int(0, 1);
                    break;
                case 1:
                    snakes[0].NextDirection = new Vector2Int(1, 0);
                    break;
                case 2:
                    snakes[0].NextDirection = new Vector2Int(0, -1);
                    break;
                case 3:
                    snakes[0].NextDirection = new Vector2Int(-1, 0);
                    break;
            }

            snakes[0].FinalizeDirection();
            snakes[0].CheckCollisions(_board);

            snakes[0].Move(_board, snakes[0].Snake, false);
            _board.DrawSnake(snakes[0].Snake);

            if (snakes[0].Snake.IsAlive) {
                AddReward(0.1f);
            } else {
                AddReward(-1f);
                EndEpisode();
            }

            _board.ClearBoard();
            _board.DrawFood(_board.FoodPositions);

            boardDisplay.DrawBoard(_board);


            scoreText.text = "Player 1 Score: " + snakes[0].Snake.Score;
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            Debug.Log("Heuristic");
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