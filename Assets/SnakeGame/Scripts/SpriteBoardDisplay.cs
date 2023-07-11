using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SnakeGame.Scripts {
    internal class SpriteBoardDisplay : BoardDisplay {
        #region Serialized Fields

        [SerializeField] private SnakeAgent snakeAgent;


        [SerializeField] private TileDisplay tileDisplayPrefab;
        [SerializeField] private Material noneMaterial;
        [SerializeField] private Material foodMaterial;
        [SerializeField] private Material snakeMaterial;
        [SerializeField] Material foodDistanceMaterial;
        [SerializeField] private Gradient rewardGradient;


        [Header("Snake Materials")] [SerializeField]
        private Material player1Material;
        [SerializeField] private Material player2Material;
        [SerializeField] private Material player3Material;
        [SerializeField] private Material player4Material;
        [SerializeField] private Material player5Material;
        [SerializeField] private Material player6Material;
        [SerializeField] private Material player7Material;
        [SerializeField] private Material player8Material;
        [SerializeField] private Material player9Material;
        [SerializeField] private Material player10Material;

        [Header("Visuals")] [SerializeField] private GameObject snakeDirectionPrefab;

        #endregion

        private readonly Dictionary<Snake, GameObject> _snakeDirections = new();
        private readonly List<GameObject> _snakeHeads = new();

        private TileDisplay[,] _tileDisplays;

        #region Event Functions

        public override void Reset() {
            foreach (var obj in _snakeDirections.Values) {
                Destroy(obj);
            }

            _snakeDirections.Clear();
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

        private void SnakeDirectionCompare(Board board) {
            if (_snakeDirections.Count != board.Snakes.Length) {
                Reset();

                foreach (var snake in board.Snakes) {
                    var obj = Instantiate(snakeDirectionPrefab, transform);
                    _snakeDirections.Add(snake, obj);
                }
            }
        }

        #region IBoardDisplay implementation

        public override void DrawBoard(Board board) {
            CompareBoardAndTileDisplays(board);


            if (board.Snakes != null) {
                SnakeDirectionCompare(board);
                SnakeDirectionUpdate();
            }

            if (board.Snakes != null) {
                var snakePos = new Vector2(board.Snakes[0].Position.x, board.Snakes[0].Position.y);
                var foodPos = new Vector2(board.FoodPositions[0].x, board.FoodPositions[0].y);
                int numInterpolationPoints = Math.Max(board.Width, board.Height);
                HashSet<Vector2Int> highlightedTiles = new HashSet<Vector2Int>();

                for (int i = 0; i < numInterpolationPoints; i++) {
                    float t = (float)i / (numInterpolationPoints - 1);
                    Vector2 interpolatedPos = Vector2.Lerp(snakePos, foodPos, t);
                    Vector2Int gridPos = Vector2Int.RoundToInt(interpolatedPos);
                    highlightedTiles.Add(gridPos);
                }

                var previousDistance = Vector2.Distance(snakePos, foodPos);

                for (int y = 0; y < board.Height; y++) {
                    for (int x = 0; x < board.Width; x++) {
                        var tile = board.GetTile(x, y);
                        var tileDisplay = _tileDisplays[x, y];

                        switch (tile.Type) {
                            case TileType.None:
                                var snakeLength = board.Snakes[0].Length;
                                // currentDistance between the current tile and the food
                                var currentDistance = Vector2.Distance(new Vector2(x, y), foodPos);

                                float reward = Mathf.Log((snakeLength + previousDistance)
                                                         / (snakeLength + currentDistance));
                                float clampedReward = Mathf.Clamp01((reward + 1) /
                                                                    2);

                                var c = Color.Lerp(foodDistanceMaterial.color, Color.black, (clampedReward + 1) / 2f);

                                if (board.FoodPositions.Count != 0) {
                                    var blendedColor = Color.Lerp(Color.green, Color.black, clampedReward);

                                    if (highlightedTiles.Contains(new Vector2Int(x, y))) {
                                        tileDisplay.ChangeMaterial(foodDistanceMaterial);
                                        tileDisplay.SetColor(blendedColor);
                                    } else {
                                        tileDisplay.ChangeMaterial(noneMaterial);
                                    }
                                } else {
                                    tileDisplay.ChangeMaterial(noneMaterial);
                                }

                                previousDistance = Vector2.Distance(new Vector2(x, y), snakePos);
                                break;
                            case TileType.Food:
                                tileDisplay.ChangeMaterial(foodMaterial);
                                break;
                            case TileType.Snake:
                                tileDisplay.ChangeMaterial(GetSnakeMaterial(tile.Snake));
                                break;
                            default:
                                // throw argument exception for invalid tile type
                                throw new ArgumentOutOfRangeException(
                                        "TileType is not valid", innerException: null);
                        }
                    }
                }
            }
        }

        private void SnakeDirectionUpdate() {
            foreach (var entity in _snakeDirections) {
                int positionY = entity.Key.Position.y;
                int positionX = entity.Key.Position.x;
                var tilePosition = _tileDisplays[positionX, positionY].transform.position;

                entity.Value.transform.position = tilePosition + new Vector3(0, 0, -1);



                //rotation 
                Quaternion rotation;

                switch (entity.Key.DirectionEnum) {
                    case SnakeDirection.Up:
                        rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case SnakeDirection.Down:
                        rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case SnakeDirection.Left:
                        rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case SnakeDirection.Right:
                        rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("SnakeDirection is not valid", innerException: null);
                }

                entity.Value.transform.localRotation = rotation;
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
    public enum SnakeDirection {
        Up,
        Down,
        Left,
        Right,
    }
}