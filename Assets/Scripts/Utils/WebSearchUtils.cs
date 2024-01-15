using UnityEngine;
using Syrus.Plugins.DFV2Client;

public class WebSearchUtils
{
    public static CustomSearch customSearch;

    // search websites
    public static void WebsiteSearch(DF2Response response)
    {
        string query = response.queryResult.parameters["query"].ToString();
        string label = response.queryResult.parameters["label"].ToString();

        if (!string.IsNullOrEmpty(query))
        {
            if (string.IsNullOrEmpty(label))
            {
                customSearch.WebsiteSearchWithoutLabel(query);        
            }
            else
            {
                customSearch.WebsiteSearchWithLabel(query, label);
            }
        }
    }

    // search images
    public static void ImageSearch(DF2Response response)
    {
        string query = response.queryResult.parameters["query"].ToString();

        if (!string.IsNullOrEmpty(query))
        {
            customSearch.ImageSearch(query);
        }
    }
}
