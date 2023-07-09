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
            int[,] matrix = new int[Width, Height];

            // Process all tiles
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    switch (_tiles[x, y].Type) {
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

        public int[] GetBoardAsArray() {
            int[] array = new int[Width * Height];
            int index = 0;

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    switch (_tiles[x, y].Type) {
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

        public void DrawSnake(Snake snake) {
            for (int i = 0; i < snake.Length; i++) {
                var bodyPart = snake.Body[i];



                // Check boundaries before updating the board
                if (bodyPart.x >= 0 && bodyPart.x < Width && bodyPart.y >= 0 && bodyPart.y < Height) {
                    var currentTile = _tiles[bodyPart.x, bodyPart.y];
                    currentTile.Type = TileType.Snake;
                    currentTile.Snake = snake;
                } else {
                    // Snake is out of bounds
                    snake.IsAlive = false;
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
            var tile =  _tiles[vector2Int.x, vector2Int.y];
            return tile != null && tile.Type != TileType.Snake;
        }

        public Snake GetSnake(int i) {
            return Snakes[i];
        }

        public void Reset(Snake[] snakes, int width, int height) {
            if (_tiles == null || _tiles.GetLength(0) != width || _tiles.GetLength(1) != height) {
                _tiles = new Tile[width, height];
            }

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (_tiles[x, y] == null) {
                        _tiles[x, y] = new Tile(TileType.None);
                    } else {
                        _tiles[x, y].Type = TileType.None;
                        _tiles[x, y].Snake = null;
                    }
                }
            }

            if (FoodPositions == null) {
                FoodPositions = new List<Vector2Int>();
            } else {
                FoodPositions.Clear();
            }

            Snakes = snakes;
        }
    }
}