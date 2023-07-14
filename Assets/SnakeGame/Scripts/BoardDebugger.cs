using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SnakeGame.Scripts;
using UnityEngine;

#if UNITY_EDITOR // Editor namespaces can only be used in the editor.
using Sirenix.OdinInspector.Editor.Examples;
#endif

public class BoardDebugger : SerializedMonoBehaviour
{
    [TableMatrix(HorizontalTitle = "Square Celled Matrix",
                 DrawElementMethod = "DrawColoredEnumElement", SquareCells = true)]
    public TileType[,] SquareCelledMatrix;


    private TileType DrawColoredEnumElement(Rect rect, TileType value)
    {
        // snake is green food is yellow empty is white
        Color tileColor;


        switch (value)
        {
            case TileType.Empty:
                tileColor = Color.white;
                break;
            case TileType.Food:
                tileColor = Color.yellow;
                break;
            case TileType.Snake:
                tileColor = Color.green;
                break;
            case TileType.Path:
                tileColor = Color.blue;
                break;
            case TileType.Blocked:
                tileColor = Color.red;
                break;

            default:
                tileColor = Color.white;
                break;
        }

        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), tileColor);

        return value;
    }

    public TileType[,] DrawColoredEnumBoard(Board board)
    {
        var tiles = board.Tiles;

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)


            {
                int length = tiles.GetLength(1) - j - 1;
                this.SquareCelledMatrix[i, length] = tiles[i, j].Type;
            }
        }

        return SquareCelledMatrix;
    }
}
