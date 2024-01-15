using Newtonsoft.Json;
using Syrus.Plugins.DFV2Client;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AzureMaps : MonoBehaviour
{
	public delegate void ErrorResponseHandler(MapsErrorResponse error);
	public event ErrorResponseHandler ErrorHandler;

	private delegate void SearchAddressResponseHandler(MapsSearchAddressResponse response, DF2Response df2response);
	private event SearchAddressResponseHandler SearchAddressResponse;

	public delegate void TimezoneResponseHandler(MapsTimezoneResponse response, DF2Response df2response);
	public event TimezoneResponseHandler TimezoneResponse;

	public delegate void CurrentConditionsResponseHandler(MapsCurrentConditionsResponse response, DF2Response df2response);
	public event CurrentConditionsResponseHandler CurrentConditionsResponse;

	// 1. Go to https://portal.azure.com/
	// 2. Click "Create a resource", search and create "Azure Maps"
	// 3. Go to "Authentication" under "Settings" to get the subscription key
	private string subscriptionKey = ""; // subscription key for Azure Maps

	private static readonly string PARAMETRIC_SEARCH_ADDRESS_URL =
		"https://atlas.microsoft.com/search/address/json?subscription-key={0}&api-version=1.0&query={1}&limit=1";

	private static readonly string PARAMETRIC_SEARCH_TIMEZONE_URL = 
		"https://atlas.microsoft.com/timezone/byCoordinates/json?subscription-key={0}&api-version=1.0&options=zoneInfo&query={1}";

	private static readonly string PARAMETRIC_WEATHER_CONDITION_URL =
		"https://atlas.microsoft.com/weather/currentConditions/json?subscription-key={0}&api-version=1.0&query={1}";
	
	public void LocationToTime(string location, DF2Response df2response)
	{
		SearchAddressResponse += Timezone;
		string encodedLocation = UnityWebRequest.EscapeURL(location);
		StartCoroutine(SearchAddress(encodedLocation, df2response));
	}

	public void LocationToWeather(string location, DF2Response df2response)
	{
		SearchAddressResponse += CurrentConditions;
		string encodedLocation = UnityWebRequest.EscapeURL(location);
		StartCoroutine(SearchAddress(encodedLocation, df2response));
	}

	// find latitude and longitude
	private IEnumerator SearchAddress(string location, DF2Response df2response = null)
	{
		// Prepares the HTTP request.
		string url = string.Format(PARAMETRIC_SEARCH_ADDRESS_URL, subscriptionKey, location);
		UnityWebRequest request = new UnityWebRequest(url, "GET");
		request.downloadHandler = new DownloadHandlerBuffer();

		yield return request.SendWebRequest();

		// Processes response.
		if (request.isNetworkError || request.isHttpError)
			ErrorHandler?.Invoke(JsonConvert.DeserializeObject<MapsErrorResponse>(request.downloadHandler.text));
		else
		{
			string response = Encoding.UTF8.GetString(request.downloadHandler.data);
			MapsSearchAddressResponse resp = JsonConvert.DeserializeObject<MapsSearchAddressResponse>(response);
			SearchAddressResponse?.Invoke(resp, df2response);
		}
	}

	private void Timezone(MapsSearchAddressResponse response, DF2Response df2response)
	{
		SearchAddressResponse -= Timezone;

        float lat;
        float lon;
        if (response.results.Length > 0) {
            lat = response.results[0].position["lat"];
            lon = response.results[0].position["lon"];
        }
        else { // use Hong Kong location by default
            lat = 22.28215f;
            lon = 114.1569f;
        }
        Debug.Log(lat + ", " + lon);
        StartCoroutine(Timezone(lat + "," + lon, df2response));
	}

	private IEnumerator Timezone(string query, DF2Response df2response)
	{
		// Prepares the HTTP request.
		string url = string.Format(PARAMETRIC_SEARCH_TIMEZONE_URL, subscriptionKey, query);
		UnityWebRequest request = new UnityWebRequest(url, "GET");
		request.downloadHandler = new DownloadHandlerBuffer();

		yield return request.SendWebRequest();

		// Processes response.
		if (request.isNetworkError || request.isHttpError)
			ErrorHandler?.Invoke(JsonConvert.DeserializeObject<MapsErrorResponse>(request.downloadHandler.text));
		else
		{
			string response = Encoding.UTF8.GetString(request.downloadHandler.data);
			MapsTimezoneResponse resp = JsonConvert.DeserializeObject<MapsTimezoneResponse>(response);
			TimezoneResponse?.Invoke(resp, df2response);
		}
	}

	private void CurrentConditions(MapsSearchAddressResponse response, DF2Response df2response)
	{
		SearchAddressResponse -= CurrentConditions;

        float lat;
        float lon;
        if (response.results.Length > 0) {
            lat = response.results[0].position["lat"];
            lon = response.results[0].position["lon"];
        }
        else { // use Hong Kong location by default
            lat = 22.28215f;
            lon = 114.1569f;
        }
		Debug.Log(lat + ", " + lon);
		StartCoroutine(CurrentConditions(lat + "," + lon, df2response));
	}

	private IEnumerator CurrentConditions(string query, DF2Response df2response)
	{
		// Prepares the HTTP request.
		string url = string.Format(PARAMETRIC_WEATHER_CONDITION_URL, subscriptionKey, query);
		UnityWebRequest request = new UnityWebRequest(url, "GET");
		request.downloadHandler = new DownloadHandlerBuffer();

		yield return request.SendWebRequest();

		// Processes response.
		if (request.isNetworkError || request.isHttpError)
			ErrorHandler?.Invoke(JsonConvert.DeserializeObject<MapsErrorResponse>(request.downloadHandler.text));
		else
		{
			string response = Encoding.UTF8.GetString(request.downloadHandler.data);
			MapsCurrentConditionsResponse resp = JsonConvert.DeserializeObject<MapsCurrentConditionsResponse>(response);
			CurrentConditionsResponse?.Invoke(resp, df2response);
		}
	}
}
