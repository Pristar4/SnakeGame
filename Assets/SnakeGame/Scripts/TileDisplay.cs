#region

using UnityEngine;

#endregion

namespace SnakeGame.Scripts
{
    /// <summary>
    /// Controls the display of a tile in the game.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileDisplay : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private SpriteRenderer spriteRenderer;

        #endregion

        #region Event Functions

        public void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        #endregion

        /// <summary>
        /// Changes the material of the tile.
        /// </summary>
        /// <param name="material">The new material to use.</param>
        public void ChangeMaterial(Material material) => spriteRenderer.material = material;

        /// <summary>
        /// Sets the color of the tile.
        /// </summary>
        /// <param name="color">The new color to use.</param>
        public void SetColor(Color color) => spriteRenderer.material.color = color;
    }
}
