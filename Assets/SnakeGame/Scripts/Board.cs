#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace SnakeGame.Scripts
{
    /// <summary>
    ///     Represents the game board.
    /// </summary>
    [Serializable]
    public class Board
    {
        #region Serialized Fields

        [SerializeField] private Snake[] snakes;

        #endregion

        [SerializeField] private Tile[,] _tiles;


        /// <summary>
        ///     Gets the width of the board.
        /// </summary>
        public int Width => Tiles.GetLength(0);
        /// <summary>
        ///     Gets the height of the board.
        /// </summary>
        public int Height => Tiles.GetLength(1);

        /// <summary>
        ///     Gets or sets the positions of the food on the board.
        /// </summary>
        public List<Vector2Int> FoodPositions { get; set; } = new();

        /// <summary>
        ///     Gets or sets the snakes on the board.
        /// </summary>
        public Snake[] Snakes
        {
            get => snakes;
            set => snakes = value;
        }

        /// <summary>
        ///     Gets or sets the tiles on the board.
        /// </summary>
        public Tile[,] Tiles
        {
            get => _tiles;
            set => _tiles = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Board" /> class with the specified width, height,
        ///     and snakes.
        /// </summary>
        /// <param name="width">The width of the board.</param>
        /// <param name="height">The height of the board.</param>
        /// <param name="snakes">The snakes on the board.</param>
        public Board(int width, int height, params Snake[] snakes)
        {
            Tiles = new Tile[width, height];
            Snakes = snakes;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tiles[x, y] = new Tile(TileType.Empty);
                }
            }
        }

        /// <summary>
        ///     Clears the board by setting all tiles to <see cref="TileType.Empty" />.
        /// </summary>
        public void ClearBoard()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tiles[x, y].Type = TileType.Empty;
                    Tiles[x, y].Snake = null;
                }
            }
        }

        /// <summary>
        ///     Draws the food on the board at the specified positions.
        /// </summary>
        /// <param name="foodPositions">The positions of the food.</param>
        public void DrawFood(List<Vector2Int> foodPositions)
        {
            Debug.Assert(foodPositions != null,
                         nameof(foodPositions) + " != null");

            foreach (Vector2Int pos in foodPositions)
            {
                Tiles[pos.x, pos.y].Type = TileType.Food;
            }
        }

        /// <summary>
        ///     Draws the specified snake on the board.
        /// </summary>
        /// <param name="snake">The snake to draw.</param>
        public void DrawSnake(Snake snake)
        {
            for (int i = 0; i < snake.Length; i++)
            {
                Vector2Int bodyPart = snake.Body[i];

                // Check boundaries before updating the board
                if (bodyPart.x >= 0 && bodyPart.x < Width && bodyPart.y >= 0 &&
                    bodyPart.y < Height)
                {
                    Tile currentTile = Tiles[bodyPart.x, bodyPart.y];
                    currentTile.Type = TileType.Snake;
                    currentTile.Snake = snake;
                }
                else
                {
                    // Snake is out of bounds
                    snake.IsAlive = false;
                }
            }
        }

        /// <summary>
        ///     Gets the board as a one-dimensional array of integers.
        /// </summary>
        /// <returns>The board as a one-dimensional array of integers.</returns>
        public int[] GetBoardAsArray()
        {
            int[] array = new int[Width * Height];
            int index = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    switch (Tiles[x, y].Type)
                    {
                        case TileType.Empty:
                            array[index] = 0;
                            break;
                        case TileType.Food:
                            array[index] = 2;
                            break;
                        case TileType.Snake:
                            array[index] = 1;
                            break;
                    }

                    index++;
                }
            }

            return array;
        }

        /// <summary>
        ///     Gets the board as a two-dimensional array of integers.
        /// </summary>
        /// <returns>The board as a two-dimensional array of integers.</returns>
        public int[,] GetBoardAsMatrix()
        {
            int[,] matrix = new int[Width, Height];

            // Process all tiles
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    switch (Tiles[x, y].Type)
                    {
                        case TileType.Empty:
                            matrix[x, y] = 0;
                            break;
                        case TileType.Food:
                            matrix[x, y] = 2;
                            break;
                        case TileType.Snake:
                            matrix[x, y] = 1;
                            break;
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        ///     Gets the snake at the specified index.
        /// </summary>
        /// <param name="i">The index of the snake.</param>
        /// <returns>The snake at the specified index.</returns>
        public Snake GetSnake(int i) => Snakes[i];

        /// <summary>
        ///     Gets the tile at the specified position.
        /// </summary>
        /// <param name="x">The x-coordinate of the position.</param>
        /// <param name="y">The y-coordinate of the position.</param>
        /// <returns>The tile at the specified position.</returns>
        public Tile GetTile(int x, int y) => Tiles[x, y];

        /// <summary>
        ///     Determines whether the specified position is occupied by a snake.
        /// </summary>
        /// <param name="vector2Int">The position to check.</param>
        /// <returns><c>true</c> if the position is occupied by a snake; otherwise, <c>false</c>.</returns>
        public bool IsOccupied(Vector2Int vector2Int)
        {
            Tile tile = Tiles[vector2Int.x, vector2Int.y];
            return tile != null && tile.Type != TileType.Snake;
        }

        /// <summary>
        ///     Determines whether the specified position is out of bounds.
        /// </summary>
        /// <param name="nextPosition">The position to check.</param>
        /// <returns><c>true</c> if the position is out of bounds; otherwise, <c>false</c>.</returns>
        public bool IsOutOfBounds(Vector2Int nextPosition) =>
                nextPosition.x < 0 || nextPosition.x >= Width || nextPosition.y < 0 ||
                nextPosition.y >= Height;

        public bool IsSnakeAtPosition(int x, int y) => Tiles[x, y].Type == TileType.Snake;

        /// <summary>
        ///     Resets the board with the specified snakes, width, and height.
        /// </summary>
        /// <param name="snakeArray">The snakes to add to the board.</param>
        /// <param name="width">The width of the board.</param>
        /// <param name="height">The height of the board.</param>
        public void Reset(Snake[] snakeArray, int width, int height)
        {
            if (Tiles == null || Tiles.GetLength(0) != width || Tiles.GetLength(1) != height)
            {
                Tiles = new Tile[width, height];
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (Tiles[x, y] == null)
                    {
                        Tiles[x, y] = new Tile(TileType.Empty);
                    }
                    else
                    {
                        Tiles[x, y].Type = TileType.Empty;
                        Tiles[x, y].Snake = null;
                    }
                }
            }

            if (FoodPositions == null)
            {
                FoodPositions = new List<Vector2Int>();
            }
            else
            {
                FoodPositions.Clear();
            }

            Snakes = snakeArray;
        }

        /// <summary>
        ///     Spawns food at a random position on the board.
        /// </summary>
        /// <returns>The position of the spawned food.</returns>
        public Vector2Int SpawnFood()
        {
            int x, y;

            do
            {
                x = Random.Range(0, Tiles.GetLength(0));
                y = Random.Range(0, Tiles.GetLength(1));
            } while (Tiles[x, y].Type == TileType.Snake || Tiles[x, y].Type == TileType.Food);

            Tiles[x, y].Type = TileType.Food;
            Vector2Int foodPosition = new(x, y);
            FoodPositions.Add(foodPosition);

            return foodPosition;
        }
    }
}
