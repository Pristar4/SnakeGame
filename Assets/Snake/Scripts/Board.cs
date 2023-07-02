using UnityEngine;

namespace Snake.Scripts {
    public class Board {
        private readonly TileType[,] _tiles;

        public Board(int width, int height) {
            _tiles = new TileType[width, height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    _tiles[x, y] = TileType.None;
                }
            }

            var playerPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            var foodPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));


            _tiles[playerPos.x, playerPos.y] = TileType.Snake;

            if (_tiles[foodPos.x, foodPos.y] == TileType.None) {
                _tiles[foodPos.x, foodPos.y] = TileType.Food;
            } 
        }

        public int Width => _tiles.GetLength(0);
        public int Height => _tiles.GetLength(1);

        public TileType GetTileType(int x, int y) {
            return _tiles[x, y];
        }

        public void SpawnFood() {
            var x = Random.Range(0, Width);
            var y = Random.Range(0, Height);

            if (_tiles[x, y] == TileType.None) {
                _tiles[x, y] = TileType.Food;
            }
            
        }
    }

    public enum TileType {
        None,
        Food,
        Snake,
    }
}