using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;





public class Board : MonoBehaviour
{
    [Header("Size of board")]
    [Space]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int borderSize;
    [Space]
    [Header("Tiles/pieces setttings")]
    [Space]
    [SerializeField] private GameObject tileNormalPrefab;
    //[SerializeField] private GameObject tileObstaclePrefab;
    [Space]
    [SerializeField] private StartingObject[] startingTiles;
    [SerializeField] private StartingObject[] StartingGamePieces;
    [SerializeField] private GameObject[] gamePiecePrebas;
    [Space]
    [SerializeField] private GameObject adjacentBombPrefab;
    [SerializeField] private GameObject columnBombPrefab;
    [SerializeField] private GameObject rowBombPrefab;
    [SerializeField] private GameObject colorBombPrefab;
    [Space]
    [SerializeField] private int maxCollectibles = 3;
    [SerializeField] private int collectibleCount = 0;
    [Range(0,0.3f)] [SerializeField] private float chanceForCollectible = 0.1f;
    [SerializeField] private GameObject[] collectiblePrefabs;
    [Space]
    private GameObject m_clickedTileBomb;
    private GameObject m_targetTileBomb;
    private Tile[,] m_allTiles;
    private GamePiece[,] m_allGamePieces;
    [Space]
    [Header("Debug and other settings")]
    [Space]
    //[SerializeField] private ParticleManager ParticleManager.Instance;
    [Space]
    [SerializeField] private int fillYOffset = 10;
    [SerializeField] private float swapTime = 0.5f;
    private int m_scoreMultiplier = 0;
    [Space]
    [SerializeField] private Tile m_clickedTile;
    [SerializeField] private Tile m_targetTile;

    private Camera mainCam;

    private bool m_playerInputEnabled = true;
    [HideInInspector] public bool isRefilling;
    
    
    
    
    
    private void Start()
    {
        mainCam = Camera.main;
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
    }

    public void SetupAll(float delay)
    {
        StartCoroutine(SetupRoutine(delay));
    }
    
    public IEnumerator SetupRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        SetupCamera();
        SetupTiles();
        SetupGamePieces();
        List<GamePiece> startingCollectibles = FindAllCollectibles();
        collectibleCount = startingCollectibles.Count;
        FillBoard(fillYOffset, swapTime);
    }

    private void SetupTiles()
    {
        foreach (StartingObject sTile in startingTiles)
            if (sTile != null)
                MakeTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
        
        for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
        {
            if (m_allTiles[i, j] == null)
                MakeTile(tileNormalPrefab, i, j);
        }
    }
    
    private void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + x + "," + y + ")";

            m_allTiles[x, y] = tile.GetComponent<Tile>();

            tile.transform.parent = transform;
            m_allTiles[x, y].Init(x, y, this);
        }
    }
    
    private void MakeGamePice(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.5f)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);

            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
            }
        }
    }

    private GameObject MakeBomb(GameObject prefab, int x, int y, float z = 0)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            bomb.GetComponent<Bomb>().Init(this);
            bomb.GetComponent<Bomb>().SetCoord(x, y);
            bomb.transform.parent = transform;
            return bomb;
        }
        return null;
    }

    private void SetupGamePieces()
    {
        foreach (StartingObject sPiece in StartingGamePieces)
        {
            GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity) as GameObject;
            MakeGamePice(piece, sPiece.x, sPiece.y, fillYOffset);
        }
    }
    
    private void SetupCamera()
    {
        mainCam.transform.position = new Vector3((float)(width - 1) / 2, (float)(height - 1) / 2, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)width / 2 + (float)borderSize;
        float horizontalSize = ((float)width / 2 + (float)borderSize) / aspectRatio;

        mainCam.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    private GameObject GetRandomObject(GameObject[] objectArray)
    {
        int randomIdx = Random.Range(0, objectArray.Length);
        if (objectArray[randomIdx] == null)
            Debug.LogWarning("BOARD.GetRandomObject at index: " + randomIdx + "does not contain a valid GameObject!");

        return objectArray[randomIdx];
    }
    
    


    
    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.Init(this);
        gamePiece.transform.parent = transform;
        if (IsWithinBounds(x, y))
            m_allGamePieces[x, y] = gamePiece;
        gamePiece.SetCoord(x, y);
    }
    
    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }
    
    private GamePiece FillRandomGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (!IsWithinBounds(x, y))
            return null;

        GameObject randomPiece = Instantiate(GetRandomObject(gamePiecePrebas), Vector3.zero, Quaternion.identity) as GameObject;

        MakeGamePice(randomPiece, x, y, falseYOffset, moveTime);
        
        return randomPiece.GetComponent<GamePiece>();
    }

    private GamePiece FillRandomCollectibleAt(int x, int y, int falseYOfsset = 0, float moveTime = 0.1f)
    {
        if (IsWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomObject(collectiblePrefabs), Vector3.zero, Quaternion.identity) as GameObject;
            MakeGamePice(randomPiece, x, y, falseYOfsset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }

        return null;
    }

    private void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxIterations = 100;
        int iterations = 0;
        
        for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
        {
            if (m_allGamePieces[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle)
            {
                GamePiece piece = null;

                if (j == height - 1 && CanAddCollectibles())
                {
                    piece = FillRandomCollectibleAt(i, j, falseYOffset, moveTime);
                    collectibleCount++;
                }
                else
                {
                    piece = FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                
                    iterations = 0;

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                    
                        piece = FillRandomGamePieceAt(i, j, falseYOffset, moveTime);

                        iterations++;
                        if (iterations >= maxIterations)
                        {
                            Debug.Log("To much iterations! Breaking.");
                            break;
                        }
                    }
                }
            }
        }
    }
    
    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if (leftMatches == null)
            leftMatches = new List<GamePiece>();
        if (downwardMatches == null)
            downwardMatches = new List<GamePiece>();

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }
    
    

    

    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null)
            m_clickedTile = tile; 
    }
    
    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile))
            m_targetTile = tile;
    }
    
    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
            StartCoroutine(SwitchTilesRoutine(m_clickedTile, m_targetTile));
                
        m_clickedTile = null;
        m_targetTile = null;
    }
    
    private IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_playerInputEnabled && !GameManager.Instance.isEndGame)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIdnex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIdnex];

            if (targetPiece != null && clickedPiece != null)
            {
                clickedPiece.Move(targetTile.xIndex, targetTile.yIdnex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIdnex, swapTime);
                
                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIdnex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIdnex);
                List<GamePiece> colorMatches = new List<GamePiece>();

                if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
                {
                    clickedPiece.matchValue = targetPiece.matchValue;
                    colorMatches = FindAllMatchValue(clickedPiece.matchValue);
                }
                else if (IsColorBomb(targetPiece) && !IsColorBomb(clickedPiece))
                {
                    targetPiece.matchValue = clickedPiece.matchValue;
                    colorMatches = FindAllMatchValue(targetPiece.matchValue);
                }
                else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                {
                    foreach (GamePiece piece in m_allGamePieces)
                    {
                        if (!colorMatches.Contains(piece))
                            colorMatches.Add(piece);
                    }
                }

                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIdnex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIdnex, swapTime);
                }
                else
                {
                    Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIdnex - clickedTile.yIdnex);
                    m_clickedTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIdnex, swipeDirection, clickedPieceMatches);
                    m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIdnex, swipeDirection, targetPieceMatches);

                    if (m_clickedTileBomb != null && targetPiece != null)
                    {
                        GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(clickedBombPiece))
                            clickedBombPiece.ChangeColor(targetPiece);
                    }
                    if (m_targetTileBomb != null && targetPiece != null)
                    {
                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(targetBombPiece))
                            targetBombPiece.ChangeColor(clickedPiece);
                    }
                    
                    StartCoroutine(ClearAndRefillBoardRoutine(clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList()));
                    
                    GameManager.Instance.movesLeft--;
                    GameManager.Instance.UpdateMoves();
                    // HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIdnex);
                    // HighlightMatchesAt(targetTile.xIndex, targetTile.yIdnex);
                }
            }
        }
    }
    
    bool IsNextTo(Tile start, Tile end)
    {
        if (Math.Abs(start.xIndex - end.xIndex) == 1 && start.yIdnex == end.yIdnex)
            return true;
        if (Math.Abs(start.yIdnex - end.yIdnex) == 1 && start.xIndex == end.xIndex)
            return true;
        return false;
    }
    
    
    
    
    
    private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
            matches.Add(startPiece);
        }
        else
            return null;

        int nextX;
        int nextY;
        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue; i++)
        {
            nextX = startX + (int) Math.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int) Math.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY))
                break;

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];
            
            /*if (nextPiece == null)
                break;
            else
            {*/
                try
                {
                    if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.None)
                        matches.Add(nextPiece);
                    else
                        break;
                }
                catch
                { break; }
            // }
        }

        if (matches.Count >= minLength)
            return matches;

        return null;
    }
    
    private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
            upwardMatches = new List<GamePiece>();
        if (downwardMatches == null)
            downwardMatches = new List<GamePiece>();

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }
    
    private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
            rightMatches = new List<GamePiece>();
        if (leftMatches == null)
            leftMatches = new List<GamePiece>();

        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }
    
    private List<GamePiece> FindMatchesAt(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(startX, startY, minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(startX, startY, minLength);

        if (horizMatches == null)
            horizMatches = new List<GamePiece>();
        if (vertMatches == null)
            vertMatches = new List<GamePiece>();
        
        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }
    
    private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();

        return matches;
    }
    
    private List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();
        
        for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
        {
            List<GamePiece> matches = FindMatchesAt(i, j);
            combinedMatches = combinedMatches.Union(matches).ToList();
        }

        return combinedMatches;
    }

    
    
    
    //DEBUG
    private void HighlightTileOn(int x, int y, Color col)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }
    
    private void HighlightTileOff(int x, int y)
    {
        if (m_allTiles[x, y].tileType == TileType.Breakeble)
            return;
        
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }
    
    private void HighlightMatchesAt(int x, int y)
    {
        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
            foreach (GamePiece piece  in combinedMatches)
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
    }
    
    private void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
        {
            HighlightMatchesAt(i, j);
        }
    }
    
    private void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
            if (piece != null)
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
    }





    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePieces[x, y];

        if (pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            pieceToClear.KillTween();
            Destroy(pieceToClear.gameObject);
            
        }
        // HighlightTileOff(x, y);
    }
    
    private void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach (GamePiece piece in gamePieces)
            if (piece != null)
            {
                int bonus = 0;
                if (gamePieces.Count >= 4)
                    bonus = 10;
                piece.ScorePoints(m_scoreMultiplier, bonus);
                
                ClearPieceAt(piece.xIndex, piece.yIndex);
                
                if (ParticleManager.Instance != null)
                {
                    if (bombedPieces.Contains(piece))
                        ParticleManager.Instance.BombFXAt(piece.xIndex, piece.yIndex);
                    else
                        ParticleManager.Instance.ClearePieceFXAt(piece.xIndex, piece.yIndex);
                }
            }
    }
    
    private void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = m_allTiles[x, y];
        if (tileToBreak != null && tileToBreak.tileType == TileType.Breakeble)
        {
            if (ParticleManager.Instance != null)
                ParticleManager.Instance.BreakTileFXAt(tileToBreak.breakableValue, x, y);
            tileToBreak.BreakTile();
        }
    }
    
    private void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
                BreakTileAt(piece.xIndex, piece.yIndex);
        }
    }
    
    
    private void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
        {
            ClearPieceAt(i, j);
        }
    }   //DEBUG

    



    private List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece piece in gamePieces)
        {
            if (!columns.Contains(piece.xIndex))
                columns.Add(piece.xIndex);
        }

        return columns;
    }
    
    private List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height - 1; i++)
            if (m_allGamePieces[column, i] == null && m_allTiles[column, i].tileType != TileType.Obstacle)
                for (int j = i + 1; j < height; j++)
                    if (m_allGamePieces[column, j] != null)
                    {
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);
                        
                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                            movingPieces.Add(m_allGamePieces[column, i]);

                        m_allGamePieces[column, j] = null;
                        break;
                    }

        return movingPieces;
    }
    
    private List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (int column in columnsToCollapse)
            movingPieces = movingPieces.Union(CollapseColumn(column, 0.3f)).ToList();

        return movingPieces;
    }
    




    private IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        m_playerInputEnabled = false;
        isRefilling = true;
        
        List<GamePiece> matches = gamePieces;

        m_scoreMultiplier = 0;
        
        do
        {
            m_scoreMultiplier++;
            
            yield return StartCoroutine(ClearAndCollapseRoutine(gamePieces));
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();
            yield return new WaitForSeconds(swapTime);
        
        } 
        while (matches.Count != 0);
        
        m_playerInputEnabled = true;
        isRefilling = false;
    }
    
    private IEnumerator RefillRoutine()
    {
        FillBoard(fillYOffset, swapTime * 2);
        yield return new WaitForSeconds(swapTime * 2);
        List<GamePiece> newMathcedPieces = FindAllMatches();
        //yield return new WaitForSeconds(swapTime);
        if (newMathcedPieces.Count != 0)
            StartCoroutine(ClearAndRefillBoardRoutine(newMathcedPieces));
    }
    
    private IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();
        
        //HighlightPieces(gamePieces);
        
        yield return new WaitForSeconds(swapTime);

        bool isFinished = false;
        while (!isFinished)
        {
            List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            bombedPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);
            List<GamePiece> allCollectibles = FindAllCollectibles();
            List<GamePiece> blockers = gamePieces.Intersect(allCollectibles).ToList();
            collectedPieces = collectedPieces.Union(blockers).ToList();
            
            collectibleCount -= collectedPieces.Count;
            gamePieces = gamePieces.Union(collectedPieces).ToList();

            ClearPieceAt(gamePieces, bombedPieces);
            BreakTileAt(gamePieces);
            
            if (m_clickedTileBomb != null)
            {
                ActivateBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }
            if (m_targetTileBomb != null)
            {
                ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }
            
            movingPieces = CollapseColumn(gamePieces);
            
            while (!IsCollapsed(movingPieces))
                yield return null;

            matches = FindMatchesAt(movingPieces);
            collectedPieces = FindCollectiblesAt(0, true);
            matches = matches.Union(collectedPieces).ToList();
            
            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                m_scoreMultiplier++;
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayBonusSound();
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }        
    }
    
    private bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
            if (piece != null)
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                    return false;
        return true;
    }





    private List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if (m_allGamePieces[i, row] != null)
                gamePieces.Add(m_allGamePieces[i, row]);
        }

        return gamePieces;
    }

    List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        
        for (int i = 0; i < height; i++)
        {
            if (m_allGamePieces[column, i] != null)
                gamePieces.Add(m_allGamePieces[column, i]);
        }

        return gamePieces;
    }

    List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        
        for (int i = x - offset; i <= x + offset; i++)
        for (int j = y - offset; j <= y + offset; j++)
        {
            if (IsWithinBounds(i, j))
                gamePieces.Add(m_allGamePieces[i, j]);
        }

        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();
                Bomb bomb = piece.GetComponent<Bomb>();
                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:
                            piecesToClear = FindAllMatchValue(bomb.matchValue);
                            break;
                    }

                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                    allPiecesToClear = RemoveCollectibles(allPiecesToClear);
                }
            }
        }

        return allPiecesToClear;
    }

    bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (xStart == -1 || yStart == -1)
                {
                    xStart = piece.xIndex;
                    yStart = piece.yIndex;
                    continue;
                }

                if (piece.xIndex != xStart && piece.yIndex == yStart)
                    horizontal = true;
                if (piece.xIndex == xStart && piece.yIndex != yStart)
                    vertical = true;
            }
        }

        return horizontal && vertical;
    }

    GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
    {
        GameObject bomb = null;
        if (gamePieces.Count >= 4)
        {
            if (IsCornerMatch(gamePieces))
                bomb = MakeBomb(adjacentBombPrefab, x, y);
            else
            {
                if (gamePieces.Count >= 5)
                {
                    if (colorBombPrefab != null)
                        bomb = MakeBomb(colorBombPrefab, x, y, -0.1f);
                }
                else
                {
                    if (swapDirection.x != 0)
                        bomb = MakeBomb(rowBombPrefab, x, y);
                    else
                        bomb = MakeBomb(columnBombPrefab, x, y);
                }
            }
        }

        return bomb;
    }

    private void ActivateBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if (IsWithinBounds(x, y))
            m_allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
    }

    private List<GamePiece> FindAllMatchValue(MatchValue mValue)
    {
        List<GamePiece> foundPieces = new List<GamePiece>();
        
        for (int i =0; i< width; i++)
        for (int j = 0; j < height; j++)
        {
            if (m_allGamePieces[i, j] != null)
            {
                if (m_allGamePieces[i, j].matchValue == mValue)
                    foundPieces.Add(m_allGamePieces[i, j]);
                
            }
        }

        return foundPieces;
    }

    private bool IsColorBomb(GamePiece gamePiece)
    {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if (bomb != null)
            return (bomb.bombType == BombType.Color);

        return false;
    }





    List<GamePiece> FindCollectiblesAt(int row, bool clearedAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if (m_allGamePieces[i, row] != null)
            {
                Collectible collectibleComponent = m_allGamePieces[i, row].GetComponent<Collectible>();
                
                if (collectibleComponent != null)
                    if (!clearedAtBottomOnly || (clearedAtBottomOnly && collectibleComponent.clearedAtBottom))
                        foundCollectibles.Add(m_allGamePieces[i, row]);
            }
        }

        return foundCollectibles;
    }

    List<GamePiece> FindAllCollectibles()
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            List<GamePiece> collectibleRow = FindCollectiblesAt(i);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }

        return foundCollectibles;
    }

    private bool CanAddCollectibles()
    {
        return Random.Range(0, 0.1f) <= chanceForCollectible && collectiblePrefabs.Length > 0 && collectibleCount < maxCollectibles;
    }

    List<GamePiece> RemoveCollectibles(List<GamePiece> bombedPieces)
    {
        List<GamePiece> collectedPieces = FindAllCollectibles();
        List<GamePiece> piecesToRemove = new List<GamePiece>();

        foreach (GamePiece piece in collectedPieces)
        {
            Collectible collectibleComponent = piece.GetComponent<Collectible>();
            
            if (collectibleComponent != null)
                if (!collectibleComponent.clearedByBomb)
                    piecesToRemove.Add(piece);
        }

        return bombedPieces.Except(piecesToRemove).ToList();
    }
}





[System.Serializable]
public class StartingObject
{
    public GameObject prefab;
    public int x;
    public int y;
    [HideInInspector] public int z;
}