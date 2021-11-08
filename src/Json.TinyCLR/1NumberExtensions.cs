// Decompiled with JetBrains decompiler
// Type: Json.NETMF.Int64Extensions
// Assembly: Json.NetMF, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 734D45F7-78D8-43F9-94B6-41A5EBA2ADBB
// Assembly location: D:\experiment\BMC.DataLogger\packages\Json.NetMF.1.3.0.0\lib\netmf43\Json.NetMF.dll

using System;

namespace Json.TinyCLR
{
  internal static class Int64Extensions
  {
    public static long Parse(string str)
    {
      long result;
      if (Int64Extensions.TryParse(str, out result))
        return result;
      throw new Exception();
    }

    public static long Parse(string str, NumberStyle style) => style == NumberStyle.Hexadecimal ? Int64Extensions.ParseHex(str) : Int64Extensions.Parse(str);

    public static bool TryParse(string str, out long result)
    {
      result = 0L;
      ulong result1;
      bool sign;
      if (Helper.TryParseUInt64Core(str, false, out result1, out sign))
      {
        if (!sign)
        {
          if (result1 <= (ulong) long.MaxValue)
          {
            result = (long) result1;
            return true;
          }
        }
        else if (result1 <= 9223372036854775808UL)
        {
          result = -(long) result1;
          return true;
        }
      }
      return false;
    }

    private static long ParseHex(string str)
    {
      ulong result;
      if (Int64Extensions.TryParseHex(str, out result))
        return (long) result;
      throw new Exception();
    }

    private static bool TryParseHex(string str, out ulong result) => Helper.TryParseUInt64Core(str, true, out result, out bool _);
  }
}
