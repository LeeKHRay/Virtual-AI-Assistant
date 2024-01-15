using UnityEngine;
using Syrus.Plugins.DFV2Client;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Examples;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

public class MapUtils : MonoBehaviour
{
    public enum ShowItem
    {
        None, Route, POI, TrafficCongestion
    }

    public static DialogFlowV2Client client;
    public static OpenCage openCage;
    public static AbstractMap map;
    public static SpawnOnMap spawnOnMap;
    public static GameObject route;
    public static DirectionsFactory directionsFactory;
    public static Camera mapCamera;

    public static Action<ShowItem, string[]> MapResponse;

    public static bool mapIsShown = false;
    public static bool routeIsShown = false;

    public static void ShowMap(DF2Response response)
    {
        string location = response.queryResult.parameters["location"].ToString();
        if (!string.IsNullOrEmpty(location))
        {
            openCage.GetLatLon(location, ShowItem.None, null);
        }
        else
        {
            MoveMap(SystemManager.Instance.homeLatLon, ShowItem.None, null); // move to home location
        }
    }

    public static void MoveMap(OpenCageResponse resp, ShowItem showItem, string[] paramList)
    {
        if (resp.results.Length == 0) // location does not exists
        {
            client.DetectIntentFromEvent("MapSearchFail", null);
        }
        else
        {
            MoveMap(resp.results[0].geometry["lat"], resp.results[0].geometry["lng"], showItem, paramList);
        }
    }

    public static void MoveMap(float lat, float lon, ShowItem showItem, string[] paramList)
    {
        MoveMap(new Vector2d(lat, lon), showItem, paramList);
    }
    
    public static void MoveMap(Vector2d latLon, ShowItem showItem, string[] paramList)
    {
        map.UpdateMap(latLon, 16); // move to (lat, lon)

        if (!mapIsShown)
        {
            mapIsShown = true;
            MapResponse?.Invoke(showItem, paramList);
        }
    }

    public static void GoToHome()
    {
        map.UpdateMap(SystemManager.Instance.homeLatLon, 16);  // move to home location and zoom to level 16
    }

    public static IEnumerator SetHome(Transform mapImage, ButtonBehaviour buttonBehaviour, GameObject prompt)
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosScreen = Input.mousePosition;
                RectTransform rectTransform = mapImage.GetComponent<RectTransform>();

                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosScreen))
                {
                    // local coordinates
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosScreen, null, out localPoint);

                    // convert to viewport coordinates, ranged from (0, 0) to (1, 1)
                    localPoint.x /= rectTransform.rect.width;
                    localPoint.y /= rectTransform.rect.height;

                    // cast a ray from map camera to find the intersection on the map
                    Ray ray = mapCamera.ViewportPointToRay(localPoint);
                    Plane plane = new Plane(Vector3.up, Vector3.zero); // map lies on x-z plane
                    float distance;
                    plane.Raycast(ray, out distance);
                    Vector3 hit = ray.GetPoint(distance);
                    
                    Vector3 worldPos = new Vector3(hit.x, mapCamera.transform.localPosition.y, hit.z); // world position on map

                    
                    Vector2d latLon = map.WorldToGeoPosition(worldPos); // convert world position to lat, lon
                    spawnOnMap.SetHomeIconLatLon(latLon);
                    SystemManager.Instance.SaveHomeLatLon(latLon);                    
                }
                buttonBehaviour.SetColor();
                prompt.SetActive(false);
                yield break;
            }

            // right click to cancel
            if (Input.GetMouseButtonDown(1))
            {
                buttonBehaviour.SetColor();
                prompt.SetActive(false);
                yield break;
            }

            yield return null;
        }
    }

    public static void ShowRoute(DF2Response response)
    {
        string from = response.queryResult.parameters["from"].ToString();
        string to = response.queryResult.parameters["to"].ToString();
        if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
        {
            Debug.Log(from + ", " + to);
            openCage.GetRouteLatLon(from, to);
        }
    }

    public static void ShowRoute(List<OpenCageResponse> responses, bool[] isHome)
    {
        Vector2d[] latLon = new Vector2d[2];
        if(isHome[0])
        {
            Debug.Log(SystemManager.Instance.homeLatLon);
            Debug.Log(responses);
            latLon[0] = SystemManager.Instance.homeLatLon;
            float lat = responses[0].results[0].geometry["lat"];
            float lon = responses[0].results[0].geometry["lng"];
            latLon[1] = new Vector2d(lat, lon);
        }
        else if (isHome[1])
        {
            latLon[1] = SystemManager.Instance.homeLatLon;
            float lat = responses[0].results[0].geometry["lat"];
            float lon = responses[0].results[0].geometry["lng"];
            latLon[0] = new Vector2d(lat, lon);
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                float lat = responses[i].results[0].geometry["lat"];
                float lon = responses[i].results[0].geometry["lng"];
                latLon[i] = new Vector2d(lat, lon);
            }
        }
        MoveMap(latLon[0], ShowItem.Route, null);

        routeIsShown = true;
        directionsFactory.SetWaypointsPosition(latLon); // put markers on map
        directionsFactory.SetRouteVisibility(true); // show route on map
    }

    public static void ShowRoute()
    {
        routeIsShown = true;
        directionsFactory.SetWaypointsPosition(new Vector3[] { new Vector3(-20, 0, 0), new Vector3(20, 0, 0) });
        directionsFactory.SetRouteVisibility(true);
    }

    public static void HideRoute()
    {
        routeIsShown = false;
        directionsFactory.SetRouteVisibility(false);
    }

    public static void ShowPOI(DF2Response response)
    {
        DF2ParameterList parameterList = JsonConvert.DeserializeObject<DF2ParameterList>("{\"paramList\": " + response.queryResult.parameters["poi"].ToString() + "}");
        string location = response.queryResult.parameters["location"].ToString();
        if (!string.IsNullOrEmpty(location)) {
            openCage.GetLatLon(location, ShowItem.POI, parameterList.paramList);
        }
        else
        {
            MoveMap(SystemManager.Instance.homeLatLon, ShowItem.POI, parameterList.paramList);
        }
    }


    // show POI labels for the specified categories
    public static void ShowPOI(string[] poiNames, MapPanel mapPanel)
    {   
        if (poiNames[0] == "All") // show POI labels of all categories
        {
            foreach (KeyValuePair<string, Toggle> entry in mapPanel.poiTogglesDict)
            {
                entry.Value.isOn = true;
            }
        }
        else
        {
            foreach (string poiName in poiNames)
            {
                mapPanel.poiTogglesDict[poiName].isOn = true;
            }
        }
    }

    // hide all POI labels
    public static void HideAllPOI(MapPanel mapPanel)
    {
        foreach (KeyValuePair<string, Toggle> entry in mapPanel.poiTogglesDict)
        {
            SetPOIDensity(entry.Key, 9);
            entry.Value.isOn = false;
        }
    }

    public static void SetPOIVisibility(string poiName, bool isVisible)
    {
        VectorLayer vectorLayer = (VectorLayer)map.VectorData;
        if (poiName.Equals("Schools"))
        {
            vectorLayer.SetPointsOfInterestSubLayerActive("Schools", isVisible);
            vectorLayer.SetPointsOfInterestSubLayerActive("Colleges", isVisible);
            vectorLayer.SetPointsOfInterestSubLayerActive("Universities", isVisible);
        }
        else
        {
            vectorLayer.SetPointsOfInterestSubLayerActive(poiName, isVisible);
        }
    }

    // set density of POI labels for the specified category
    public static void SetPOIDensity(string poiName, int density)
    {
        VectorLayer vectorLayer = (VectorLayer)map.VectorData;
        if (poiName.Equals("Schools"))
        {
            vectorLayer.SetPointsOfInterestSubLayerDensity("Schools", density);
            vectorLayer.SetPointsOfInterestSubLayerDensity("Colleges", density);
            vectorLayer.SetPointsOfInterestSubLayerDensity("Universities", density);
        }
        else
        {
            vectorLayer.SetPointsOfInterestSubLayerDensity(poiName, density);
        }
    }

    public static void ShowTrafficCongestion(DF2Response response)
    {
        DF2ParameterList parameterList = JsonConvert.DeserializeObject<DF2ParameterList>("{\"paramList\": " + response.queryResult.parameters["trafficCongestion"].ToString() + "}");
        string location = response.queryResult.parameters["location"].ToString();
        if (!string.IsNullOrEmpty(location))
        {
            openCage.GetLatLon(location, ShowItem.TrafficCongestion, parameterList.paramList);
        }
        else
        {
            MoveMap(SystemManager.Instance.homeLatLon, ShowItem.TrafficCongestion, parameterList.paramList);
        }
    }

    // show traffic congestion regions of the specified levels
    public static void ShowTrafficCongestion(string[] trafficCongestionNames, MapPanel mapPanel)
    {
        foreach (string trafficCongestionName in trafficCongestionNames)
        {
            mapPanel.trafficCongestionTogglesDict[trafficCongestionName].isOn = true;
        }
    }

    // hide all traffic congestion regions
    public static void HideAllTrafficCongestion(MapPanel mapPanel)
    {
        foreach (KeyValuePair<string, Toggle> entry in mapPanel.trafficCongestionTogglesDict)
        {
            entry.Value.isOn = false;
        }
    }

    public static void SetTrafficCongestionVisibility(string trafficCongestionName, bool isVisible)
    {
        VectorLayer vectorLayer = (VectorLayer)map.VectorData;
        vectorLayer.SetFeatureSubLayerActive(trafficCongestionName + "TrafficCongestion", isVisible);
    }
}
