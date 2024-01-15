using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using Syrus.Plugins.DFV2Client;

public class DateTimeUtils
{
    public static DialogFlowV2Client client;
    public static AzureMaps azureMaps;

    // get date
    public static void GetDate(DF2Response response)
    {
        string location = response.queryResult.parameters["location"].ToString();
        if (string.IsNullOrEmpty(location)) // local date
        {
            if (string.IsNullOrEmpty(response.queryResult.parameters["date"].ToString()))
            {
                string date = DateTime.Now.ToString("MMMM d", CultureInfo.CreateSpecificCulture("en-US"));
                Dictionary<string, object> param = new Dictionary<string, object> { { "ans", date } };
                client.DetectIntentFromEvent("DateResp", param);
            }
            else
            {
                string dateStr = response.queryResult.parameters["date"].ToString();
                DateTime queryDate = DateTime.Parse(dateStr).ToUniversalTime().Date;
                string date = queryDate.ToString("MMMM d", CultureInfo.CreateSpecificCulture("en-US"));
                Dictionary<string, object> param = new Dictionary<string, object> { { "ans", date } };

                if (DateTime.Compare(DateTime.Now.Date, queryDate) <= 0)
                    client.DetectIntentFromEvent("DateResp", param); // present tense
                else
                    client.DetectIntentFromEvent("DateRespPast", param); // past tense
            }
        }
        else
        {
            azureMaps.TimezoneResponse += GetDateForLoc;
            azureMaps.LocationToTime(location, response);
        }
    }

    // get date in a location
    public static void GetDateForLoc(MapsTimezoneResponse response, DF2Response df2response)
    {
        azureMaps.TimezoneResponse -= GetDateForLoc;
        string wallTime = response.TimeZones[0].ReferenceTime.WallTime;
        DateTime now = DateTimeOffset.Parse(wallTime).Date;

        DateTime date = now;
        if (!string.IsNullOrEmpty(df2response.queryResult.parameters["date"].ToString()))        
        {
            string dateStr = df2response.queryResult.parameters["date"].ToString();
            DateTime dateTmp = DateTime.Parse(dateStr).ToUniversalTime().Date;
            date = date.Add(dateTmp.Subtract(DateTime.Now.Date));
        }

        string dateAns = date.ToString("MMMM d", CultureInfo.CreateSpecificCulture("en-US"));
        Dictionary<string, object> param = new Dictionary<string, object> { { "ans", dateAns } };

        if (DateTime.Compare(now, date) <= 0)
            client.DetectIntentFromEvent("DateResp", param); // present tense
        else
            client.DetectIntentFromEvent("DateRespPast", param); // past tense
    }

    // check if the date is the same as the date provided by user
    public static void CheckDate(DF2Response response)
    {
        string location = response.queryResult.parameters["location"].ToString();
        if (string.IsNullOrEmpty(location)) // local
        {
            if (string.IsNullOrEmpty(response.queryResult.parameters["date"].ToString()))
            {
                client.DetectIntentFromEvent("Fallback", null);
            }
            else
            {
                string dateStr = response.queryResult.parameters["date"].ToString();
                DateTime date = DateTime.Parse(dateStr).ToUniversalTime().Date;

                if (DateTime.Compare(DateTime.Now.Date, date) == 0)
                {
                    client.DetectIntentFromEvent("CheckDateRespYes", null);
                }
                else
                {
                    client.DetectIntentFromEvent("CheckDateRespNo", null);
                }
            }

        }
        else
        {
            azureMaps.TimezoneResponse += CheckDateForLoc;
            azureMaps.LocationToTime(location, response);
        }
    }

    // check the date in a location
    public static void CheckDateForLoc(MapsTimezoneResponse response, DF2Response df2response)
    {
        azureMaps.TimezoneResponse -= CheckDateForLoc;
        string wallTime = response.TimeZones[0].ReferenceTime.WallTime;

        string dateStr = df2response.queryResult.parameters["date"].ToString();
        DateTime date = DateTime.Parse(dateStr).ToUniversalTime().Date;
        DateTime dateAns = DateTimeOffset.Parse(wallTime).Date;

        if (DateTime.Compare(dateAns, date) == 0)
        {
            client.DetectIntentFromEvent("CheckDateRespYes", null);
        }
        else
        {
            client.DetectIntentFromEvent("CheckDateRespNo", null);
        }
    }

    // get day of the week
    public static void GetDayOfWeek(DF2Response response)
    {
        string location = response.queryResult.parameters["location"].ToString();
        if (string.IsNullOrEmpty(location)) // local
        {
            if (string.IsNullOrEmpty(response.queryResult.parameters["date"].ToString()))
            {
                string dayOfWeek = DateTime.Now.DayOfWeek.ToString();
                Dictionary<string, object> param = new Dictionary<string, object> { { "ans", dayOfWeek } };
                client.DetectIntentFromEvent("DateResp", param);
            }
            else
            {
                string dateStr = response.queryResult.parameters["date"].ToString();
                DateTime queryDate = DateTime.Parse(dateStr).ToUniversalTime().Date;
                string dayOfWeek = queryDate.DayOfWeek.ToString();
                Dictionary<string, object> param = new Dictionary<string, object>
                    { { "ans", dayOfWeek } };

                if (DateTime.Compare(DateTime.Now.Date, queryDate) <= 0)
                    client.DetectIntentFromEvent("DateResp", param);
                else
                    client.DetectIntentFromEvent("DateRespPast", param);
            }
        }
        else
        {
            azureMaps.TimezoneResponse += GetDayOfWeekForLoc;
            azureMaps.LocationToTime(location, response);
        }
    }


    // get day of the week in a location
    public static void GetDayOfWeekForLoc(MapsTimezoneResponse response, DF2Response df2response)
    {
        azureMaps.TimezoneResponse -= GetDayOfWeekForLoc;
        string wallTime = response.TimeZones[0].ReferenceTime.WallTime;
        DateTime now = DateTimeOffset.Parse(wallTime).Date;

        DateTime date = now;
        if (!string.IsNullOrEmpty(df2response.queryResult.parameters["date"].ToString()))
        {
            string dateStr = df2response.queryResult.parameters["date"].ToString();
            DateTime dateTmp = DateTime.Parse(dateStr).ToUniversalTime().Date;
            date = date.Add(dateTmp.Subtract(DateTime.Now.Date));
        }

        string dayOfWeek = date.DayOfWeek.ToString();
        Dictionary<string, object> param = new Dictionary<string, object> { { "ans", dayOfWeek } };

        if (DateTime.Compare(now, date) <= 0)
            client.DetectIntentFromEvent("DateResp", param);
        else
            client.DetectIntentFromEvent("DateRespPast", param);
    }

    public static void CheckDayOfWeek(DF2Response response)
    {     
        string location = response.queryResult.parameters["location"].ToString();
        if (string.IsNullOrEmpty(location))
        {
            string dayOfWeek = response.queryResult.parameters["dayofweek"].ToString();
            string dayOfWeekAns;
            DateTime queryDate;

            if (string.IsNullOrEmpty(response.queryResult.parameters["date"].ToString()))
            {
                queryDate = DateTime.Now.Date;
            }
            else
            {
                string dateStr = response.queryResult.parameters["date"].ToString();
                queryDate = DateTime.Parse(dateStr).ToUniversalTime().Date;
            }
            dayOfWeekAns = queryDate.DayOfWeek.ToString();

            if (dayOfWeek == dayOfWeekAns)
            {
                if (DateTime.Compare(DateTime.Now.Date, queryDate) <= 0)
                    client.DetectIntentFromEvent("CheckDateRespYes", null);
                else
                    client.DetectIntentFromEvent("CheckDateRespPastYes", null);
            }
            else
            {
                if (DateTime.Compare(DateTime.Now.Date, queryDate) <= 0)
                    client.DetectIntentFromEvent("CheckDateRespNo", null);
                else
                    client.DetectIntentFromEvent("CheckDateRespPastNo", null);
            }
        }
        else
        {
            azureMaps.TimezoneResponse += CheckDayOfWeekForLoc;
            azureMaps.LocationToTime(location, response);
        }
    }

    public static void CheckDayOfWeekForLoc(MapsTimezoneResponse response, DF2Response df2response)
    {
        azureMaps.TimezoneResponse -= CheckDayOfWeekForLoc;
        string wallTime = response.TimeZones[0].ReferenceTime.WallTime;
        //Debug.Log(wallTime);
        DateTime now = DateTimeOffset.Parse(wallTime).Date;

        string dayOfWeek = df2response.queryResult.parameters["dayofweek"].ToString();
        string dayOfWeekAns;

        DateTime queryDate = now;
        if (!string.IsNullOrEmpty(df2response.queryResult.parameters["date"].ToString()))
        {
            string dateStr = df2response.queryResult.parameters["date"].ToString();
            DateTime dateTmp = DateTime.Parse(dateStr).ToUniversalTime().Date;
            queryDate = queryDate.Add(dateTmp.Subtract(DateTime.Now.Date));
        }

        dayOfWeekAns = queryDate.DayOfWeek.ToString();

        if (dayOfWeek == dayOfWeekAns)
        {
            if (DateTime.Compare(now, queryDate) <= 0)
                client.DetectIntentFromEvent("CheckDateRespYes", null);
            else
                client.DetectIntentFromEvent("CheckDateRespPastYes", null);
        }
        else
        {
            if (DateTime.Compare(now, queryDate) <= 0)
                client.DetectIntentFromEvent("CheckDateRespNo", null);
            else
                client.DetectIntentFromEvent("CheckDateRespPastNo", null);
        }
    }

    // get current year
    public static void GetYear(DF2Response response)
    {
        int year = DateTime.Now.Year;
        Dictionary<string, object> param = new Dictionary<string, object> { { "ans", year } };
        client.DetectIntentFromEvent("DateResp", param);
    }

    // get time
    public static void GetTime(DF2Response response)
    {
        string location = response.queryResult.parameters["location"].ToString();
        if (string.IsNullOrEmpty(location)) // local
        {
            string time = DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("en-US"));
            Dictionary<string, object> param = new Dictionary<string, object> { { "ans", time } };
            client.DetectIntentFromEvent("TimeResp", param);
        }
        else
        {
            azureMaps.TimezoneResponse += GetTimeForLoc;
            azureMaps.LocationToTime(location, response);            
        }
    }
    
    // get time in a location
    public static void GetTimeForLoc(MapsTimezoneResponse response, DF2Response df2response)
    {
        azureMaps.TimezoneResponse -= GetTimeForLoc;

        string wallTime = response.TimeZones[0].ReferenceTime.WallTime;
        DateTime dateTime = DateTimeOffset.Parse(wallTime).DateTime;
        string time = dateTime.ToString("t", CultureInfo.CreateSpecificCulture("en-US"));

        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        string location = textInfo.ToTitleCase(df2response.queryResult.parameters["location"].ToString());

        Dictionary<string, object> param = new Dictionary<string, object> { { "loc", location }, { "ans", time } };
        client.DetectIntentFromEvent("TimeRespLoc", param);
    }
}
