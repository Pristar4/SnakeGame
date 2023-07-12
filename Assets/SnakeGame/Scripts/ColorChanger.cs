#region

using UnityEngine;

#endregion

namespace SnakeGame.Scripts
{
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

        public ColorOption colorOption; // The color option you want to apply to the sprite

        public Material redMaterial;
        public Material greenMaterial;
        public Material blueMaterial;

        #endregion

        private SpriteRenderer _spriteRenderer;

        #region Event Functions

        private void Start() => _spriteRenderer = GetComponent<SpriteRenderer>();

        #endregion

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
