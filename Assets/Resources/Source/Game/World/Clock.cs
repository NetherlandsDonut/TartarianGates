using System.Collections.Generic;

using static Season;

public class Clock
{
    public Clock() { }
    public Clock(int year = 1, int season = 1, int day = 1, int hour = 0, int minute = 0, int second = 0)
    {
        this.year = year;
        this.season = season - 1;
        this.day = day - 1;
        this.hour = hour;
        this.minute = minute;
        this.second = second;
    }

    //Current month
    public int year;

    //Current season
    public int season;

    //Current day
    public int day;

    //Current hours of the day
    public int hour;

    //Current minutes of the day
    public int minute;

    //Current seconds of the day
    public int second;

    //Converts the day of the season into a string
    public string DayOf()
    {
        var two = day % 100;
        var one = day % 10;
        if (two / 10 == 1) return day + "th";
        else if (one == 1) return day + "st";
        else if (one == 2) return day + "nd";
        else if (one == 3) return day + "rd";
        else return day + "th";
    }

    //Adds time to the clock
    public void AddTime(int seconds)
    {
        second += seconds;
        var over = second / 60;
        if (over > 0)
        {
            second %= 60;
            minute += over;
            over = minute / 60;
            if (over > 0)
            {
                minute %= 60;
                hour += over;
                over = hour / 24;
                if (over > 0)
                {
                    hour %= 24;
                    day += over;
                    while (day >= seasons[season].days)
                    {
                        day -= seasons[season++].days;
                        if (season + 1 == seasons.Count)
                        {
                            season = 0;
                            year++;
                        }
                    }
                }
            }
        }
    }

    //Gets the amount of sunlight based on the current hour of the day
    public int Sunlight() => sunPowerByHour[hour];

    //Stregth of the sun on the outdoor tiles
    public static Dictionary<int, int> sunPowerByHour = new()
    {
        { 00, 0125 },
        { 01, 0125 },
        { 02, 0125 },
        { 03, 0125 },
        { 04, 0125 },
        { 05, 0125 },
        { 06, 0250 },
        { 07, 0375 },
        { 08, 0500 },
        { 09, 0625 },
        { 10, 0750 },
        { 11, 0875 },
        { 12, 1000 },
        { 13, 1000 },
        { 14, 0875 },
        { 15, 0875 },
        { 16, 0750 },
        { 17, 0750 },
        { 18, 0625 },
        { 19, 0500 },
        { 20, 0375 },
        { 21, 0250 },
        { 22, 0125 },
        { 23, 0125 },
    };
}
