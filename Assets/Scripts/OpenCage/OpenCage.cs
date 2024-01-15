using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OpenCage : MonoBehaviour
{
    public delegate void MapResponseHandler(OpenCageResponse resp, MapUtils.ShowItem showItem, string[] parameterList);
    public event MapResponseHandler MapResponse;

    public delegate void RouteResponseHandler(List<OpenCageResponse> responses, bool[] isHome);
    public event RouteResponseHandler RouteResponse;

    // 1. Create an OpenCage account in https://opencagedata.com/
    // 2. Obtain API key in "Geocoding API" tab in dashboard
    private static readonly string APIKey = ""; // API key for OpenCage Geocoding API

    private static readonly string PARAMETRIC_URL = "https://api.opencagedata.com/geocode/v1/json?key={0}&q={1}&language=en&limit={2}&pretty=1";

    public void GetLatLon(string query, MapUtils.ShowItem showItem, string[] paramList)
    {
        StartCoroutine(GetLatLon(query, 1, showItem, paramList));
    }

    public void GetRouteLatLon(string from, string to)
    {
        StartCoroutine(GetRouteLatLon(from, to, 1));
    }

    private IEnumerator GetLatLon(string query, int limit, MapUtils.ShowItem showItem, string[] paramList)
    {
        // Prepares the HTTP request.
        string encodedQuery = UnityWebRequest.EscapeURL(query);
        string url = string.Format(PARAMETRIC_URL, APIKey, encodedQuery, limit);

        UnityWebRequest request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        // Processes response.
        if (request.isNetworkError || request.isHttpError)
        {
            string response = Encoding.UTF8.GetString(request.downloadHandler.data);
            Debug.Log("Error:\n" + response);
        }
        else
        {
            string response = Encoding.UTF8.GetString(request.downloadHandler.data);
            Debug.Log("Search Response:\n" + response);
            OpenCageResponse resp = JsonConvert.DeserializeObject<OpenCageResponse>(response);
            MapResponse?.Invoke(resp, showItem, paramList);
        }
    }
    private IEnumerator GetRouteLatLon(string from, string to, int limit)
    {
        List<UnityWebRequestAsyncOperation> requests = new List<UnityWebRequestAsyncOperation>();
        List<string> locations = new List<string>();
        locations.Add(from);
        locations.Add(to);

        bool[] isHome = new bool[2];
        for (int i = 0; i < locations.Count; i++)
        {
            if (!locations[i].ToLower().Contains("home"))
            {
                isHome[i] = false;
                string encodedQuery = UnityWebRequest.EscapeURL(locations[i]);
                string url = string.Format(PARAMETRIC_URL, APIKey, encodedQuery, limit);
                UnityWebRequest request = new UnityWebRequest(url, "GET");
                request.downloadHandler = new DownloadHandlerBuffer();
                requests.Add(request.SendWebRequest());
            }
            else
            {
                isHome[i] = true;
            }
        }

        yield return new WaitUntil(() => requests.All(request => request.isDone));

        // Processes response.
        List<OpenCageResponse> responses = new List<OpenCageResponse>();
        foreach (UnityWebRequestAsyncOperation request in requests)
        {
            UnityWebRequest r = request.webRequest;
            if (r.isNetworkError || r.isHttpError)
            {
                string response = Encoding.UTF8.GetString(r.downloadHandler.data);
                Debug.Log("Error:\n" + response);
            }
            else
            {
                string response = Encoding.UTF8.GetString(r.downloadHandler.data);
                Debug.Log(response);
                Debug.Log("Search Response:\n" + response);
                OpenCageResponse resp = JsonConvert.DeserializeObject<OpenCageResponse>(response);
                responses.Add(resp);
            }
        }

        RouteResponse?.Invoke(responses, isHome);
    }
}
