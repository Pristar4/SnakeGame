#region

using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace SnakeGame.Scripts
{
    /// <summary>
    ///     This class gives the direction a player wants to move to
    ///     it only allows the directions up, down, left and right
    ///     without the opposite direction
    /// </summary>
    public static class InputController
    {
        #region Methods

        public static Vector2Int
                HandleInput(Vector2Int currentDirection, InputSchemer inputSchemer)
        {
            Vector2Int up = Vector2Int.up;
            Vector2Int down = Vector2Int.down;
            Vector2Int left = Vector2Int.left;
            Vector2Int right = Vector2Int.right;

            // default direction is the current direction

            Vector2Int nextDirection = default;

            if (Keyboard.current[inputSchemer.UpKey].wasPressedThisFrame &&
                currentDirection != down)
            {
                nextDirection = up;
            }
            else if (Keyboard.current[inputSchemer.LeftKey].wasPressedThisFrame &&
                     currentDirection != right)
            {
                nextDirection = left;
            }
            else if (Keyboard.current[inputSchemer.DownKey].wasPressedThisFrame &&
                     currentDirection != up)
            {
                nextDirection = down;
            }
            else if (Keyboard.current[inputSchemer.RightKey].wasPressedThisFrame &&
                     currentDirection != left)
            {
                nextDirection = right;
            }

            return nextDirection;
        }

        #endregion
    }
}
