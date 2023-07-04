using UnityEngine;

namespace SnakeGame.Scripts {
    public class Snake {
        [SerializeField] private Vector2Int _direction;


        public Snake(Vector2Int position, Vector2Int direction, int length, int id, Vector2Int[] body) {
            Position = position;
            Direction = direction;
            Length = length;
            Id = id;
            Body = body;
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
}