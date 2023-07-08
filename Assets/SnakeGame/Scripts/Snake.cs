using UnityEngine;

namespace SnakeGame.Scripts {
    public class Snake {
        private Vector2Int _direction;

        private SnakeColor _color;


        public Snake(Vector2Int position, Vector2Int direction, int length, int id, Vector2Int[] body,
                     SnakeColor color) {
            Position = position;
            Direction = direction;
            Length = length;
            Id = id;
            Body = body;
            _color = color;
        }

        public Vector2Int Position { get; set; }
        public Vector2Int Direction
        {
            get => _direction;
            set => _direction = value;
        }
        public Vector2Int[] Body { get; set; }
        public int Length { get; set; }
        public int Id { get; set; }

        public int Score { get; set; } = 0;

        public bool IsAlive { get; set; } = true;
        public SnakeColor Color => _color;

        public void Grow() {
            Length++;
            var newBody = new Vector2Int[Length];

            for (int i = 0; i < Length - 1; i++) {
                newBody[i] = Body[i];
            }

            newBody[Length - 1] = Body[Length - 2];
            Body = newBody;
            Score++;
        }


        public void Die() {
            IsAlive = false;
            Debug.Log("Snake " + Id + " died");
        }
    }

    public enum SnakeColor {
        // 10 color slots for 10 players
        Player1,
        Player2,
        Player3,
        Player4,
        Player5,
        Player6,
        Player7,
        Player8,
        Player9,
        Player10,
    }
}