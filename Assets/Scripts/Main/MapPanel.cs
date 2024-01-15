using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPanel : MonoBehaviour
{
    public GameObject prompt;
    public ButtonBehaviour setHomeButtonBehaviour;
    public ButtonBehaviour routeButtonBehaviour;
    public Toggle poiMenuToggle;
    public GameObject poiMenu;
    public Toggle[] poiToggles;
    public Dictionary<string, Toggle> poiTogglesDict;
    public Slider[] poiDensities;
    public Toggle trafficCongestionMenuToggle;
    public GameObject trafficCongestionMenu;
    public Toggle[] trafficCongestionToggles;
    public Dictionary<string, Toggle> trafficCongestionTogglesDict;
    public Button closeButton;

    void Awake()
    {
        poiTogglesDict = new Dictionary<string, Toggle>();
        for (int i = 0; i < poiToggles.Length; i++)
        {
            int idx = i;
            string poiName = poiToggles[idx].transform.parent.name;
            poiTogglesDict.Add(poiName, poiToggles[idx]);

            poiToggles[idx].onValueChanged.AddListener((isOn) => {
                MapUtils.SetPOIVisibility(poiName, isOn);
            });
            
            poiDensities[idx].onValueChanged.AddListener((value) => {
                MapUtils.SetPOIDensity(poiName, (int)value);
                MapUtils.SetPOIVisibility(poiName, !poiToggles[idx].isOn);
                MapUtils.SetPOIVisibility(poiName, poiToggles[idx].isOn);
            });
        }

        trafficCongestionTogglesDict = new Dictionary<string, Toggle>();
        for (int i = 0; i < trafficCongestionToggles.Length; i++)
        {
            int idx = i;
            string trafficCongestionName = trafficCongestionToggles[idx].transform.parent.name;
            trafficCongestionTogglesDict.Add(trafficCongestionName, trafficCongestionToggles[idx]);

            trafficCongestionToggles[idx].onValueChanged.AddListener((isOn) =>
            {
                MapUtils.SetTrafficCongestionVisibility(trafficCongestionName, isOn);
            });
        }
    }

    public void GoToHome()
    {
        MapUtils.GoToHome();
    }

    public void SetHome()
    {
        setHomeButtonBehaviour.SetColor();
        prompt.SetActive(true);
        StartCoroutine(MapUtils.SetHome(transform.GetChild(0), setHomeButtonBehaviour, prompt));
    }

    public void ToggleRoute()
    {
        routeButtonBehaviour.SetColor();
        if (MapUtils.routeIsShown)
        {
            MapUtils.HideRoute();
        }
        else
        {
            MapUtils.ShowRoute();
        }
    }

    public void TogglePOIMenu(bool isOn)
    {
        poiMenu.SetActive(isOn);
    }

    public void ToggleTrafficCongestionMenu(bool isOn)
    {
        trafficCongestionMenu.SetActive(isOn);
    }

    void OnDestroy()
    {
        foreach (Toggle poiToggle in poiToggles)
        {
            poiToggle.isOn = false;
        }
    }
}
