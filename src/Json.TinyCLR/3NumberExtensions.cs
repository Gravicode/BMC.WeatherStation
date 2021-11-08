// Decompiled with JetBrains decompiler
// Type: Json.NETMF.Helper
// Assembly: Json.NetMF, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 734D45F7-78D8-43F9-94B6-41A5EBA2ADBB
// Assembly location: D:\experiment\BMC.DataLogger\packages\Json.NetMF.1.3.0.0\lib\netmf43\Json.NetMF.dll

using System;
using System.Globalization;

namespace Json.TinyCLR
{
  internal static class Helper
  {
    public const int MaxDoubleDigits = 16;

    public static bool IsWhiteSpace(char ch) => ch == ' ';

    public static bool TryParseUInt64Core(
      string str,
      bool parseHex,
      out ulong result,
      out bool sign)
    {
      if (str == null)
        throw new ArgumentNullException(nameof (str));
      if (str.Length >= 2 && str.Substring(0, 2).ToLower() == "0x")
      {
        str = str.Substring(2);
        parseHex = true;
      }
      bool flag = true;
      result = 0UL;
      int length = str.Length;
      int index = 0;
      while (index < length && Helper.IsWhiteSpace(str[index]))
        ++index;
      NumberFormatInfo numberFormat = CultureInfo.CurrentUICulture.NumberFormat;
      string positiveSign = numberFormat.PositiveSign;
      string negativeSign = numberFormat.NegativeSign;
      sign = false;
      while (index < length)
      {
        char ch = str[index];
        if (!parseHex && (int) ch == (int) negativeSign[0])
        {
          sign = true;
          ++index;
        }
        else if (!parseHex && (int) ch == (int) positiveSign[0])
        {
          sign = false;
          ++index;
        }
        else
        {
          if ((!parseHex || (ch < 'A' || ch > 'F') && (ch < 'a' || ch > 'f')) && (ch < '0' || ch > '9'))
            return false;
          break;
        }
      }
      if (index >= length)
        return false;
      uint num1 = 0;
      uint num2 = 0;
      if (parseHex)
      {
        do
        {
          char ch = str[index];
          uint num3;
          if (ch >= '0' && ch <= '9')
            num3 = (uint) ch - 48U;
          else if (ch >= 'A' && ch <= 'F')
            num3 = (uint) ((int) ch - 65 + 10);
          else if (ch >= 'a' && ch <= 'f')
            num3 = (uint) ((int) ch - 97 + 10);
          else
            break;
          if (flag)
          {
            ulong num4 = (ulong) num1 * 16UL;
            ulong num5 = (ulong) num2 * 16UL + (num4 >> 32);
            if (num5 > (ulong) uint.MaxValue)
            {
              flag = false;
            }
            else
            {
              ulong num6 = (num4 & (ulong) uint.MaxValue) + (ulong) num3;
              ulong num7 = num5 + (num6 >> 32);
              if (num7 > (ulong) uint.MaxValue)
              {
                flag = false;
              }
              else
              {
                num1 = (uint) num6;
                num2 = (uint) num7;
              }
            }
          }
          ++index;
        }
        while (index < length);
      }
      else
      {
        do
        {
          char ch = str[index];
          switch (ch)
          {
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
              uint num8 = (uint) ch - 48U;
              if (flag)
              {
                ulong num9 = (ulong) num1 * 10UL;
                ulong num10 = (ulong) num2 * 10UL + (num9 >> 32);
                if (num10 > (ulong) uint.MaxValue)
                {
                  flag = false;
                }
                else
                {
                  ulong num11 = (num9 & (ulong) uint.MaxValue) + (ulong) num8;
                  ulong num12 = num10 + (num11 >> 32);
                  if (num12 > (ulong) uint.MaxValue)
                  {
                    flag = false;
                  }
                  else
                  {
                    num1 = (uint) num11;
                    num2 = (uint) num12;
                  }
                }
              }
              ++index;
              continue;
            default:
              goto label_39;
          }
        }
        while (index < length);
      }
label_39:
      if (index < length)
      {
        while (Helper.IsWhiteSpace(str[index]))
        {
          ++index;
          if (index >= length)
            break;
        }
        if (index < length)
          return false;
      }
      result = (ulong) num2 << 32 | (ulong) num1;
      return flag;
    }
  }
}
