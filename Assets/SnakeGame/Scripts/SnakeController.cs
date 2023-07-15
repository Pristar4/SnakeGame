#region

using UnityEngine;

#endregion

namespace SnakeGame.Scripts
{
    public class SnakeController
    {
        #region Methods

        public static void InitializeSnakeBody(Snake snake, Vector2Int position,
                                               Vector2Int direction, int length)
        {
            for (int i = 0; i < length; i++)
            {
                snake.Body[i] = position - direction * i;
            }
        }

        private Snake CreateSnake(Vector2Int position, Vector2Int direction, int length, int id)
        {
            Snake snake = new(position, direction, length, id, new Vector2Int[length],
                              SnakeColor.Player1);
            InitializeSnakeBody(snake, position, direction, length);

            return snake;
        }


        public void FinalizeDirection(Snake snake) =>
                snake.Direction = snake.NextDirection; // Update the direction of the Snake

        public void Move(Board board, Snake snake, bool wrapIsEnabled)
        {
            Vector2Int nextPosition = snake.Position + snake.NextDirection;

            if (nextPosition.x < 0 || nextPosition.x >= board.Width || nextPosition.y < 0 ||
                nextPosition.y >= board.Height)
            {
                if (wrapIsEnabled)
                {
                    // make the snake wrap around the board
                    if (nextPosition.x < 0)
                    {
                        snake.Position = new Vector2Int(board.Width - 1, snake.Position.y);
                    }
                    else if (nextPosition.x >= board.Width)
                    {
                        snake.Position = new Vector2Int(0, snake.Position.y);
                    }
                    else if (nextPosition.y < 0)
                    {
                        snake.Position = new Vector2Int(snake.Position.x, board.Height - 1);
                    }
                    else if (nextPosition.y >= board.Height)
                    {
                        snake.Position = new Vector2Int(snake.Position.x, 0);
                    }
                }
            }
            else
            {
                snake.Position = nextPosition;
            }


            // update the body
            for (int i = snake.Length - 1; i > 0; i--)
            {
                snake.Body[i] = snake.Body[i - 1];
            }

            snake.Body[0] = snake.Position;
        }

        public TileType CheckCollisions(Board board, Snake snake)
        {
            // check if the next move would be a collision with the wall
            Vector2Int nextPosition = snake.Position + snake.NextDirection;

            if (board.IsOutOfBounds(nextPosition))
            {
                return TileType.Wall;
            }

            TileType nextTileType = board.GetTile(nextPosition.x, nextPosition.y).Type;

            return nextTileType switch
            {
                TileType.Snake => TileType.Snake,
                TileType.Food => TileType.Food,
                _ => TileType.Empty,
            };
        }

        public Snake[] CreateSnakes(int width, int height, int numberOfSnakes, int startSize)
        {
            Snake[] snakeArray = new Snake[numberOfSnakes];

            for (int i = 0; i < numberOfSnakes; i++)
            {
                Vector2Int startSpawnPosition =
                        new(Random.Range(0, width), Random.Range(0, height));
                Vector2Int startDirection = Vector2Int.up;
                snakeArray[i] = CreateSnake(startSpawnPosition, startDirection, startSize, i);
            }


            return snakeArray;
        }

        #endregion
    }


}
