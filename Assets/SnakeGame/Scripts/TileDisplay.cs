using UnityEngine;

namespace SnakeGame.Scripts {
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileDisplay : MonoBehaviour {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        #region Event Functions

        public void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        #endregion

        public void ChangeMaterial(Material material) {
            spriteRenderer.material = material;
        }
    }
}