#region

using NUnit.Framework;
using SnakeGame.Scripts;

#endregion

namespace Tests.EditMode
{
    public class BoardTests
    {
        private readonly int _height = 10;
        private readonly int _width = 10;
        private Board _board;

        [SetUp]
        public void Setup() => _board = new Board(_width, _height);

        [Test]
        public void TestBoardCreation()
        {
            Assert.IsNotNull(_board);
            Assert.IsNotNull(_board.Tiles);
            Assert.IsNotNull(_board.FoodPositions);
            Assert.AreEqual(_board.Width, _width);
            Assert.AreEqual(_board.Height, _height);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Assert.IsNotNull(_board.GetTile(x, y));
                    Assert.AreEqual(TileType.Empty, _board.GetTile(x, y).Type);
                }
            }
        }

        [Test]
        public void TestFoodSpawn()
        {
            var foodPosition = _board.SpawnFood();

            Assert.IsTrue(foodPosition.x >= 0);
            Assert.IsTrue(foodPosition.x < _board.Width);
            Assert.IsTrue(foodPosition.y >= 0);
            Assert.IsTrue(foodPosition.y < _board.Height);
            Assert.AreEqual(TileType.Food, _board.GetTile(foodPosition.x, foodPosition.y).Type);
        }
    }
}
