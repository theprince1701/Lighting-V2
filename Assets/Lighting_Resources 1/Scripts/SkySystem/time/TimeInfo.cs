using UnityEngine;
using System;
using UnityEditor.VFX;
using Random = System.Random;

[System.Serializable]
public class TimeInfo
{
     public int year;
     public int month;
     public int day;
     public int hour;
     public int minute;
     public int second;

    public void ConvertFromDateTime(DateTime time)
    {
        year = time.Year;
        month = time.Month;
        day = time.Day;
        hour = time.Hour;
        minute = time.Minute;
        second = time.Second;
    }
    
    public DateTime TimeInfoToDateTime()
    {
        var dateTime = new DateTime(year, month, day, hour, minute, second, 0);

        return dateTime;
    }
}