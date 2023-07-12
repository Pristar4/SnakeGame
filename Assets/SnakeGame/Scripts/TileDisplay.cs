#region

using UnityEngine;

#endregion

namespace SnakeGame.Scripts {
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileDisplay : MonoBehaviour {
        #region Serialized Fields

        [SerializeField] private SpriteRenderer spriteRenderer;

        #endregion

        #region Event Functions

        public void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        #endregion

        public void ChangeMaterial(Material material) {
            spriteRenderer.material = material;
        }
        public void SetColor(Color color) {
            spriteRenderer.material.color = color;
        }
    }
}