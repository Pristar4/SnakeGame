using System.Collections.Generic;
using UnityEngine;

namespace SnakeGame.Scripts {
    public class Board {
        private readonly Tile[,] _tiles;


        public Board(int width, int height) {
            _tiles = new Tile[width, height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    _tiles[x, y] = new Tile(TileType.None);
                }
            }

            var playerPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            var foodPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));


            _tiles[playerPos.x, playerPos.y].Type = TileType.Snake;

            if (_tiles[foodPos.x, foodPos.y].Type == TileType.None) {
                _tiles[foodPos.x, foodPos.y].Type = TileType.Food;
            }
        }

        public int Width => _tiles.GetLength(0);
        public int Height => _tiles.GetLength(1);
        public List<Vector2Int> FoodPositions { get; set; } = new();

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

                    // make the snake appear on the other side of the board
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
    }

    public enum TileType {
        None,
        Food,
        Snake,
        Wall,
    }
    public class Tile {
        public TileType Type { get; set; }
        public Snake Snake { get; set; }

        public Tile(TileType type, Snake snake = null) {
            Type = type;
            Snake = snake;
        }
    }
}