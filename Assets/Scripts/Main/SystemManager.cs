using Mapbox.Examples;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Instance { get; private set; }

    public int imageNum = 0;
    public bool greeted = false;
    public Vector2d homeLatLon;
    public string emailAddress;
    public string password;
    public bool isFullscreen = true;
    public bool prevIsFullscreen = true;

    private string settingsFileDir;
    private TransparentWindow transparentWindow;
    private DialogManager dialogManager;
    private QuadTreeCameraMovement map;

    void Awake()
    {
        Time.timeScale = 1.0f;

        Directory.CreateDirectory(Application.dataPath + "\\Resources\\DialogflowV2"); // create DialogflowV2 folder if it does not exist

        settingsFileDir = Application.dataPath + "\\Resources\\Settings";
        Directory.CreateDirectory(settingsFileDir); // create Settings folder if it does not exist
        LoadHomeLatLon();
        LoadEmailAccount();

        transparentWindow = GetComponent<TransparentWindow>();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void SaveHomeLatLon(Vector2d latLon)
    {
        string path = settingsFileDir + "\\home.dat";
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        sw.WriteLine(latLon.x + "," + latLon.y);
        sw.Close();
        fs.Close();

        homeLatLon = latLon;
    }

    private void LoadHomeLatLon()
    {
        string path = settingsFileDir + "\\home.dat";
        if (File.Exists(path))
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            homeLatLon = Conversions.StringToLatLon(sr.ReadLine());
            sr.Close();
            fs.Close();
        }
        else
        {            
            homeLatLon = new Vector2d(22.420704430233, 114.207384481311);
        }
    }

    public void SaveEmailAccount(string emailAddress, string password)
    {
        string path = settingsFileDir + "\\email.dat";
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        sw.WriteLine(emailAddress);
        sw.WriteLine(Encode(password));
        sw.Close();
        fs.Close();

        this.emailAddress = emailAddress;
        this.password = Encode(password);
    }

    private void LoadEmailAccount()
    {
        string path = settingsFileDir + "\\email.dat";
        if (File.Exists(path))
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            emailAddress = sr.ReadLine();
            password = sr.ReadLine();
            sr.Close();
            fs.Close();
        }
        else
        {
            emailAddress = null;
            password = null;
        }
    }

    public string GetDecodedPassword()
    {
        return Decode(password);
    }

    public void SwitchDisplayMode()
    {
        prevIsFullscreen = !prevIsFullscreen;
        transparentWindow.SwitchDisplayMode();
        dialogManager.SwitchUI(isFullscreen);
        map.SwitchUI(isFullscreen);
    }

    public void SetFullscreenMode()
    {
        transparentWindow.SetFullscreenMode();
    }

    public void SetDesktopMode()
    {
        transparentWindow.SetDesktopMode();
    }

    public void SetReference()
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            dialogManager = GameObject.Find("DialogManager").GetComponent<DialogManager>();

            GameObject.Find("DesktopModeButton").GetComponent<Button>().onClick.AddListener(() => {
                SwitchDisplayMode();
            });

            GameObject.Find("FullscreenModeButton").GetComponent<Button>().onClick.AddListener(() => {
                SwitchDisplayMode();
            });

            transparentWindow.SetReference();
            map = GameObject.Find("Map").GetComponent<QuadTreeCameraMovement>();
        }
    }

    private string Encode(string str)
    {
        return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
    }

    private string Decode(string str)
    {
        return Encoding.ASCII.GetString(Convert.FromBase64String(str));
    }
}
