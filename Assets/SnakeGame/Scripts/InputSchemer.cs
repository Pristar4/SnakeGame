#region

using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace SnakeGame.Scripts
{
    [CreateAssetMenu(fileName = "InputSchemer", menuName = "ScriptableObjects/InputSchemer",
                     order = 1)]
    public class InputSchemer : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private Key upKey;
        [SerializeField] private Key leftKey;
        [SerializeField] private Key downKey;
        [SerializeField] private Key rightKey;

        #endregion

        public Key UpKey => upKey;
        public Key DownKey => downKey;
        public Key RightKey => rightKey;

        public Key LeftKey => leftKey;
    }
}
