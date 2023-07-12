#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace SnakeGame.Scripts
{
    [Serializable]
    public class Board
    {
        #region Serialized Fields

        [SerializeField] private Snake[] snakes;

        #endregion


        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);
        public List<Vector2Int> FoodPositions { get; set; } = new();

        public Snake[] Snakes
        {
            get => snakes;
            set => snakes = value;
        }

        public Tile[,] Tiles
        {
            get;
            set;
        }


        public Board(int width, int height, params Snake[] snakes)
        {
            Tiles = new Tile[width, height];
            Snakes = snakes;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tiles[x, y] = new Tile(TileType.None);
                }
            }
        }

        public void ClearBoard()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tiles[x, y].Type = TileType.None;
                }
            }
        }

        public void DrawFood(List<Vector2Int> foodPositions)
        {
            foreach (Vector2Int pos in foodPositions)
            {
                Tiles[pos.x, pos.y].Type = TileType.Food;
            }
        }

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
                        case TileType.None:
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
                        case TileType.None:
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

        public Snake GetSnake(int i) => Snakes[i];

        public Tile GetTile(int x, int y) => Tiles[x, y];

        public bool IsOccupied(Vector2Int vector2Int)
        {
            Tile tile = Tiles[vector2Int.x, vector2Int.y];
            return tile != null && tile.Type != TileType.Snake;
        }

        public bool IsOutOfBounds(Vector2Int nextPosition) =>
                nextPosition.x < 0 || nextPosition.x >= Width || nextPosition.y < 0 ||
                nextPosition.y >= Height;

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
                        Tiles[x, y] = new Tile(TileType.None);
                    }
                    else
                    {
                        Tiles[x, y].Type = TileType.None;
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

        public Vector2Int SpawnFood()
        {
            int x = Random.Range(0, Width);
            int y = Random.Range(0, Height);

            if (Tiles[x, y].Type == TileType.None)
            {
                Tiles[x, y].Type = TileType.Food;
            }

            return new Vector2Int(x, y);
        }
    }
}
