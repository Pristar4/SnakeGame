#region

using Unity.MLAgents;
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

        /// <summary>
        ///     Handles the snake's direction based on the given action.
        /// </summary>
        /// <param name="action">The action to take.</param>
        /// <param name="snake">The snake to handle.</param>
        public void HandleSnakeDirection(int action, Snake snake, SnakeAgent snakeAgent)
        {
            switch (action)
            {
                case 0: //turn left
                    snake.NextDirection = snakeAgent.RotateCounterClockwise(snake.Direction);
                    break;
                case 1: //go straight, no need to change the direction
                    snake.NextDirection = snake.Direction;
                    break;
                case 2: //turn right
                    snake.NextDirection = snakeAgent.RotateClockwise(snake.Direction);
                    break;
            }
        }
        /// <summary>
        ///     Gets the relative direction of the input direction relative to the current direction.
        /// </summary>
        /// <param name="currentDirection">The current direction.</param>
        /// <param name="inputDirection">The input direction.</param>
        /// <returns>The relative direction of the input direction relative to the current direction.</returns>
        public int GetRelativeDirection(Vector2Int currentDirection, Vector2Int inputDirection,
                                        SnakeAgent snakeAgent)
        {
            Vector2Int clockwiseDirection = snakeAgent.RotateClockwise(currentDirection);
            Vector2Int counterClockwiseDirection =
                    snakeAgent.RotateCounterClockwise(currentDirection);

            if (inputDirection == counterClockwiseDirection)
            {
                return 0; // turn left
            }

            if (inputDirection == currentDirection)
            {
                return 1; // go straight
            }

            if (inputDirection == clockwiseDirection)
            {
                return 2; // turn right
            }

            return 1;
        }
        /// <summary>
        ///     Checks if the current score is higher than the high score and updates it if necessary.
        /// </summary>
        /// <param name="snake">The snake to check the score for.</param>
        public void CheckForNewHighScore(Snake snake, SnakeAgent snakeAgent)
        {
            // update high score if current score is higher
            if (snakeAgent.Board.Snakes[0].Score > snakeAgent.HighScore)
            {
                snakeAgent.HighScore = snake.Score;
                snakeAgent.Stats.Add("Score/High Score", snakeAgent.HighScore,
                                     StatAggregationMethod.MostRecent);
            }

            snakeAgent.HighScoreText.text = "High Score: " + snakeAgent.HighScore;

            snakeAgent.ScoreText.text = "Score: " + snakeAgent.Board.Snakes[0].Score;
        }
        /// <summary>
        ///     Checks the status of the given snake and updates the reward accordingly.
        /// </summary>
        /// <param name="snake">The snake to check.</param>
        public void CheckSnakeStatus(Snake snake, SnakeAgent snakeAgent)
        {
            int availableSpace = snakeAgent.Board.Width * snakeAgent.Board.Height - snake.Length;

            if (availableSpace <= 0)
            {
                Debug.Log("Game Won!");
                snakeAgent.CurrentReward += 10f;
            }

            if (snake.AteFood)
            {
                snakeAgent.Stats1.Add("Score/Time to Eat (Scaled with Length)",
                                      (Time.fixedUnscaledTime - snakeAgent.EpisodeStarted) /
                                      snake.Length);
                Debug.Log("Eat : " + 1f);
                snakeAgent.CurrentReward += 1f;
                snakeAgent.AddReward(1f);
                snake.AteFood = false;
                snakeAgent.EatTimer = 0;
            }

            if (snakeAgent.EatTimer > snakeAgent.MaxHungryTime * 2)
            {
                Debug.Log("Starved");
                snake.IsAlive = false;
            }


            if (snakeAgent.EatTimer > snakeAgent.MaxHungryTime)
            {
                Debug.Log("Starving");
                snakeAgent.CurrentReward -= 0.1f;
                snakeAgent.AddReward(-0.1f);
            }

            if (!snake.IsAlive)
            {
                snakeAgent.Stats1.Add("Score/AVG Score", snake.Score);
                snakeAgent.Stats1.Add("Score/Time to Die (Scaled with Length)",
                                      (Time.fixedUnscaledTime - snakeAgent.EpisodeStarted) /
                                      snake.Length);
                snakeAgent.Stats1.Add("Score/StepCount", snakeAgent.StepCount);
                Debug.Log("Dead : " + -1f);
                snakeAgent.CurrentReward -= 1f;
                snakeAgent.AddReward(-1f);
                snakeAgent.TotalScore += snakeAgent.Board.Snakes[0].Score;
                snakeAgent.TotalReward += snakeAgent.CurrentReward;
                snakeAgent.AverageScore = snakeAgent.TotalScore / snakeAgent.EpisodeCount;
                snakeAgent.AverageReward = snakeAgent.TotalReward / snakeAgent.EpisodeCount;
                snakeAgent.EndEpisode();
            }

            snakeAgent.CurrentRewardText.text =
                    "Current Reward:\n" + snakeAgent.CurrentReward.ToString("F2");
            snakeAgent.AverageRewardText.text =
                    "Average Reward:\n" + snakeAgent.AverageReward.ToString("F2");
            snakeAgent.AverageScoreText.text =
                    "Average Score:\n" + snakeAgent.AverageScore.ToString("F2");
        }
        public void ProcessSnakeMovement(Snake snake, SnakeAgent snakeAgent)
        {
            this.FinalizeDirection(snake);

            TileType collisionTileType = this.CheckCollisions(snakeAgent.Board, snake);

            if (collisionTileType == TileType.Food)
            {
                snake.Grow();
                int availableSpace =
                        snakeAgent.Board.Width * snakeAgent.Board.Height - snake.Length;
                snakeAgent.Board1.FoodPositions.Remove(snake.Position + snake.Direction);
                snake.AteFood = true;
                snakeAgent.EatTimer1 = 0;

                if (availableSpace > 0)
                {
                    snakeAgent.Board1.SpawnFood();
                }
            }
            else if (collisionTileType == TileType.Snake || collisionTileType == TileType.Wall)
            {
                snakeAgent.Board.Snakes[0].IsAlive = false;
            }

            if (snake.IsAlive)
            {
                this.Move(snakeAgent.Board, snake, false);
            }
        }
    }


}
