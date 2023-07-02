using System;
using UnityEngine;

namespace Snake.Scripts {
    class SpriteBoardDisplay : BoardDisplay {
        [SerializeField] private TileDisplay tileDisplayPrefab;
        [SerializeField] private Material noneMaterial;
        [SerializeField] private Material foodMaterial;
        [SerializeField] private Material snakeMaterial;
        private TileDisplay[,] _tileDisplays;

        #region IBoardDisplay implementation

        public override void DrawBoard(Board board) {
            CompareBoardAndTileDisplays(board);
            

            for (int y = 0; y < board.Height; y++) {
                for (int x = 0; x < board.Width; x++) {
                    var tileType = board.GetTileType(x, y);
                    var tileDisplay = _tileDisplays[x, y];

                    switch (tileType) {
                        case TileType.None:
                            tileDisplay.ChangeMaterial(noneMaterial);
                            break;
                        case TileType.Food:
                            tileDisplay.ChangeMaterial(foodMaterial);
                            break;
                        case TileType.Snake:
                            tileDisplay.ChangeMaterial(snakeMaterial);
                            break;
                        default:
                            // throw argument exception for invalid tile type
                            throw new ArgumentOutOfRangeException("TileType is not valid",innerException: null);
                            
                    }
                }
            }
        }

        public override void ClearBoard(Board board) {
            throw new NotImplementedException();
        }

        #endregion
        

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
    }
}