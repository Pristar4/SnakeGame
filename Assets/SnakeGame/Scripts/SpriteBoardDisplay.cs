using System;
using UnityEngine;

namespace SnakeGame.Scripts {
    internal class SpriteBoardDisplay : BoardDisplay {
        #region Serialized Fields

        [SerializeField] private TileDisplay tileDisplayPrefab;
        [SerializeField] private Material noneMaterial;
        [SerializeField] private Material foodMaterial;
        [SerializeField] private Material snakeMaterial;
        
        [Header("Snake Materials")]
        [SerializeField] private Material player1Material;
        [SerializeField] private Material player2Material;
        [SerializeField] private Material player3Material;
        [SerializeField] private Material player4Material;
        [SerializeField] private Material player5Material;
        [SerializeField] private Material player6Material;
        [SerializeField] private Material player7Material;
        [SerializeField] private Material player8Material;
        [SerializeField] private Material player9Material;
        [SerializeField] private Material player10Material;
        

        #endregion

        private TileDisplay[,] _tileDisplays;


        private TileDisplay CreateTileDisplay(Vector3 position) {
            var tileDisplay = Instantiate(tileDisplayPrefab, transform);
            tileDisplay.transform.localPosition = position;
            return tileDisplay;
        }

        private void DeleteTileDisplay(TileDisplay tileDisplay) {
            Destroy(tileDisplay.gameObject);
        }

        private void CreateTileDisplays(int width, int height) {
            _tileDisplays = new TileDisplay[width, height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    var tilePos = new Vector3(x, y, 0) - new Vector3(width, height, 0) / 2f + Vector3.one / 2f;
                    _tileDisplays[x, y] = CreateTileDisplay(tilePos);
                }
            }
        }

        private void DeleteTileDisplays() {
            foreach (var tileDisplay in _tileDisplays) {
                DeleteTileDisplay(tileDisplay);
            }
        }

        // implement object pooling
        private void CompareBoardAndTileDisplays(Board board) {
            if (_tileDisplays == null) {
                CreateTileDisplays(board.Width, board.Height);
            } else if (_tileDisplays.GetLength(0) != board.Width
                       || _tileDisplays.GetLength(1) != board.Height) {
                DeleteTileDisplays();
                CreateTileDisplays(board.Width, board.Height);
            }
        }

        #region IBoardDisplay implementation

        public override void DrawBoard(Board board) {
            CompareBoardAndTileDisplays(board);



            for (int y = 0; y < board.Height; y++) {
                for (int x = 0; x < board.Width; x++) {
                    var tile = board.GetTile(x, y);
                    var tileDisplay = _tileDisplays[x, y];

                    switch (tile.Type) {
                        case TileType.None:
                            tileDisplay.ChangeMaterial(noneMaterial);
                            break;
                        case TileType.Food:
                            tileDisplay.ChangeMaterial(foodMaterial);
                            break;
                        case TileType.Snake:
                            tileDisplay.ChangeMaterial(GetSnakeMaterial(tile.Snake));
                            break;
                        default:
                            // throw argument exception for invalid tile type
                            throw new ArgumentOutOfRangeException("TileType is not valid", innerException: null);
                    }
                }
            }
        }

        private Material GetSnakeMaterial(Snake tileSnake) {
            switch (tileSnake.Color) {
                case SnakeColor.Player1:
                    return player1Material;
                case SnakeColor.Player2:
                    return player2Material;
                case SnakeColor.Player3:
                    return player3Material;
                case SnakeColor.Player4:
                    return player4Material;
                case SnakeColor.Player5:
                    return player5Material;
                case SnakeColor.Player6:
                    return player6Material;
                case SnakeColor.Player7:
                    return player7Material;
                case SnakeColor.Player8:
                    return player8Material;
                case SnakeColor.Player9:
                    return player9Material;
                case SnakeColor.Player10:
                    return player10Material;
            }

            return snakeMaterial;
        }

        public override void ClearBoard(Board board) {
            CompareBoardAndTileDisplays(board);

            for (int y = 0; y < board.Height; y++) {
                for (int x = 0; x < board.Width; x++) {
                    var tileDisplay = _tileDisplays[x, y];
                    tileDisplay.ChangeMaterial(noneMaterial);
                }
            }
        }

        #endregion
    }
}