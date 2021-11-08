// Decompiled with JetBrains decompiler
// Type: Json.NETMF.DateTimeExtensions
// Assembly: Json.NetMF, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 734D45F7-78D8-43F9-94B6-41A5EBA2ADBB
// Assembly location: D:\experiment\BMC.DataLogger\packages\Json.NetMF.1.3.0.0\lib\netmf43\Json.NetMF.dll

using System;

namespace Json.TinyCLR
{
  public static class DateTimeExtensions
  {
    /// <summary>
    /// Converts an ISO 8601 time/date format string, which is used by JSON and others,
    /// into a DateTime object.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTime FromIso8601(string date)
    {
      bool flag = StringExtensions.EndsWith(date, "Z");
      string[] strArray = date.Split('T', 'Z', ':', '-', '.', '+');
      string str1 = strArray[0];
      string str2 = strArray.Length > 1 ? strArray[1] : "1";
      string str3 = strArray.Length > 2 ? strArray[2] : "1";
      string str4 = strArray.Length > 3 ? strArray[3] : "0";
      string str5 = strArray.Length > 4 ? strArray[4] : "0";
      string str6 = strArray.Length > 5 ? strArray[5] : "0";
      string str7 = strArray.Length > 6 ? strArray[6] : "0";
      DateTime dateTime = new DateTime(Convert.ToInt32(str1), Convert.ToInt32(str2), Convert.ToInt32(str3), Convert.ToInt32(str4), Convert.ToInt32(str5), Convert.ToInt32(str6), Convert.ToInt32(str7));
      if (!flag && strArray.Length >= 9)
      {
        string str8 = strArray.Length > 7 ? strArray[7] : "";
        string str9 = strArray.Length > 8 ? strArray[8] : "";
        if (StringExtensions.Contains(date, "+"))
        {
          dateTime = dateTime.AddHours(Convert.ToDouble(str8));
          dateTime = dateTime.AddMinutes(Convert.ToDouble(str9));
        }
        else
          dateTime = dateTime.AddHours(-Convert.ToDouble(str8)).AddMinutes(-Convert.ToDouble(str9));
      }
      if (flag)
        dateTime = new DateTime(0L, DateTimeKind.Utc).AddTicks(dateTime.Ticks);
      return dateTime;
    }

    /// <summary>
    /// Converts a DateTime object into an ISO 8601 string.  This version
    /// always returns the string in UTC format.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string ToIso8601(DateTime dt) => dt.Year.ToString() + "-" + DateTimeExtensions.TwoDigits(dt.Month) + "-" + DateTimeExtensions.TwoDigits(dt.Day) + "T" + DateTimeExtensions.TwoDigits(dt.Hour) + ":" + DateTimeExtensions.TwoDigits(dt.Minute) + ":" + DateTimeExtensions.TwoDigits(dt.Second) + "." + DateTimeExtensions.ThreeDigits(dt.Millisecond) + "Z";

    /// <summary>
    /// Ensures a two-digit number with leading zero if necessary.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string TwoDigits(int value) => value < 10 ? "0" + value.ToString() : value.ToString();

    /// <summary>
    /// Ensures a three-digit number with leading zeros if necessary.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string ThreeDigits(int value)
    {
      if (value < 10)
        return "00" + value.ToString();
      return value < 100 ? "0" + value.ToString() : value.ToString();
    }

    /// <summary>
    /// The ASP.NET Ajax team made up their own time date format for JSON strings, and it's
    /// explained in this article: http://msdn.microsoft.com/en-us/library/bb299886.aspx
    /// Converts a DateTime to the ASP.NET Ajax JSON format.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string ToASPNetAjax(DateTime dt) => "\\/Date(" + dt.Ticks.ToString() + ")\\/";

    /// <summary>Converts an ASP.NET Ajax JSON string to DateTime</summary>
    /// <param name="ajax"></param>
    /// <returns></returns>
    public static DateTime FromASPNetAjax(string ajax) => new DateTime(Convert.ToInt64(ajax.Split('(', ')')[1]), DateTimeKind.Utc);
  }
}
