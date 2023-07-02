using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Snake.Scripts {
    public class SnakeController : MonoBehaviour {
        
        private readonly Snake _snake;


        public SnakeController(Vector2Int position, Vector2Int direction, int length, int id) {
            _snake = new Snake(position, direction, length, id, new Vector2Int[length]);

            for (int i = 0; i < length; i++) {
                _snake.Body[i] = position - direction * i;
            }
        }
        


        public void CheckCollisions(Board board) {
            // check if the snake is colliding with itself
            for (int i = 1; i < _snake.Length; i++) {
                if (_snake.Body[0] == _snake.Body[i]) {
                    // the snake is colliding with itself
                    // the snake dies
                    _snake.Length = i;
                    var newBody = new Vector2Int[_snake.Length];

                    for (int j = 0; j < _snake.Length; j++) {
                        newBody[j] = _snake.Body[j];
                    }

                    _snake.Body = newBody;
                    break;
                }
            }

            // check if the snake is colliding with the walls
            if (_snake.Position.x < 0 || _snake.Position.x >= board.Width || _snake.Position.y < 0 || _snake.Position.y >= board.Height) {
                // the snake is colliding with the walls
                // the snake dies
                _snake.Length = 0;
                _snake.Body = new Vector2Int[0];
            }

            // check if the snake is colliding with the food
            if (board.GetTileType(_snake.Position.x, _snake.Position.y) == TileType.Food) {
                // the snake is colliding with the food
                // the snake eats the food
                Grow();
                board.SpawnFood();
            }
        }

        public void Move(Board board) {
            //check if the tile in front of the snake is empty

            if (board.GetTileType(_snake.Position.x + _snake.Direction.x, _snake.Position.y + _snake.Direction.y) == TileType.None) {
                // the tile in front of the snake is empty
                // the snake moves forward
                _snake.Position += _snake.Direction;
            } else if (board.GetTileType(_snake.Position.x + _snake.Direction.x, _snake.Position.y + _snake.Direction.y) == TileType.Food) {
                // the tile in front of the snake is food
                // the snake eats the food
                Grow();
                board.SpawnFood();
                _snake.Position += _snake.Direction;
            } else if (board.GetTileType(_snake.Position.x + _snake.Direction.x, _snake.Position.y + _snake.Direction.y) == TileType.Snake) {
                // the tile in front of the snake is not empty
                // the snake dies
                _snake.Length = 0;
                _snake.Body = new Vector2Int[0];
            }
        }

        public void ChangeDirection(Vector2Int direction) {
            _snake.Direction = direction;
        }

        public void Grow() {
            _snake.Length++;
            var newBody = new Vector2Int[_snake.Length];

            for (int i = 0; i < _snake.Length - 1; i++) {
                newBody[i] = _snake.Body[i];
            }

            newBody[_snake.Length - 1] = _snake.Body[_snake.Length - 2];
            _snake.Body = newBody;
        }

        public void HandleInputControls() {
           // 
           if (Keyboard.current.leftArrowKey.wasPressedThisFrame) {
               RotateLeft();
               
               
           } else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) {
               RotateRight();
           }
        }
    }
}