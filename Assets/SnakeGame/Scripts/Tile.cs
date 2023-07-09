namespace SnakeGame.Scripts {
    public class Tile {
        public Tile(TileType type, Snake snake = null) {
            Type = type;
            Snake = snake;
        }

        public TileType Type { get; set; }
        public Snake Snake { get; set; }
    }
}