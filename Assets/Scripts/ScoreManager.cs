using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;





public class ScoreManager : Singleton<ScoreManager>
{
    private int m_currentScore = 0;
    private int m_counterValue = 0;
    private int m_increment = 5;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private Text scoreText;
    [SerializeField] private float countTime = 1;

    
    
    
    
    private void Start()
    {
        scoreText.text = m_currentScore.ToString();
    }

    public void AddScore(int value)
    {
        m_currentScore += value;
        StartCoroutine(CountScoreRoutine());
    }

    private IEnumerator CountScoreRoutine()
    {
        while (m_counterValue < m_currentScore)
        {
            m_counterValue += m_increment;
            scoreText.text = m_counterValue.ToString();
            gameManager.UpdateMoves(m_counterValue);
            yield return new WaitForEndOfFrame();
        }
    }
}
