using UnityEngine;
using UnityEngine.Serialization;

namespace Snake.Scripts {
    public class GameManager : MonoBehaviour
    {
        private Board _board;
        [SerializeField] 
        private BoardDisplay boardDisplay;
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private bool wantsToDrawBoard = true;

        void Start()
        {
            // Create board
            _board = new Board(width, height);
        
        }

        void Update() {
            
            if (!wantsToDrawBoard) return;
            boardDisplay.DrawBoard(_board);
            wantsToDrawBoard = false;

        }
    }
}


