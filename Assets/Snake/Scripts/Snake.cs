using UnityEngine;

namespace Snake.Scripts {
    public class Snake {
        private Vector2Int _position;
        private Vector2Int _direction;
        private Vector2Int[] _body;
        private int _length;
        private int _id;
        
        public Vector2Int Position {
            get => _position;
            set => _position = value;
        }
        public Vector2Int Direction {
            get => _direction;
            set => _direction = value;
        }
        public Vector2Int[] Body {
            get => _body;
            set => _body = value;
        }
        public int Length {
            get => _length;
            set => _length = value;
        }
        public int Id {
            get => _id;
            set => _id = value;
        }

        public Snake(Vector2Int position, Vector2Int direction, int length, int id, Vector2Int[] body) {
            _position = position;
            _direction = direction;
            _length = length;
            _id = id;
            _body = body;
        }
    }
}