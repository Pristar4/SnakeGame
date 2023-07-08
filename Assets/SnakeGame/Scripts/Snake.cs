using System;
using UnityEngine;

namespace SnakeGame.Scripts {
    [Serializable]
    public class Snake {
        #region Serialized Fields

        [SerializeField] private Vector2Int position;
        [SerializeField] private Vector2Int direction;
        [SerializeField] private Vector2Int nextDirection;

        [SerializeField] private int length;
        [SerializeField] private int id;
        [SerializeField] private Vector2Int[] body;
        [SerializeField] private SnakeColor color;

        #endregion

        public Snake(Vector2Int position, Vector2Int direction, int length, int id, Vector2Int[] body,
                     SnakeColor color) {
            Position = position;
            Direction = direction;
            Length = length;
            Id = id;
            Body = body;
            this.color = color;
        }


        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }
        public Vector2Int Direction
        {
            get => direction;
            set => direction = value;
        }

        public SnakeDirection DirectionEnum
        {
            get
            {
                if (Direction == Vector2Int.up) {
                    return SnakeDirection.Up;
                }

                if (Direction == Vector2Int.down) {
                    return SnakeDirection.Down;
                }

                if (Direction == Vector2Int.left) {
                    return SnakeDirection.Left;
                }

                if (Direction == Vector2Int.right) {
                    return SnakeDirection.Right;
                }

                Debug.LogError("Direction is not a valid direction");
                return SnakeDirection.Up;
            }
        }
        public Vector2Int[] Body
        {
            get => body;
            set => body = value;
        }
        public int Length
        {
            get => length;
            set => length = value;
        }
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int Score { get; set; }

        public bool IsAlive { get; set; } = true;
        public SnakeColor Color => color;
        public bool AteFood { get; set; }
        public Vector3 Head => new(Position.x, Position.y, 0);
        public Vector2Int NextDirection
        {
            get => nextDirection;
            set => nextDirection = value;
        }

        public void Grow() {
            Length++;
            var newBody = new Vector2Int[Length];

            for (int i = 0; i < Length - 1; i++) {
                newBody[i] = Body[i];
            }

            newBody[Length - 1] = Body[Length - 2];
            Body = newBody;
            Score++;
            AteFood = true;
        }


        public void Die() {
            IsAlive = false;
            // Debug.Log("Snake " + Id + " died");
        }

        public bool ContainsPosition(Vector2Int vector2Int) {
            for (int i = 0; i < Length; i++) {
                if (Body[i] == vector2Int) {
                    return true;
                }
            }

            return false;
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