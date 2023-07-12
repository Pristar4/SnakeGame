#region

using NUnit.Framework;
using SnakeGame.Scripts;
using UnityEngine;

#endregion

namespace Tests.EditMode
{
    [TestFixture]
    public class SnakeTests
    {
        [SetUp]
        public void SetUp()
        {
            var position = new Vector2Int(0, 0);
            var direction = new Vector2Int(0, 1);
            Vector2Int[] body =
            {
                position,
            };

            _snake = new Snake(position, direction, 1, 1, body, SnakeColor.Player1);
        }

        private Snake _snake;

        [Test]
        public void ContainsPosition_ReturnsExpectedResult_WhenPositionIsInSnakeBody()
        {
            var positionInBody = _snake.Body[0];

            bool result = _snake.ContainsPosition(positionInBody);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsPosition_ReturnsExpectedResult_WhenPositionIsNotInSnakeBody()
        {
            var positionNotInBody = new Vector2Int(5, 5);

            bool result = _snake.ContainsPosition(positionNotInBody);

            Assert.IsFalse(result);
        }

        [Test]
        public void Die_MakesSnakeDead()
        {
            _snake.Die();

            Assert.IsFalse(_snake.IsAlive);
        }

        [Test]
        public void Grow_IncreasesSnakeLengthAndScore_AdjestsBodyAndAteFoodState()
        {
            int initialLength = _snake.Length;
            int initialScore = _snake.Score;

            _snake.Grow();

            Assert.AreEqual(initialLength + 1, _snake.Length);
            Assert.AreEqual(initialScore + 1, _snake.Score);
            Assert.AreEqual(_snake.Body.Length, _snake.Length);
            Assert.IsTrue(_snake.AteFood);
        }

        [Test]
        public void SnakeConstructor_CreatesSnakeInstance() => Assert.IsNotNull(_snake);
    }
}
