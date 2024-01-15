using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using TMPro;

public class SpeechToText : MonoBehaviour
{
    public TMP_InputField fullscreenInputField;
    public TMP_InputField desktopInputField;
    public Button sendButton;
    public Button recordButton;

    // 1. Go to https://portal.azure.com/
    // 2. Click "Create a resource", search and create "Azure AI services"
    // 3. Go to "Keys and Endpoint" under "Resource Management" to get the API key
    private string SpeechServiceAPIKey = ""; // API key for Azure AI service API
    private string SpeechServiceRegion = ""; // Region for Azure AI service instance

    private object threadLocker = new object();
    private bool isRecording = false;
    private bool recordingFinished = false;
    private string message;
    private TMP_InputField inputField;

    void Start()
    {
        inputField = fullscreenInputField;
    }

    void Update()
    {
        lock (threadLocker)
        {
            if (isRecording) 
            {
                sendButton.interactable = false;
                recordButton.interactable = false;
                inputField.interactable = false;
                inputField.text = "Recording...";
            }
            if (recordingFinished)
            {
                inputField.text = message;
                sendButton.interactable = true;
                recordButton.interactable = true;
                inputField.interactable = true;
                recordingFinished = false;
            }
        }
    }

    public async void StartRecording()
    {
        SpeechConfig config = SpeechConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
        config.SpeechRecognitionLanguage = "en-HK";

        // Make sure to dispose the recognizer after use!
        using (SpeechRecognizer recognizer = new SpeechRecognizer(config))
        {
            Debug.Log("Start recording");
            lock (threadLocker)
            {
                isRecording = true;
            }

            // Starts speech recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result.
            RecognitionResult result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

            // Checks result.
            string newMessage = string.Empty;
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                Debug.Log(result.Text);
                newMessage = result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Debug.Log("No input");
                newMessage = "";
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                CancellationDetails cancellation = CancellationDetails.FromResult(result);
                Debug.LogFormat($"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}");
            }

            lock (threadLocker)
            {
                message = newMessage;
                isRecording = false;
                recordingFinished = true;
            }
        }
    }

    public void SwitchUI(bool isFullscreen)
    {
        if (isFullscreen)
        {
            inputField = fullscreenInputField;
        }
        else
        {
            inputField = desktopInputField;
        }
    }
}
