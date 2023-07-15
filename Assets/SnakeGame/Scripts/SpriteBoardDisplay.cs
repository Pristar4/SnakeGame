#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace SnakeGame.Scripts
{
    /// <summary>
    ///     Displays the game board using sprites.
    /// </summary>
    internal class SpriteBoardDisplay : BoardDisplay
    {
        #region Serialized Fields

        [SerializeField] private TileDisplay tileDisplayPrefab;
        [SerializeField] private Material noneMaterial;
        [SerializeField] private Material foodMaterial;
        [SerializeField] private Material snakeMaterial;
        [SerializeField] private Material foodDistanceMaterial;
        [SerializeField] private Gradient rewardGradient;


        [Header("Snake Materials")] [SerializeField]
        private Material player1Material;

        [SerializeField] private Material player2Material;
        [SerializeField] private Material player3Material;
        [SerializeField] private Material player4Material;
        [SerializeField] private Material player5Material;
        [SerializeField] private Material player6Material;
        [SerializeField] private Material player7Material;
        [SerializeField] private Material player8Material;
        [SerializeField] private Material player9Material;
        [SerializeField] private Material player10Material;

        [Header("Visuals")] [SerializeField] private GameObject snakeDirectionPrefab;
        [SerializeField] private bool isShowingReward;

        #endregion


        private readonly Dictionary<Snake, GameObject> _snakeDirections = new();

        private TileDisplay[,] _tileDisplays;

        #region Event Functions

        public override void Reset()
        {
            foreach (GameObject obj in _snakeDirections.Values)
            {
                Destroy(obj);
            }

            _snakeDirections.Clear();
        }

        #endregion

        /// <summary>
        ///     Compares the current tile displays with the board and creates or deletes tile displays as
        ///     necessary.
        /// </summary>
        /// <param name="board">The board to compare with the tile displays.</param>
        private void CompareBoardAndTileDisplays(Board board)
        {
            if (_tileDisplays == null)
            {
                CreateTileDisplays(board.Width, board.Height);
            }
            else if (_tileDisplays.GetLength(0) != board.Width
                     || _tileDisplays.GetLength(1) != board.Height)
            {
                DeleteTileDisplays();
                CreateTileDisplays(board.Width, board.Height);
            }
        }


        /// <summary>
        ///     Creates a new tile display at the specified position.
        /// </summary>
        /// <param name="position">The position to create the tile display at.</param>
        /// <returns>The newly created tile display.</returns>
        private TileDisplay CreateTileDisplay(Vector3 position)
        {
            TileDisplay tileDisplay = Instantiate(tileDisplayPrefab, transform);
            tileDisplay.transform.localPosition = position;
            return tileDisplay;
        }
        /// <summary>
        ///     Creates tile displays for the board based on the given width and height.
        /// </summary>
        /// <param name="width">The width of the board.</param>
        /// <param name="height">The height of the board.</param>
        /// <see cref="TileDisplay" />
        private void CreateTileDisplays(int width, int height)
        {
            _tileDisplays = new TileDisplay[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 tilePos = new Vector3(x, y, 0) - new Vector3(width, height, 0) / 2f +
                                      Vector3.one / 2f;
                    _tileDisplays[x, y] = CreateTileDisplay(tilePos);
                }
            }
        }

        /// <summary>
        ///     Deletes the given tile display.
        /// </summary>
        /// <param name="tileDisplay">The tile display to delete.</param>
        private void DeleteTileDisplay(TileDisplay tileDisplay) => Destroy(tileDisplay.gameObject);

        /// <summary>
        ///     Deletes all tile displays in the current board.
        /// </summary>
        private void DeleteTileDisplays()
        {
            foreach (TileDisplay tileDisplay in _tileDisplays)
            {
                DeleteTileDisplay(tileDisplay);
            }
        }

        /// <summary>
        ///     Compares the number of snakes in the board with the number of snake direction objects.
        ///     If the numbers are different, it resets the snake direction objects and creates new ones for
        ///     each snake.
        /// </summary>
        /// <param name="board">The board to compare with the snake direction objects.</param>
        private void SnakeDirectionCompare(Board board)
        {
            if (_snakeDirections.Count != board.Snakes.Length)
            {
                Reset();

                foreach (Snake snake in board.Snakes)
                {
                    GameObject obj = Instantiate(snakeDirectionPrefab, transform);
                    _snakeDirections.Add(snake, obj);
                }
            }
        }

        #region IBoardDisplay implementation

        /// <inheritdoc />
        public override void DrawBoard(Board board)
        {
            CompareBoardAndTileDisplays(board);


            if (board.Snakes != null)
            {
                SnakeDirectionCompare(board);
                SnakeDirectionUpdate();

                Vector2Int snakePos = board.GetSnake(0).Position;
                Vector2Int foodPos = snakePos;

                if (board.FoodPositions.Count > 0)
                {
                    foodPos = board.FoodPositions[0];
                }
                const int rewardRadius = 10;

                for (int y = 0; y < board.Height; y++)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        Tile tile = board.GetTile(x, y);
                        TileDisplay tileDisplay = _tileDisplays[x, y];

                        switch (tile.Type)
                        {
                            case TileType.Empty:
                                HandleTileTypeNone(x, y, snakePos, foodPos, rewardRadius,
                                                   tileDisplay);

                                break;
                            case TileType.Food:
                                tileDisplay.ChangeMaterial(foodMaterial);
                                break;
                            case TileType.Snake:
                                tileDisplay.ChangeMaterial(GetSnakeMaterial(tile.Snake));
                                break;
                            case TileType.Wall:
                            default:
                                throw new ArgumentOutOfRangeException(
                                        "TileType is not valid", innerException: null);
                        }
                    }
                }
            }
        }
        /// <summary>
        ///     Handles the display of tiles with TileType.None.
        ///     If isShowingReward is true, it changes the material of the tile to a reward material if the
        ///     tile is within the reward radius of the snake or food.
        ///     Otherwise, it changes the material of the tile to a default none material.
        /// </summary>
        /// <param name="x">The x position of the tile.</param>
        /// <param name="y">The y position of the tile.</param>
        /// <param name="snakePos">The position of the snake.</param>
        /// <param name="foodPos">The position of the food.</param>
        /// <param name="rewardRadius">The radius within which a tile is considered a reward tile.</param>
        /// <param name="tileDisplay">The tile display to change the material of.</param>
        private void HandleTileTypeNone(int x, int y, Vector2Int snakePos, Vector2Int foodPos,
                                        int rewardRadius, TileDisplay tileDisplay)
        {
            if (isShowingReward)
            {
                Vector2Int currentTilePos = new(x, y);
                float distanceToSnake =
                        Vector2Int.Distance(currentTilePos, snakePos);
                float distanceToFood =
                        Vector2Int.Distance(currentTilePos, foodPos);

                if (distanceToSnake <= rewardRadius)
                {
                    Material rewardMaterial = new(foodDistanceMaterial)
                    {
                        color = rewardGradient.Evaluate(
                                1f - distanceToFood / rewardRadius),
                    };

                    tileDisplay.ChangeMaterial(rewardMaterial);
                }
                else
                {
                    tileDisplay.ChangeMaterial(noneMaterial);
                }
            }
            else
            {
                tileDisplay.ChangeMaterial(noneMaterial);
            }
        }


        /// <summary>
        ///     Updates the position and rotation of the snake direction objects on the board.
        /// </summary>
        private void SnakeDirectionUpdate()
        {
            foreach (KeyValuePair<Snake, GameObject> entity in _snakeDirections)
            {
                int positionY = entity.Key.Position.y;
                int positionX = entity.Key.Position.x;
                Vector3 tilePosition = _tileDisplays[positionX, positionY].transform.position;

                entity.Value.transform.position = tilePosition + new Vector3(0, 0, -1);

                //rotation
                Quaternion rotation;

                switch (entity.Key.DirectionEnum)
                {
                    case SnakeDirection.Up:
                        rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case SnakeDirection.Down:
                        rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case SnakeDirection.Left:
                        rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case SnakeDirection.Right:
                        rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                                "SnakeDirection is not valid", innerException: null);
                }

                entity.Value.transform.localRotation = rotation;
            }
        }


        /// <summary>
        ///     Returns the material associated with the given snake color.
        /// </summary>
        /// <param name="tileSnake">The snake to get the material for.</param>
        /// <returns>The material associated with the given snake color.</returns>
        private Material GetSnakeMaterial(Snake tileSnake)
        {
            switch (tileSnake.Color)
            {
                case SnakeColor.Player1:
                    return player1Material;
                case SnakeColor.Player2:
                    return player2Material;
                case SnakeColor.Player3:
                    return player3Material;
                case SnakeColor.Player4:
                    return player4Material;
                case SnakeColor.Player5:
                    return player5Material;
                case SnakeColor.Player6:
                    return player6Material;
                case SnakeColor.Player7:
                    return player7Material;
                case SnakeColor.Player8:
                    return player8Material;
                case SnakeColor.Player9:
                    return player9Material;
                case SnakeColor.Player10:
                    return player10Material;
            }

            return snakeMaterial;
        }
        /// <summary>
        ///     Clears the board by changing the material of all tile displays to the default none material.
        /// </summary>
        /// <param name="board">The board to clear.</param>
        public override void ClearBoard(Board board)
        {
            CompareBoardAndTileDisplays(board);

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    TileDisplay tileDisplay = _tileDisplays[x, y];
                    tileDisplay.ChangeMaterial(noneMaterial);
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     Represents the possible directions that a snake can move in.
    /// </summary>
    public enum SnakeDirection
    {
        Up,
        Down,
        Left,
        Right,
    }
}
