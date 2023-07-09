using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SnakeGame.Scripts {
    [Serializable]
    public class Board {
        #region Serialized Fields

        [SerializeField] private Snake[] snakes;

        #endregion

        private Tile[,] _tiles;


        public Board(int width, int height, params Snake[] snakes) {
            _tiles = new Tile[width, height];
            Snakes = snakes;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    _tiles[x, y] = new Tile(TileType.None);
                }
            }




        }

        public int Width => _tiles.GetLength(0);
        public int Height => _tiles.GetLength(1);
        public List<Vector2Int> FoodPositions { get; set; } = new();
        public Snake[] Snakes
        {
            get => snakes;
            set => snakes = value;
        }

        public Tile GetTile(int x, int y) {
            return _tiles[x, y];
        }

        public Vector2Int SpawnFood() {
            int x = Random.Range(0, Width);
            int y = Random.Range(0, Height);

            if (_tiles[x, y].Type == TileType.None) {
                _tiles[x, y].Type = TileType.Food;
            }

            return new Vector2Int(x, y);
        }

        public int[,] GetBoardAsMatrix() {
            var matrix = new int[Width, Height];
            // Process all tiles
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    switch (_tiles[x, y].Type)
                    {
                        case TileType.None:
                            matrix[x, y] = 0;
                            break;
                        case TileType.Food:
                            matrix[x, y] = 2;
                            break;
                        case TileType.Snake:
                        case TileType.Wall:
                            matrix[x, y] = 1;
                            break;
                    }
                }
            }

            return matrix;
            
            
        }

        public int[] GetBoardAsArray() {
            int[] array = new int[Width * Height];
            int index = 0;

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    switch (_tiles[x,y].Type) {
                        case TileType.None:
                            array[index] = 0;
                            break;
                        case TileType.Food:
                            array[index] = 2;
                            break;
                        case TileType.Snake:
                        case TileType.Wall:
                            array[index] = 1;
                            break;
                    }

                    index++;
                }
            }

            return array;

        }

        public void DrawSnake(Snake snake) {
            for (int i = 0; i < snake.Length; i++) {
                var bodyPart = snake.Body[i];



                // Check boundaries before updating the board
                if (bodyPart.x >= 0 && bodyPart.x < Width && bodyPart.y >= 0 && bodyPart.y < Height) {
                    var currentTile = _tiles[bodyPart.x, bodyPart.y];
                    currentTile.Type = TileType.Snake;
                    currentTile.Snake = snake;
                } else {
                    Debug.Log("Snake out of bounds");
                }
            }
        }

        public void ClearBoard() {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    _tiles[x, y].Type = TileType.None;
                }
            }
        }

        public bool IsOutOfBounds(Vector2Int nextPosition) {
            return nextPosition.x < 0 || nextPosition.x >= Width || nextPosition.y < 0 || nextPosition.y >= Height;
        }

        public void DrawFood(List<Vector2Int> foodPositions) {
            foreach (var pos in foodPositions) {
                _tiles[pos.x, pos.y].Type = TileType.Food;
            }
        }

        public bool IsOccupied(Vector2Int vector2Int) {
            return _tiles[vector2Int.x, vector2Int.y].Type == TileType.Snake;
        }

        public Snake GetSnake(int i) {
            return Snakes[i];
        }

        public void Reset(Snake[] snakes, int width, int height) {
            
            FoodPositions.Clear(); // No need to instantiate a new list, Clear old list instead
            Snakes = snakes;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    _tiles[x, y].Type = TileType.None;
                    _tiles[x, y].Snake = null; // Assuming Snake=null is same as new Tile()
                    
                }
            }

        }
    }

    public enum TileType {
        None,
        Food,
        Snake,
        Wall,
    }
    public class Tile {
        public Tile(TileType type, Snake snake = null) {
            Type = type;
            Snake = snake;
        }

        public TileType Type { get; set; }
        public Snake Snake { get; set; }
    }
}