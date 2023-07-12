#region

using UnityEngine;

#endregion

namespace SnakeGame.Scripts
{
    public abstract class BoardDisplay : MonoBehaviour
    {
        #region Event Functions

        public abstract void Reset();

        #endregion

        public abstract void ClearBoard(Board board);

        /// <summary>
        ///    Draws the board.
        /// </summary>
        /// <param name="board"></param>
        public abstract void DrawBoard(Board board);
    }
}
