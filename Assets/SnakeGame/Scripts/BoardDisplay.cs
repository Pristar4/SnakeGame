using UnityEngine;

namespace SnakeGame.Scripts {
    public abstract class BoardDisplay : MonoBehaviour {
        #region Event Functions

        public abstract void Reset();

        #endregion

        public abstract void DrawBoard(Board board);

        public abstract void ClearBoard(Board board);
    }
}