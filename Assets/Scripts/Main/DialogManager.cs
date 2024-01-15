using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Syrus.Plugins.DFV2Client;
using System.IO;
using System.Globalization;
using System;
using UnityEngine.Networking;
using System.Linq;
using Mapbox.Unity.Map;
using Mapbox.Examples;
using Mapbox.Unity.MeshGeneration.Factories;

public class DialogManager : MonoBehaviour
{
    public MainSceneManager mainSceneManager;
    public GameObject assistant;
    public GameObject textPrefab;
    public GameObject websiteSearchResultsPrefab;
    public GameObject imageSearchResultsPrefab;
    public GameObject imagePrefab;
    public GameObject resultPrefab;
    public GameObject weatherPrefab;
    public Transform mapAndCamera;
    public ScrollRect scrollRect;
    public GameObject dialogWindow;
    public SpeechBubble speechBubble;

    [Header("Map Panel Prefab")]
    public GameObject fullscreenMapPanelPrefab;
    public GameObject desktopMapPanelPrefab;
    private GameObject mapPanelPrefab;

    [Space]
    public GameObject emailFormPrefab;
    public GameObject settingsPanelPrefab;

    [Header("Settings Button")]
    public Button fullscreenSettingsButton;
    public Button desktopSettingsButton;
    private Button settingsButton;

    [Header("Input Field")]
    public TMP_InputField fullscreenInputField;
    public TMP_InputField desktopInputField;
    private TMP_InputField inputField;

    [Header("UI")]
    public Transform fullscreenUI;
    public Transform desktopUI;
    private Transform UI;

    private DialogFlowV2Client client;
    private AzureMaps azureMaps;
    private CustomSearch customSearch;
    private OpenCage openCage;
    private AssistantBehaviour assistantBehaviour;
    private SpeechToText speechToText;

    private List<GameObject> messages = new List<GameObject>();
    private bool canSendMessage = true;

    void Start()
    {
        mapPanelPrefab = fullscreenMapPanelPrefab;
        settingsButton = fullscreenSettingsButton;
        inputField = fullscreenInputField;
        UI = fullscreenUI;

        client = GetComponent<DialogFlowV2Client>();
        azureMaps = GetComponent<AzureMaps>();
        customSearch = GetComponent<CustomSearch>();
        openCage = GetComponent<OpenCage>();
        assistantBehaviour = assistant.GetComponent<AssistantBehaviour>();
        speechToText = GetComponent<SpeechToText>();

        client.CreateSession(); // generate session ID to identify different users
        client.ChatbotResponded += Chat;
        client.DetectIntentError += LogError;
        client.RetryFailError += RetryFail;

        azureMaps.ErrorHandler += LogError;

        customSearch.WebsiteSearchResponse += GetWebsiteResults;
        customSearch.ImageSearchResponse += GetImageResults;
        customSearch.ErrorHandler += LogError;

        openCage.MapResponse += MapUtils.MoveMap;
        openCage.RouteResponse += MapUtils.ShowRoute;

        DateTimeUtils.client = client;
        DateTimeUtils.azureMaps = azureMaps;
        WeatherUtils.client = client;
        WeatherUtils.azureMaps = azureMaps;
        WebSearchUtils.customSearch = customSearch;
        MapUtils.client = client;
        MapUtils.openCage = openCage;
        MapUtils.map = mapAndCamera.GetChild(0).GetComponent<AbstractMap>();
        MapUtils.spawnOnMap = mapAndCamera.GetChild(0).GetComponent<SpawnOnMap>();
        MapUtils.route = mapAndCamera.GetChild(2).gameObject;
        MapUtils.directionsFactory = mapAndCamera.GetChild(2).GetComponent<DirectionsFactory>();
        MapUtils.mapCamera = mapAndCamera.GetChild(1).GetComponent<Camera>();
        MapUtils.MapResponse = OpenMap;
        EmailUtils.client = client;
        EmailUtils.EmailResponse = OpenEmailForm;

        if (!SystemManager.Instance.greeted)
        {
            client.DetectIntentFromEvent("Welcome", null);
            SystemManager.Instance.greeted = true;
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            AddMessage();
        }
    }

    // assistant talks to user
    public void Chat(DF2Response response)
    {
        if (!string.IsNullOrEmpty(response.queryResult.fulfillmentText))
        {
            byte[] audioBytes = Convert.FromBase64String(response.OutputAudio);
            AudioClip clip = WavUtility.ToAudioClip(audioBytes);

            if (!HasEmotion(response))
            {
                assistantBehaviour.Speak(clip);
                AddMessage(response.queryResult.fulfillmentText, "ai", clip.length, response.queryResult.action);                
                
                inputField.ActivateInputField();
            }
        }
        
        Debug.Log("action: " + response.queryResult.action);
        CheckAction(response);
    }

    // check if assistant should show facial expression
    private bool HasEmotion(DF2Response response)
    {
        string emotion = response.queryResult.action;
        if (string.IsNullOrEmpty(emotion))
        {
            return false;
        }
        else if (!assistantBehaviour.audioEmotionConf.ContainsKey(emotion))
        {
            return false;
        }
        else
        {
            assistantBehaviour.SetExpressionIndex(emotion);
            Dictionary<string, object> param = new Dictionary<string, object>() { { "resp", response.queryResult.fulfillmentText } };
            client.DetectIntentFromEvent("RespEmotion", param, assistantBehaviour.audioEmotionConf[emotion]);
            return true;
        }
    }

    // check which action should the assistant perform
    private void CheckAction(DF2Response response)
    {
        switch (response.queryResult.action)
        {
            case "quit":
                mainSceneManager.Quit();
                break;

            case "date.get":
                DateTimeUtils.GetDate(response);
                break;

            case "date.check":
                DateTimeUtils.CheckDate(response);
                break;

            case "date.day_of_week.get":
                DateTimeUtils.GetDayOfWeek(response);
                break;

            case "date.day_of_week.check":
                DateTimeUtils.CheckDayOfWeek(response);
                break;

            case "date.year.get":
                DateTimeUtils.GetYear(response);
                break;

            case "time.get":
                DateTimeUtils.GetTime(response);
                break;

            case "weather.get":
                if (!string.IsNullOrEmpty(response.queryResult.parameters["location"].ToString()))
                {
                    azureMaps.CurrentConditionsResponse += AddWeatherMessage;
                    WeatherUtils.GetWeather(response);
                }
                break;

            case "weather.temperature.get":
                WeatherUtils.GetTemperature(response);
                break;

            case "web.search":
                WebSearchUtils.WebsiteSearch(response);
                break;

            case "web.image.search":
                WebSearchUtils.ImageSearch(response);
                break;

            case "maps.search":
                MapUtils.ShowMap(response);
                break;

            case "maps.route":
                MapUtils.ShowRoute(response);
                break;

            case "maps.poi":
                MapUtils.ShowPOI(response);
                break;

            case "maps.traffic":
                MapUtils.ShowTrafficCongestion(response);
                break;

            case "email.send":
                EmailUtils.CheckEmailSettings();
                break;

            case "email.no_account":
                OpenSettingsPanel(true);
                break;

            case "game.play":
                mainSceneManager.PlayGame(response);
                break;
        }
    }

    // put message into dialog window
    public void AddMessage()
    {
        if (canSendMessage && inputField.text != "")
        {
            canSendMessage = false;
            string input = inputField.text;
            inputField.text = ""; // clear input field
            AddMessage(input, "user");
            client.DetectIntentFromText(input);
        }
    }

    private void AddMessage(string message, string sender = "", float messageDuration = 0.0f, string action = "")
    {
        if (messages.Count > 10)
        {
            Destroy(messages[0].gameObject);
            messages.Remove(messages[0]);
        }

        GameObject textObj = Instantiate(textPrefab, dialogWindow.transform);
        TMP_InputField newText = textObj.GetComponent<TMP_InputField>();
        Transform textArea = textObj.transform.GetChild(0);
        message = char.ToUpper(message[0]) + message.Substring(1);
        Debug.Log(message);

        if (sender == "user")
        {
            newText.text = message;
            textArea.GetChild(textArea.childCount - 1).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineRight;
        }
        else if(sender == "ai")
        {
            newText.text = "<color=#FF0000>" + message + "</color>";
            textArea.GetChild(textArea.childCount - 1).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            newText.text = "<color=#FF0000>" + message + "</color>";
            textArea.GetChild(textArea.childCount - 1).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Midline;
        }

        if (sender == "ai")
        {
            canSendMessage = true;
        }
        messages.Add(newText.gameObject);

        // show message in bubble in desktop mode
        if (!SystemManager.Instance.isFullscreen && sender == "ai")
        {
            speechBubble.gameObject.SetActive(true);
            speechBubble.SetMessage(message, messageDuration, action != "weather.resp");
        }

        // scroll to the bottom of dialog window
        scrollRect.velocity += new Vector2(0f, 1000f);
    }

    // put weather information into dialog window
    private void AddWeatherMessage(MapsCurrentConditionsResponse response, DF2Response df2response)
    {
        azureMaps.CurrentConditionsResponse -= AddWeatherMessage;

        if (messages.Count > 10)
        {
            Destroy(messages[0].gameObject);
            messages.Remove(messages[0]);
        }

        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        string location = textInfo.ToTitleCase(df2response.queryResult.parameters["location"].ToString());

        StartCoroutine(AddWeatherMessage(response, location));
    }

    private IEnumerator AddWeatherMessage(MapsCurrentConditionsResponse response, string location)
    {
        while (!canSendMessage)
        {
            yield return null;
        }

        string phrase = response.results[0].phrase;
        int iconCode = response.results[0].iconCode;
        string temperature = response.results[0].temperature["value"].ToString();
        int cloudCover = response.results[0].cloudCover;
        int relativeHumidity = response.results[0].relativeHumidity;

        GameObject weatherObj = Instantiate(weatherPrefab, dialogWindow.transform);

        Image icon = weatherObj.transform.GetChild(0).GetComponent<Image>();
		Texture2D texture = Resources.Load<Texture2D>($"Weather_Icons/" + iconCode); // load weather icon
        icon.sprite = Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f));

        TMP_Text weatherInfo1 = weatherObj.transform.GetChild(1).GetComponent<TMP_Text>();
        weatherInfo1.text = string.Format("{0}\n{1}, {2}°C", location, phrase, temperature);

        TMP_Text weatherInfo2 = weatherObj.transform.GetChild(2).GetComponent<TMP_Text>();
        weatherInfo2.text = string.Format("Humidity: {0}%\nCloud cover: {1}%", relativeHumidity, cloudCover);

        messages.Add(weatherObj);

        if (!SystemManager.Instance.isFullscreen)
        {
            speechBubble.gameObject.SetActive(true);
            speechBubble.SetWeatherMessage(weatherPrefab);
        }

        // scroll to the bottom
        scrollRect.velocity += new Vector2(0f, 1000f);
    }

    // show websites
    public void GetWebsiteResults(CustomSearchResponse response, string query)
    {
        if (response.items == null)
        {
            client.DetectIntentFromEvent("WebSearchRespNo", null);
        }
        else
        {
            inputField.interactable = false;
            client.DetectIntentFromEvent("WebSearchRespYes", null);

            GameObject searchResultsObj = Instantiate(websiteSearchResultsPrefab, UI);
            TMP_Text title = searchResultsObj.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            title.text += " " + query;
            Button closeButton = searchResultsObj.transform.GetChild(0).GetChild(1).GetComponent<Button>();
            // click close button to remove searchResultsObj
            closeButton.onClick.AddListener(() => {
                inputField.interactable = true;
                canSendMessage = true;
                Destroy(searchResultsObj);
            });

            Transform transform = searchResultsObj.transform.GetChild(1).GetChild(0).GetChild(0);
            foreach (CustomSearchItem item in response.items)
            {
                GameObject resultObj = Instantiate(resultPrefab, transform);
                resultObj.GetComponent<Button>().onClick.AddListener(() => Application.OpenURL(item.link));
                TMP_Text text = resultObj.transform.GetChild(0).GetComponent<TMP_Text>();
                text.text = "<b><u>" + item.title + "</u></b>\n" + item.snippet.Replace("\n", "").Replace("\r", "");
            }
        }
    }

    // show images
    public void GetImageResults(CustomSearchResponse response, string query)
    {
        if (response.items == null)
        {
            client.DetectIntentFromEvent("WebSearchRespNo", null);
        }
        else
        {
            inputField.interactable = false;
            client.DetectIntentFromEvent("WebSearchRespYes", null);
            StartCoroutine(GetImageResults(response.items, query));
        }
    }

    public IEnumerator GetImageResults(CustomSearchItem[] items, string query)
    {
        GameObject searchResultsObj = Instantiate(imageSearchResultsPrefab, UI);
        TMP_Text title = searchResultsObj.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        title.text += " " + query;

        Button closeButton = searchResultsObj.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        // click close button to remove searchResultsObj
        closeButton.onClick.AddListener(() => {
            inputField.interactable = true;
            canSendMessage = true;
            Destroy(searchResultsObj);
        });

        List<UnityWebRequestAsyncOperation> requests = new List<UnityWebRequestAsyncOperation>();
        foreach (CustomSearchItem item in items)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(item.image["thumbnailLink"].ToString());

            // starts the request but doesn't wait for it
            requests.Add(request.SendWebRequest());
        }

        yield return new WaitUntil(() => requests.All(request => request.isDone)); // wait for all requests

        Transform rowTransform = searchResultsObj.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0);
        foreach (UnityWebRequestAsyncOperation request in requests)
        {
            UnityWebRequest r = request.webRequest;
            if (r.isNetworkError || r.isHttpError)
            {
                Debug.Log("Error:\n" + r.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)r.downloadHandler).texture;
                GameObject imageObj = Instantiate(imagePrefab, rowTransform);
                imageObj.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                imageObj.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
            }
        }
        searchResultsObj.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Flexbox>().enabled = true;
    }

    // show map
    public void OpenMap(MapUtils.ShowItem showItem, string[] paramList)
    {
        GetInputField().interactable = false;
        GameObject mapPanelObj = Instantiate(mapPanelPrefab, UI);
        MapPanel mapPanel = mapPanelObj.GetComponent<MapPanel>();        

        mapPanel.closeButton.onClick.AddListener(() => {
            inputField.interactable = true;
            canSendMessage = true;
            MapUtils.mapIsShown = false;
            MapUtils.HideRoute();
            MapUtils.HideAllPOI(mapPanel);
            MapUtils.HideAllTrafficCongestion(mapPanel);
            Destroy(mapPanelObj);
        });

        if (showItem == MapUtils.ShowItem.Route)
        {
            mapPanel.routeButtonBehaviour.SetColor();
        }
        else if (showItem == MapUtils.ShowItem.POI)
        {
            MapUtils.ShowPOI(paramList, mapPanel);
        }
        else if (showItem == MapUtils.ShowItem.TrafficCongestion)
        {
            MapUtils.ShowTrafficCongestion(paramList, mapPanel);
        }
    }

    // show mail form
    public void OpenEmailForm()
    {
        GetInputField().interactable = false;
        GameObject emailFormObj = Instantiate(emailFormPrefab, UI);
        EmailForm emailForm = emailFormObj.GetComponent<EmailForm>();

        emailForm.sendButton.onClick.AddListener(() => {
            if (emailForm.Send())
            {
                inputField.interactable = true;
                canSendMessage = true;
                Destroy(emailFormObj);
            }
        });

        emailForm.closeButton.onClick.AddListener(() => {
            inputField.interactable = true;
            canSendMessage = true;
            Destroy(emailFormObj);
        });
    }

    // show settings panel
    public void OpenSettingsPanel(bool needToSend)
    {
        settingsButton.enabled = false;
        inputField.interactable = false;
        GameObject settingsPanelObj = Instantiate(settingsPanelPrefab, UI);
        SettingsPanel settingsPanel = settingsPanelObj.GetComponent<SettingsPanel>();

        settingsPanel.saveButton.onClick.AddListener(() => {
            if (settingsPanel.SaveEmailAccount())
            {
                inputField.interactable = true;
                canSendMessage = true;
                settingsButton.enabled = true;
                Destroy(settingsPanelObj);

                if (needToSend)
                {
                    client.DetectIntentFromEvent("EmailSend", null);
                }
            }
        });

        settingsPanel.closeButton.onClick.AddListener(() => {
            inputField.interactable = true;
            canSendMessage = true;
            settingsButton.enabled = true;
            Destroy(settingsPanelObj);
        });
    }

    public void ToggleInputField()
    {
        inputField.gameObject.SetActive(!inputField.gameObject.activeSelf);
    }

    private void LogError(DF2ErrorResponse response)
    {
        Debug.LogError("Error " + response.error.code.ToString() + ": " + response.error.message);
    }

    private void RetryFail()
    {
        StartCoroutine(ShowErrorMessage());
    }

    private IEnumerator ShowErrorMessage()
    {
        AddMessage("Server Busy!");

        // remove error message after 2 seconds
        yield return new WaitForSeconds(2.0f);
        Destroy(messages[messages.Count - 1].gameObject);
        messages.Remove(messages[messages.Count - 1]);
        canSendMessage = true;
    }

    private void LogError(MapsErrorResponse response)
    {
        Debug.LogError("Error " + response.error.code + ": " + response.error.message);
    }

    private void LogError(CustomSearchErrorResponse response)
    {
        Debug.LogError("Error " + response.error.code + ": " + response.error.message);
    }

    // callback for quit button
    public void Quit()
    {
        client.DetectIntentFromEvent("Quit", null); ;
    }

    // toggle between fullscreen and desktop mode
    public void SwitchUI(bool isFullscreen)
    {
        if (isFullscreen)
        {
            mapPanelPrefab = fullscreenMapPanelPrefab;
            settingsButton = fullscreenSettingsButton;
            fullscreenInputField.interactable = true;
            desktopInputField.text = "";
            desktopInputField.interactable = false;
            inputField = fullscreenInputField;
            UI = fullscreenUI;
        } 
        else
        {
            mapPanelPrefab = desktopMapPanelPrefab;
            settingsButton = desktopSettingsButton;
            desktopInputField.interactable = true;
            fullscreenInputField.text = "";
            fullscreenInputField.interactable = false;
            inputField = desktopInputField;
            UI = desktopUI;
        }
        speechToText.SwitchUI(isFullscreen);
    }

    // get the reference of input field in the current UI
    private TMP_InputField GetInputField()
    {
        TMP_InputField tmpInputfield;

        if (SystemManager.Instance.isFullscreen)
        {
            tmpInputfield = GameObject.Find("FullscreenUI").transform.GetChild(3).GetComponent<TMP_InputField>();
        }
        else
        {
            tmpInputfield = GameObject.Find("DesktopUI").transform.GetChild(1).GetComponent<TMP_InputField>();
        }

        return tmpInputfield;
    }

    void OnApplicationQuit()
    {
        client.ClearSession(); // clear session ID
    }
}
