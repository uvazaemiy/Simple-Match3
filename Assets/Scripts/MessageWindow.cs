using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour
{
    [SerializeField] private Image messageIcon;
    [SerializeField] private Text messageText;
    [SerializeField] private Text buttonText;





    public void MoveWindow(Vector3 endValue, float duration)
    {
        transform.DOMove(endValue, duration);
    }
    
    public void ShowMessage(Sprite sprite, string message, string buttonMsg)
    {
        if (messageIcon != null)
            messageIcon.sprite = sprite;
        if (messageText != null)
            messageText.text = message;
        if (buttonText != null)
            buttonText.text = buttonMsg;
    }
}