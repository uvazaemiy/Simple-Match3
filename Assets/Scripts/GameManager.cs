using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;





public class GameManager : Singleton<GameManager>
{
    public int movesLeft = 30;
    [SerializeField] private int scoreGoal = 10000;
    [SerializeField] private float delay = 1;
    [Space]
    [SerializeField] private MessageWindow messageWindow;
    [SerializeField] private Sprite loseIcon;
    [SerializeField] private Sprite winIcon;
    [SerializeField] private Sprite goalIcon;
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 middlePoint;
    [SerializeField] private Vector3 downPoint;
    [Space]
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private ScreenFader screenFaderBlack;
    [SerializeField] private Text levelNameText;
    [SerializeField] private Board m_board;
    [SerializeField] private Text movesLeftText;

    [HideInInspector] public bool isEndGame;


    
    
    
    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (levelNameText != null)
            levelNameText.text = scene.name;
        DOTween.SetTweensCapacity(500, 50);
        
        messageWindow.ShowMessage(winIcon, "SCORE GOAL " + scoreGoal, "START");
        screenFaderBlack.Fade(0, delay / 2);
        messageWindow.MoveWindow(middlePoint, delay);
    }

    public void StartGame()
    {
        if (isEndGame)
        {
            StartCoroutine(ReloadScene());
            return;
        }
        
        UpdateMoves();
        screenFader.Fade(0, delay);
        messageWindow.MoveWindow(downPoint, delay);
        if (m_board != null)
            m_board.SetupAll(delay);
    }

    public void UpdateMoves(int score = 0)
    {
        if (isEndGame)
            return;

        if (movesLeftText != null)
            movesLeftText.text = movesLeft.ToString();

        if (score >= scoreGoal)
        {
            StartCoroutine(EndGame(goalIcon, "YOU WIN", "OK", true));
            return;
        }

        if (movesLeft == 0)
            StartCoroutine(EndGame(loseIcon, "YOU LOSE", "OK", false));
    }

    private IEnumerator WaitForBoardRoutine()
    {
        if (m_board != null)
        {
            while (m_board.isRefilling)
                yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator EndGame(Sprite sprite, string message, string buttonMsg, bool isWin)
    {
        isEndGame = true;
        yield return StartCoroutine(WaitForBoardRoutine());

        if (isWin)
            SoundManager.Instance.PlayWinSound();
        else
            SoundManager.Instance.PlayLoseSound();
        
        messageWindow.ShowMessage(sprite, message, buttonMsg);
        
        yield return new WaitForSeconds(delay);
        screenFader.Fade(1, delay);
        messageWindow.transform.position = startPoint;
        messageWindow.MoveWindow(middlePoint, delay);
    }

    private IEnumerator ReloadScene()
    {
        messageWindow.MoveWindow(downPoint, 1);
        screenFaderBlack.Fade(1, delay);

        yield return new WaitForSeconds(delay);
        DOTween.KillAll();
        SceneManager.LoadScene("Level 1");
        DestroyAllManagers();
    }

    private void DestroyAllManagers()
    {
        Destroy(gameObject);
        Destroy(ScoreManager.Instance.gameObject);
        Destroy(ParticleManager.Instance.gameObject);
        Destroy(SoundManager.Instance.gameObject);
    }
}