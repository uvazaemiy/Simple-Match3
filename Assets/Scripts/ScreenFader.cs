using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;





[RequireComponent(typeof(Image))]
public class ScreenFader : MonoBehaviour
{
    private Image m_image;
    
    
    
    

    private void Awake()
    {
        m_image = GetComponent<Image>();
        m_image.enabled = true;
    }

    public void Fade(float alpha, float duration = 1)
    {
        m_image.enabled = true;
        m_image.DOFade(alpha, duration);
        if (alpha == 0)
            StartCoroutine(DisableImage(duration));
    }

    private IEnumerator DisableImage(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_image.enabled = false;
    }
}
