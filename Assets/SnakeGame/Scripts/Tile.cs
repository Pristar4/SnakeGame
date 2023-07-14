using System;
using UnityEngine;

namespace SnakeGame.Scripts
{
    /// <summary>
    /// Represents a tile in the game board.
    /// </summary>
    [Serializable]
    public class Tile

    {
        #region Constructors and Finalizers

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        /// <param name="type">The type of the tile.</param>
        /// <param name="snake">The snake occupying the tile, if any.</param>
        public Tile(TileType type, Snake snake = null)
        {
            Type = type;
            Snake = snake;
        }

        #endregion

        #region Fields and Properties

        /// <summary>
        /// Gets or sets the type of the tile.
        /// </summary>
        [field: SerializeField]
        public TileType Type { get; set; }

        /// <summary>
        /// Gets or sets the snake occupying the tile, if any.
        /// </summary>
        public Snake Snake { get; set; }

        #endregion
    }
}
