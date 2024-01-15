using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class CustomSearch : MonoBehaviour
{
	public delegate void ErrorResponseHandler(CustomSearchErrorResponse error);
	public event ErrorResponseHandler ErrorHandler;

	public delegate void WebSearchResponseHandler(CustomSearchResponse response, string query);
	public event WebSearchResponseHandler WebsiteSearchResponse;

	public delegate void ImageSearchResponseHandler(CustomSearchResponse response, string query);
	public event ImageSearchResponseHandler ImageSearchResponse;

	// 1. Create a search engine in https://programmablesearchengine.google.com/
	// 2. In the Basics tab under Setup, enable "Image search"
	// 3. The search engine ID can be found in the "Search Engine ID" section in the Basics tab
    private string searchEngineID = ""; // search engine ID

	// 1. Create another search engine
	// 2. Create 2 refinement labels "wiki" and "youtube" in the Refinements tab under Search features
	// 3. In Basics tab, add the site "www.youtube.com" in Sites to search section
	// 4. Choose "Include all pages whose address contains this URL"
	// 5. Tag it with the label "youtube"
	// 6. Add another site "*.wikipedia.org"
	// 7. Choose "Include all pages whose address contains this URL"
	// 8. Tag it with the label "wiki"
	// 9. Search engine ID can be found in the "Search Engine ID" section in the Basics tab
	private string searchEngineRestrictedID = ""; // search engine ID

	// 1. Go to https://developers.google.com/custom-search/v1/introduction
	// 2. Click the "Get a key" button under "Identify your application to Google with API key" to obtain the API key
	private string APIKey = ""; // API key for Custom Search API

	private static readonly string PARAMETRIC_SEARCH_URL =
		"https://www.googleapis.com/customsearch/v1?cx={0}&key={1}&q={2}&num={3}";

	private static readonly string PARAMETRIC_SEARCH_RESTRICTED_URL =
		"https://www.googleapis.com/customsearch/v1/siterestrict?cx={0}&key={1}&q={2}&num={3}";

	public void WebsiteSearchWithoutLabel(string query)
	{
		StartCoroutine(WebsiteSearch(query));
	}

	public void WebsiteSearchWithLabel(string query, string label)
	{
		StartCoroutine(WebsiteSearch(query, label));
	}

	private IEnumerator WebsiteSearch(string query, string label = "", int num = 10)
	{
		// Prepares the HTTP request.
		string url;
		if (label == "")
		{
			string encodedQuery = UnityWebRequest.EscapeURL(query);
			url = string.Format(PARAMETRIC_SEARCH_URL, searchEngineID, APIKey, encodedQuery, num);
		}
		else
		{
			string encodedQuery = UnityWebRequest.EscapeURL(query + " more:" + label);
			url = string.Format(PARAMETRIC_SEARCH_RESTRICTED_URL, searchEngineRestrictedID, APIKey, encodedQuery, num);
		}

		UnityWebRequest request = new UnityWebRequest(url, "GET");
		request.downloadHandler = new DownloadHandlerBuffer();
		yield return request.SendWebRequest();

		// Processes response.
		if (request.isNetworkError || request.isHttpError)
		{
			string response = Encoding.UTF8.GetString(request.downloadHandler.data);
			Debug.Log("Search Response:\n" + response);
			ErrorHandler?.Invoke(JsonConvert.DeserializeObject<CustomSearchErrorResponse>(request.downloadHandler.text));
		}
		else
		{
			string response = Encoding.UTF8.GetString(request.downloadHandler.data);
			Debug.Log("Search Response:\n" + response);
			CustomSearchResponse resp = JsonConvert.DeserializeObject<CustomSearchResponse>(response);
			WebsiteSearchResponse?.Invoke(resp, query);
		}
	}

	public void ImageSearch(string query)
	{
		StartCoroutine(ImageSearch(query, 10));
	}

	private IEnumerator ImageSearch(string query, int num)
	{
		// Prepares the HTTP request.
		string encodedQuery = UnityWebRequest.EscapeURL(query);
		string url = string.Format(PARAMETRIC_SEARCH_URL + "&searchType=image", searchEngineID, APIKey, encodedQuery, num);

		UnityWebRequest request = new UnityWebRequest(url, "GET");
		request.downloadHandler = new DownloadHandlerBuffer();
		yield return request.SendWebRequest();

		// Processes response.
		if (request.isNetworkError || request.isHttpError)
		{
			string response = Encoding.UTF8.GetString(request.downloadHandler.data);
			ErrorHandler?.Invoke(JsonConvert.DeserializeObject<CustomSearchErrorResponse>(request.downloadHandler.text));
		}
		else
		{
			string response = Encoding.UTF8.GetString(request.downloadHandler.data);
			Debug.Log("Search Response:\n" + response);
			CustomSearchResponse resp = JsonConvert.DeserializeObject<CustomSearchResponse>(response);
			ImageSearchResponse?.Invoke(resp, query);
		}
	}
}
