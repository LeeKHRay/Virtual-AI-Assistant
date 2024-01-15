using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using Syrus.Plugins.DFV2Client;

public class WeatherUtils
{
    public static DialogFlowV2Client client;
    public static AzureMaps azureMaps;

    // get weather information
    public static void GetWeather(DF2Response response)
    {
        string location = response.queryResult.parameters["location"].ToString();
        if (!string.IsNullOrEmpty(location))
        {
            client.DetectIntentFromEvent("WeatherResp", null);
            azureMaps.LocationToWeather(location, response);
        }
    }

    // get temperature
    public static void GetTemperature(DF2Response response)
    {
        string location = response.queryResult.parameters["location"].ToString();
        if (!string.IsNullOrEmpty(location))
        {
            azureMaps.CurrentConditionsResponse += GetTemperature;
            azureMaps.LocationToWeather(location, response);
        }
    }

    public static void GetTemperature(MapsCurrentConditionsResponse response, DF2Response df2response)
    {
        azureMaps.CurrentConditionsResponse -= GetTemperature;

        string temperature = response.results[0].temperature["value"] + "°C";
        Dictionary<string, object> param = new Dictionary<string, object> { { "temp", temperature } };
        client.DetectIntentFromEvent("TemperatureResp", param);
    }
}
