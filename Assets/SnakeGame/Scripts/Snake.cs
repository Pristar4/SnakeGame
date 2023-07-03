using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnakeGame.Scripts {
    public class Snake {
        [SerializeField]
        private Vector2Int _direction;
        public Vector2Int Position { get; set; }
        public Vector2Int Direction
        {
            get => _direction;
            set => _direction = value;
        }
        public Vector2Int[] Body { get; set; }
        public int Length { get; set; }
        public int Id { get; set; }

        public Snake(Vector2Int position, Vector2Int direction, int length, int id, Vector2Int[] body) {
            Position = position;
            Direction = direction;
            Length = length;
            Id = id;
            Body = body;
        }

        public void Grow() {
            Length++;
            var newBody = new Vector2Int[Length];

            for (int i = 0; i < Length - 1; i++) {
                newBody[i] = Body[i];
            }

            newBody[Length - 1] = Body[Length - 2];
            Body = newBody;
        }

        public void CheckCollisions(Board board) {
            // check if the next move would be a collision with the wall
            var nextPosition = Position + Direction;


            if (nextPosition.x < 0 || nextPosition.x >= board.Width || nextPosition.y < 0 ||
                nextPosition.y >= board.Height) {
                // make the snake wrap around the board

                Debug.Log("Collision with wall");
            } else {
                var nextTileType = board.GetTileType(nextPosition.x, nextPosition.y);

                if (nextTileType == TileType.Snake) {
                    Debug.Log("Collision with snake");
                    // Game Over
                    Time.timeScale = 0;
                } else if (nextTileType == TileType.Food) {
                    Grow();
                    board.SpawnFood();
                }
            }
        }

    }
}