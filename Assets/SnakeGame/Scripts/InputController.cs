using UnityEngine;
using UnityEngine.InputSystem;

namespace SnakeGame.Scripts {
    /// <summary>
    ///     This class gives the direction a player wants to move to
    ///     it only allows the directions up, down, left and right
    ///     without the opposite direction
    /// </summary>
    public static class InputController {
        public static Vector2Int HandleInput(Vector2Int currentDirection, InputSchemer inputSchemer) {
            var up = Vector2Int.up;
            var down = Vector2Int.down;
            var left = Vector2Int.left;
            var right = Vector2Int.right;

            Vector2Int nextDirection = default;

            if (Keyboard.current[inputSchemer.UpKey].wasPressedThisFrame && currentDirection != down) {
                nextDirection = up;
            } else if (Keyboard.current[inputSchemer.LeftKey].wasPressedThisFrame && currentDirection != right) {
                nextDirection = left;
            } else if (Keyboard.current[inputSchemer.DownKey].wasPressedThisFrame && currentDirection != up) {
                nextDirection = down;
            } else if (Keyboard.current[inputSchemer.RightKey].wasPressedThisFrame && currentDirection != left) {
                nextDirection = right;
            }

            return nextDirection;
        }
    }
}