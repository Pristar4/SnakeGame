namespace SnakeGame.Scripts
{
    public class Tile
    {
        #region Constructors and Finalizers

        public Tile(TileType type, Snake snake = null)
        {
            Type = type;
            Snake = snake;
        }

        #endregion

        #region Fields and Properties

        public TileType Type { get; set; }
        public Snake Snake { get; set; }

        #endregion
    }
}
