using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour
{
    public TMP_Text text;
    public int linecharNum = 30;
    private bool isMessageShown = false;

    public void SetMessage(string message, float messageDuration, bool shouldDisable)
    {
        string[] tokens = message.Split(' ');
        message = "";
        int charNum = 0;

        isMessageShown = true;

        // insert newline character if message is too long
        foreach (string token in tokens)
        {
            charNum += token.Length;
            message += token + " ";
            if (charNum >= linecharNum)
            {
                message += "\n";
                charNum = 0;
            }
        }
        text.text = message;
        StartCoroutine(HideMessage(messageDuration, shouldDisable));
    }

    private IEnumerator HideMessage(float messageDuration, bool shouldDisable)
    {
        // hide bubble 1 second after the assistant finishes the speech
        yield return new WaitForSeconds(messageDuration + 1.0f);
        isMessageShown = false;

        if (shouldDisable)
        {
            gameObject.SetActive(false);
        }

    }

    public void SetWeatherMessage(GameObject weatherPrefab)
    {
        GameObject weatherObj = Instantiate(weatherPrefab, transform);
        StartCoroutine(HideWeatherMessage(weatherObj, 5.0f));
    }

    private IEnumerator HideWeatherMessage(GameObject weatherObj, float messageDuration)
    {
        while (isMessageShown)
        {
            yield return null;
        }
        Destroy(weatherObj, messageDuration);
        yield return new WaitForSeconds(messageDuration);
        gameObject.SetActive(false);
    }
}
