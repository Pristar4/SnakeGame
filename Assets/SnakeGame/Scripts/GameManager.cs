using System;
using System.Collections.Generic;
using UnityEngine;

namespace SnakeGame.Scripts {
    public class GameManager : MonoBehaviour {
        #region Serialized Fields

        [SerializeField] private BoardDisplay boardDisplay;
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;

        [Tooltip("The time in seconds between each move")] [SerializeField]
        private float turnDuration = 1;

        [SerializeField] private SnakeController[] snakes;

        [SerializeField] private int snakeNumber = 2;
        [SerializeField] private GameObject snakeControllerPrefab;

        [SerializeField] private List<Player> players;

        #endregion

        private Board _board;
        private float _currentTimer;

        #region Event Functions

        private void Start() {
            _board = new Board(width, height);
            snakes = new SnakeController[snakeNumber];
            CreateSnakeControllers(snakeNumber);

            // food
            _board.FoodPositions.Add(_board.SpawnFood());
        }

        private void Update() {
            _currentTimer += Time.deltaTime;

            foreach (var player in players) {
                if (snakes.Length <= player.snakeId) {
                    Debug.LogError("Player snake id is out of range");
                    continue;
                }

                var snake = snakes[player.snakeId];

                var inputDirection = InputController.HandleInput(snake.Snake.Direction, player.inputSchemer);

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
                    snakeController.Move(_board, snakeController.Snake);
                    _board.DrawSnake(snakeController.Snake);
                }
            }

            // draw the board
            boardDisplay.DrawBoard(_board);
        }

        #endregion

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

        private static bool IsSnakeAlive(SnakeController snakeController) {
            return snakeController.Snake.IsAlive;
        }
    }

    [Serializable]
    public struct Player {
        #region Serialized Fields

        public int snakeId;
        public InputSchemer inputSchemer;

        #endregion
    }
}