using UnityEngine;

public class Board {
    private TileType[,] _tiles;

    public Board(int width, int height) {
        _tiles = new TileType[width, height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                _tiles[x, y] = TileType.NONE;
            }
        }

        var playerPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        var foodPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));


        _tiles[playerPos.x, playerPos.y] = TileType.SNAKE;

        if (_tiles[foodPos.x, foodPos.y] == TileType.NONE) {
            _tiles[foodPos.x, foodPos.y] = TileType.FOOD;
        }
    }
}

enum TileType {
    NONE,
    FOOD,
    SNAKE,
}