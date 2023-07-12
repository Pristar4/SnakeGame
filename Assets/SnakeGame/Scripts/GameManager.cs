#region

using System;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace SnakeGame.Scripts
{
    public class GameManager : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private BoardDisplay boardDisplay;
        [Range(5, 100)] [SerializeField] private int width = 10;
        [Range(5, 100)] [SerializeField] private int height = 10;

        [Tooltip("The time in seconds between each move")] [SerializeField]
        private float turnDuration = 1;

        [SerializeField] private int snakeNumber = 2;
        [SerializeField] private GameObject snakeControllerPrefab;

        [SerializeField] private List<Player> players;

        [Header("Cinemachine")] [SerializeField]
        private CinemachineVirtualCamera virtualCamera;


        [SerializeField] private TMP_Text player1ScoreText;
        [SerializeField] private TMP_Text player2ScoreText;
        [SerializeField] private bool wrapIsEnabled;
        [SerializeField] private int foodCount = 1;

        #endregion

        private Board _board;
        private float _currentTimer;

        [SerializeField] private SnakeController[] snakes;

        #region Event Functions

        private void Start()
        {
            _board = new Board(width, height);
            virtualCamera.m_Lens.OrthographicSize = Mathf.Max(width, height) / 2f;
            snakes = new SnakeController[snakeNumber];
            CreateSnakeControllers(snakeNumber);

            // food
            for (int i = 0; i < foodCount; i++)
            {
                _board.FoodPositions.Add(_board.SpawnFood());
            }
        }

        private void Update()
        {
            _currentTimer += Time.deltaTime;

            if (!IsSnakeAlive(_board.Snakes[0]))
                    //reload the scene
            {
                SceneManager.LoadScene(0);
            }


            foreach (Player player in players)
            {
                if (snakes.Length <= player.snakeId)
                {
                    Debug.Log("<color=red>Player snake id is out of range</color>");
                    continue;
                }

                Snake snake = _board.Snakes[0];

                Vector2Int inputDirection =
                        InputController.HandleInput(snake.Direction, player.inputSchemer);

                if (inputDirection != Vector2Int.zero)
                {
                    snake.NextDirection = inputDirection;
                }
            }


            if (_currentTimer < turnDuration)
            {
                return;
            }

            _currentTimer = 0;


            // check for collisions
            foreach (SnakeController snakeController in snakes)
            {
                if (IsSnakeAlive(_board.Snakes[0]))
                {
                    snakeController.FinalizeDirection(_board.Snakes[0]);
                    snakeController.CheckCollisions(_board);
                }
            }

            _board.ClearBoard();
            _board.DrawFood(_board.FoodPositions);

            foreach (SnakeController snakeController in snakes)
            {
                if (IsSnakeAlive(_board.Snakes[0]))
                {
                    snakeController.Move(_board, _board.Snakes[0], wrapIsEnabled);
                    _board.DrawSnake(_board.Snakes[0]);
                }
            }

            // draw the board
            boardDisplay.DrawBoard(_board);

            //update score
            if (snakes.Length > 0)
            {
                player1ScoreText.text = "Player 1 Score: " + _board.Snakes[0].Score;
            }

            if (snakes.Length > 1)
            {
                player2ScoreText.text = "Player 2 Score: " + _board.Snakes[1].Score;
            }
        }

        #endregion

        private void CreateSnakeControllers(int number)
        {
            for (int i = 0; i < number; i++)
            {
                GameObject snakeControllerObj = Instantiate(snakeControllerPrefab);
                SnakeController snakeController =
                        snakeControllerObj.GetComponent<SnakeController>();

                Vector2Int startSpawnPosition = new(width / 2, height / 2);
                startSpawnPosition += Vector2Int.right * i * 10;

                snakes[i] = snakeController;
                SnakeController.InitializeSnakeBody(_board.Snakes[0], startSpawnPosition,
                                                    Vector2Int.up, 5);
            }
        }

        private static bool IsSnakeAlive(Snake snake) => snake.IsAlive;
    }

    [Serializable]
    public struct Player
    {
        #region Serialized Fields

        public int snakeId;
        public InputSchemer inputSchemer;

        #endregion
    }
}
