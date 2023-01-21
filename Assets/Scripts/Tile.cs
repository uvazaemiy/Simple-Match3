using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public enum TileType
{
    Normal,
    Obstacle,
    Breakeble
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIdnex;
    
    private Board m_board;

    public TileType tileType = TileType.Normal;

    private SpriteRenderer m_spriteRenderer;

    public int breakableValue = 0;
    [SerializeField] private Sprite[] breakableSprites;
    [SerializeField] private Color normalColor;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIdnex = y;
        m_board = board;
        
        if (tileType == TileType.Breakeble)
            if (breakableSprites[breakableValue] != null)
                m_spriteRenderer.sprite = breakableSprites[breakableValue];
    }


    
    

    private void OnMouseDown()
    {
        m_board.ClickTile(this);
    }
    private void OnMouseEnter()
    {
        m_board.DragToTile(this);
    }
    private void OnMouseUp()
    {
        m_board.ReleaseTile();
    }





    public void BreakTile()
    {
        if (tileType != TileType.Breakeble)
            return;

        StartCoroutine(BreakTileRoutine());
    }
    IEnumerator BreakTileRoutine()
    {
        breakableValue = Math.Clamp(--breakableValue, 0, breakableValue);
        
        yield return new WaitForSeconds(0.25f);

        if (breakableSprites[breakableValue] != null)
            m_spriteRenderer.sprite = breakableSprites[breakableValue];

        if (breakableValue == 0)
        {
            tileType = TileType.Normal;
            m_spriteRenderer.color = normalColor;
        }
    }
}