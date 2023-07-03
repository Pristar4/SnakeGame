using System;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SnakeGame.Scripts {
    public class GameManager : MonoBehaviour {
        private Board _board;
        [SerializeField] private BoardDisplay boardDisplay;
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private bool wantsToDrawBoard = true;

        [Tooltip("The time in seconds between each move")] [SerializeField]
        private float turnDuration = 1;
        private float currentTimer = 0;

        [SerializeField] private SnakeController[] snakeControllers;

        [SerializeField] private int snakeNumber = 2;
        [SerializeField] private GameObject snakeControllerPrefab;

        private void Start() {
            _board = new Board(width, height);
            snakeControllers = new SnakeController[snakeNumber];
            CreateSnakeControllers();
        }

        private void CreateSnakeControllers() {
            for (int i = 0; i < snakeNumber; i++) {
                var snakeControllerObj = Instantiate(snakeControllerPrefab);
                SnakeController snakeController = snakeControllerObj.GetComponent<SnakeController>();

                Vector2Int startSpawnPosition = new Vector2Int(width / 2, height / 2);
                startSpawnPosition += Vector2Int.right * i * 2;

                snakeControllers[i] = snakeController;
                snakeController.InitializeSnakeProperties(startSpawnPosition, Vector2Int.up, 5, i);
            }
        }

        private void FixedUpdate() {
            // move the snakes
            foreach (var snakeController in snakeControllers) {
                
                
                snakeController.Move(_board, snakeController.Snake);
                _board.UpdateBoard(snakeController.Snake);
            }

            boardDisplay.DrawBoard(_board);
        }

        private void Update() {
            currentTimer += Time.deltaTime;

            // handle input
             snakeControllers[0].HandleInputControls();

            if (currentTimer >= turnDuration) {
                currentTimer = 0;


                wantsToDrawBoard = true;
            }

            // draw the board
            if (wantsToDrawBoard) {
                wantsToDrawBoard = false;
                boardDisplay.DrawBoard(_board);
            }
        }
    }
}