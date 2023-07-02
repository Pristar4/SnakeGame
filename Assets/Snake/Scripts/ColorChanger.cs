using UnityEngine;

namespace Snake.Scripts
{
    public class ColorChanger : MonoBehaviour
    {
        public enum ColorOption
        {
            Red,
            Green,
            Blue
        }

        public ColorOption colorOption; // The color option you want to apply to the sprite

        private SpriteRenderer _spriteRenderer;

        public Material redMaterial;
        public Material greenMaterial;
        public Material blueMaterial;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

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
