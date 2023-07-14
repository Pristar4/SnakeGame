#region

using UnityEngine;

#endregion

namespace SnakeGame.Scripts
{
    /// <summary>
    ///     Changes the color of a sprite renderer based on a selected color option.
    /// </summary>
    public class ColorChanger : MonoBehaviour
    {
        #region ColorOption enum

        public enum ColorOption
        {
            Red,
            Green,
            Blue,
        }

        #endregion

        #region Serialized Fields

        [Tooltip("The color option you want to apply to the sprite.")]
        public ColorOption colorOption;

        public Material redMaterial;
        public Material greenMaterial;
        public Material blueMaterial;

        #endregion

        private SpriteRenderer _spriteRenderer;

        #region Event Functions

        private void Start() => _spriteRenderer = GetComponent<SpriteRenderer>();

        #endregion

        /// <summary>
        ///     Changes the color of the sprite renderer based on the selected color option.
        /// </summary>
        public void ChangeColor()
        {
            Material newMaterial = GetMaterialFromColorOption(colorOption);

            if (newMaterial != null)
            {
                _spriteRenderer.material = newMaterial;
            }
            else
            {
                Debug.LogError("Invalid color option!");
            }
        }

        private Material GetMaterialFromColorOption(ColorOption color)
        {
            switch (color)
            {
                case ColorOption.Red:
                    return redMaterial;
                case ColorOption.Green:
                    return greenMaterial;
                case ColorOption.Blue:
                    return blueMaterial;
                default:
                    return null;
            }
        }
    }
}
