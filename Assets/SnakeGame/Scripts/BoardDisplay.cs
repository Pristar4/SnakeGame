using UnityEngine;

namespace SnakeGame.Scripts {
    public abstract class BoardDisplay : MonoBehaviour {
        public abstract void DrawBoard(Board board);


        public abstract void ClearBoard(Board board);
    }
}