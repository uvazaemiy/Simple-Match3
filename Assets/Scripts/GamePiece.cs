using System.Collections;
using DG.Tweening;
using UnityEngine;





public enum MatchValue
{
    Yellow,
    Blue,
    Magenta,
    Indigo,
    Green,
    Teal,
    Red,
    Cyan,
    Wild,
    None
}
public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    private Board m_Board;
    private bool m_isMoving;
    private Tween currentTween;

    public MatchValue matchValue;
    [SerializeField] private int scoreValue = 20;
    [SerializeField] private AudioClip clearSound;
    
    
    
    
    
    public void Init(Board board)
    {
        m_Board = board;
    }
    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
    
    
    
    

    public void Move(int destX, int destY, float timeToMove)
    {
        if (!m_isMoving)
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }
    private IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        m_isMoving = true;
        
        currentTween = transform.DOMove(destination, timeToMove);
        yield return new WaitForSeconds(timeToMove);

        SetCoord((int)destination.x, (int)destination.y);
        m_Board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
        
        m_isMoving = false;
    }

    public void ChangeColor(GamePiece pieceToMatch)
    {
        SpriteRenderer rendererToChange = GetComponent<SpriteRenderer>();
        Color colorToMatch = Color.clear;

        if (pieceToMatch != null)
        {
            SpriteRenderer rendererToMatch = pieceToMatch.GetComponent<SpriteRenderer>();

            if (rendererToMatch != null && rendererToChange != null)
                rendererToChange.color = rendererToMatch.color;
            
            matchValue = pieceToMatch.matchValue;
        }
    }

    public void ScorePoints(int multiplier = 1, int bonus = 0)
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(scoreValue * multiplier + bonus);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayClipAtPoint(clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
    }
    
    public void KillTween()
    {
        currentTween.Kill();
    }
}