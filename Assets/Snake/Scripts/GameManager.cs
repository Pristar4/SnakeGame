using System;
using System.Collections;
using System.Collections.Generic;
using Snake.Scripts;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Board _board;
    [SerializeField] 
    private BoardDisplay _boardDisplay;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private bool wantsToDrawBoard = true;

    void Start()
    {
        // Create board
        _board = new Board(width, height);
        
    }

    void Update() {
        // Draw board  on space key press
        if (!wantsToDrawBoard) return;
        _boardDisplay.DrawBoard(_board);
        wantsToDrawBoard = false;

    }
}


