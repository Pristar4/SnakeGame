#region

using UnityEngine;

#endregion

namespace SnakeGame.Scripts
{
    /// <summary>
    ///     Abstract class for displaying the game board.
    /// </summary>
    public abstract class BoardDisplay : MonoBehaviour
    {
        #region Event Functions

        /// <summary>
        ///     Resets the board display.
        /// </summary>
        public abstract void Reset();

        #endregion

        /// <summary>
        ///     Clears the board display.
        /// </summary>
        /// <param name="board">The game board to clear.</param>
        public abstract void ClearBoard(Board board);

        /// <summary>
        ///     Draws the game board.
        /// </summary>
        /// <param name="board">The game board to draw.</param>
        public abstract void DrawBoard(Board board);
    }
}
