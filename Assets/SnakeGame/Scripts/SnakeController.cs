using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SnakeGame.Scripts {
    public class SnakeController : MonoBehaviour {
        private Snake _snake;
        [SerializeField] private Vector2Int _nextDirection;

        public SnakeController
                InitializeSnakeProperties(Vector2Int position, Vector2Int direction, int length, int id) {
            Debug.Log("SnakeController InitializeSnakeProperties");
            _snake = new Snake(position, direction, length, id, new Vector2Int[length]);
            _nextDirection = direction;

            for (int i = 0; i < length; i++) {
                _snake.Body[i] = position - direction * i;
            }

            return this;
        }


        public Snake Snake => _snake;


        public void HandleInputControls() {
            Vector2Int up = Vector2Int.up; 
            Vector2Int down = Vector2Int.down;
            Vector2Int left = Vector2Int.left;
            Vector2Int right = Vector2Int.right;
            Vector2Int currentDirection = _snake.Direction;



            if (Keyboard.current.upArrowKey.wasPressedThisFrame && currentDirection != down) {
                _nextDirection = up;
            } else if (Keyboard.current.downArrowKey.wasPressedThisFrame && currentDirection != up) {
                _nextDirection = down;
            } else if (Keyboard.current.leftArrowKey.wasPressedThisFrame && currentDirection != right) {
                _nextDirection = left;
            } else if (Keyboard.current.rightArrowKey.wasPressedThisFrame && currentDirection != left) {
                _nextDirection = right;
            }
        }


        public void Move(Board board, Snake snake) {
            //Check Collisions
            snake.CheckCollisions(board);
            
            _snake.Direction = _nextDirection; // Update the direction of the Snake
            var nextPosition = snake.Position + _nextDirection;


            if (nextPosition.x < 0 || nextPosition.x >= board.Width || nextPosition.y < 0 ||
                nextPosition.y >= board.Height) {
                // make the snake wrap around the board
                if (nextPosition.x < 0) {
                    snake.Position = new Vector2Int(board.Width - 1, snake.Position.y);
                } else if (nextPosition.x >= board.Width) {
                    snake.Position = new Vector2Int(0, snake.Position.y);
                } else if (nextPosition.y < 0) {
                    snake.Position = new Vector2Int(snake.Position.x, board.Height - 1);
                } else if (nextPosition.y >= board.Height) {
                    snake.Position = new Vector2Int(snake.Position.x, 0);
                }
            } else {
                snake.Position = nextPosition;
            }

// update the body
            for (int i = snake.Length - 1; i > 0; i--) {
                snake.Body[i] = snake.Body[i - 1];
            }

            snake.Body[0] = snake.Position;
        }
    }
}