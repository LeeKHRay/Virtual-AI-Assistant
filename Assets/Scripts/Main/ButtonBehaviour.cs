using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    public Color pressedColor;
    public Image buttonImage;

    private Color originalColor;
    private bool isPressed = false;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
    }

    public void SetColor()
    {
        isPressed = !isPressed;
        buttonImage.color = isPressed ? pressedColor : originalColor;
    }
}
