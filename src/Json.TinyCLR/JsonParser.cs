// Decompiled with JetBrains decompiler
// Type: Json.NETMF.JsonParser
// Assembly: Json.NetMF, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 734D45F7-78D8-43F9-94B6-41A5EBA2ADBB
// Assembly location: D:\experiment\BMC.DataLogger\packages\Json.NetMF.1.3.0.0\lib\netmf43\Json.NetMF.dll

using System.Collections;
using System.Globalization;
using System.Text;

namespace Json.TinyCLR
{
  /// <summary>
  /// Parses JSON strings into a Hashtable.  The Hashtable contains one or more key/value pairs
  /// (DictionaryEntry objects).  Each key is the name of a property that (hopefully) exists
  /// in the class object that it represents.  Each value is one of the following:
  ///   Hastable - Another list of one or more DictionaryEntry objects, essentially representing
  ///              a property that is another class.
  ///   ArrayList - An array of one or more objects, which themselves can be one of the items
  ///               enumerated in this list.
  ///   Value Type - an actual value, such as a string, int, bool, Guid, DateTime, etc
  /// </summary>
  internal class JsonParser
  {
    /// <summary>Parses the string json into a value</summary>
    /// <param name="json">A JSON string.</param>
    /// <returns>An ArrayList, a Hashtable, a double, long, a string, null, true, or false</returns>
    public static object JsonDecode(string json)
    {
      bool success = true;
      return JsonParser.JsonDecode(json, ref success);
    }

    /// <summary>
    /// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
    /// </summary>
    /// <param name="json">A JSON string.</param>
    /// <param name="success">Successful parse?</param>
    /// <returns>An ArrayList, a Hashtable, a double, a long, a string, null, true, or false</returns>
    public static object JsonDecode(string json, ref bool success)
    {
      success = true;
      if (json == null)
        return (object) null;
      char[] charArray = json.ToCharArray();
      int index = 0;
      return JsonParser.ParseValue(charArray, ref index, ref success);
    }

    protected static Hashtable ParseObject(char[] json, ref int index, ref bool success)
    {
      Hashtable hashtable = new Hashtable();
      int num1 = (int) JsonParser.NextToken(json, ref index);
      bool flag = false;
      while (!flag)
      {
        switch (JsonParser.LookAhead(json, index))
        {
          case JsonParser.Token.None:
            success = false;
            return (Hashtable) null;
          case JsonParser.Token.ObjectEnd:
            int num2 = (int) JsonParser.NextToken(json, ref index);
            return hashtable;
          case JsonParser.Token.ItemsSeparator:
            int num3 = (int) JsonParser.NextToken(json, ref index);
            continue;
          default:
            string str = JsonParser.ParseString(json, ref index, ref success);
            if (!success)
            {
              success = false;
              return (Hashtable) null;
            }
            if (JsonParser.NextToken(json, ref index) != JsonParser.Token.PropertySeparator)
            {
              success = false;
              return (Hashtable) null;
            }
            object obj = JsonParser.ParseValue(json, ref index, ref success);
            if (!success)
            {
              success = false;
              return (Hashtable) null;
            }
            hashtable[(object) str] = obj;
            continue;
        }
      }
      return hashtable;
    }

    protected static ArrayList ParseArray(char[] json, ref int index, ref bool success)
    {
      ArrayList arrayList = new ArrayList();
      int num1 = (int) JsonParser.NextToken(json, ref index);
      bool flag = false;
      while (!flag)
      {
        switch (JsonParser.LookAhead(json, index))
        {
          case JsonParser.Token.None:
            success = false;
            return (ArrayList) null;
          case JsonParser.Token.ArrayEnd:
            int num2 = (int) JsonParser.NextToken(json, ref index);
            goto label_9;
          case JsonParser.Token.ItemsSeparator:
            int num3 = (int) JsonParser.NextToken(json, ref index);
            continue;
          default:
            object obj = JsonParser.ParseValue(json, ref index, ref success);
            if (!success)
              return (ArrayList) null;
            arrayList.Add(obj);
            continue;
        }
      }
label_9:
      return arrayList;
    }

    protected static object ParseValue(char[] json, ref int index, ref bool success)
    {
      switch (JsonParser.LookAhead(json, index))
      {
        case JsonParser.Token.ObjectBegin:
          return (object) JsonParser.ParseObject(json, ref index, ref success);
        case JsonParser.Token.ArrayBegin:
          return (object) JsonParser.ParseArray(json, ref index, ref success);
        case JsonParser.Token.StringType:
          return (object) JsonParser.ParseString(json, ref index, ref success);
        case JsonParser.Token.NumberType:
          return JsonParser.ParseNumber(json, ref index, ref success);
        case JsonParser.Token.BooleanTrue:
          int num1 = (int) JsonParser.NextToken(json, ref index);
          return (object) true;
        case JsonParser.Token.BooleanFalse:
          int num2 = (int) JsonParser.NextToken(json, ref index);
          return (object) false;
        case JsonParser.Token.NullType:
          int num3 = (int) JsonParser.NextToken(json, ref index);
          return (object) null;
        default:
          success = false;
          return (object) null;
      }
    }

    protected static string ParseString(char[] json, ref int index, ref bool success)
    {
      StringBuilder stringBuilder = new StringBuilder();
      JsonParser.EatWhitespace(json, ref index);
      char ch1 = json[index++];
      bool flag = false;
      while (!flag && index != json.Length)
      {
        char ch2 = json[index++];
        switch (ch2)
        {
          case '"':
            flag = true;
            goto label_19;
          case '\\':
            if (index != json.Length)
            {
              switch (json[index++])
              {
                case '"':
                  stringBuilder.Append('"');
                  continue;
                case '/':
                  stringBuilder.Append('/');
                  continue;
                case '\\':
                  stringBuilder.Append('\\');
                  continue;
                case 'b':
                  stringBuilder.Append('\b');
                  continue;
                case 'f':
                  stringBuilder.Append('\f');
                  continue;
                case 'n':
                  stringBuilder.Append('\n');
                  continue;
                case 'r':
                  stringBuilder.Append('\r');
                  continue;
                case 't':
                  stringBuilder.Append('\t');
                  continue;
                case 'u':
                  if (json.Length - index >= 4)
                  {
                    uint result;
                    if (!(success = UInt32Extensions.TryParse(new string(json, index, 4), NumberStyle.Hexadecimal, out result)))
                      return "";
                    stringBuilder.Append(CharExtensions.ConvertFromUtf32((int) result));
                    index += 4;
                    continue;
                  }
                  goto label_19;
                default:
                  continue;
              }
            }
            else
              goto label_19;
          default:
            stringBuilder.Append(ch2);
            continue;
        }
      }
label_19:
      if (flag)
        return stringBuilder.ToString();
      success = false;
      return (string) null;
    }

    /// <summary>
    /// Determines the type of number (int, double, etc) and returns an object
    /// containing that value.
    /// </summary>
    /// <param name="json"></param>
    /// <param name="index"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    protected static object ParseNumber(char[] json, ref int index, ref bool success)
    {
      JsonParser.EatWhitespace(json, ref index);
      int lastIndexOfNumber = JsonParser.GetLastIndexOfNumber(json, index);
      int length = lastIndexOfNumber - index + 1;
      string str = new string(json, index, length);
      string decimalSeparator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
      string numberGroupSeparator = CultureInfo.CurrentUICulture.NumberFormat.NumberGroupSeparator;
      string negativeSign = CultureInfo.CurrentUICulture.NumberFormat.NegativeSign;
      string positiveSign = CultureInfo.CurrentUICulture.NumberFormat.PositiveSign;
      object obj;
      if (StringExtensions.Contains(str, decimalSeparator) || StringExtensions.Contains(str, numberGroupSeparator) || StringExtensions.Contains(str, "e") || StringExtensions.Contains(str, "E"))
      {
        obj = (object) double.Parse(new string(json, index, length));
      }
      else
      {
        NumberStyle style = NumberStyle.Decimal;
        if (!StringExtensions.StartsWith(str, "0x"))
        {
          if (str.IndexOfAny(new char[12]
          {
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F'
          }) < 0)
            goto label_5;
        }
        style = NumberStyle.Hexadecimal;
label_5:
        obj = (object) Int64Extensions.Parse(str, style);
      }
      index = lastIndexOfNumber + 1;
      return obj;
    }

    protected static int GetLastIndexOfNumber(char[] json, int index)
    {
      int index1 = index;
      while (index1 < json.Length && "0123456789+-.eE".IndexOf(json[index1]) != -1)
        ++index1;
      return index1 - 1;
    }

    protected static void EatWhitespace(char[] json, ref int index)
    {
      while (index < json.Length && " \t\n\r".IndexOf(json[index]) != -1)
        ++index;
    }

    protected static JsonParser.Token LookAhead(char[] json, int index)
    {
      int index1 = index;
      return JsonParser.NextToken(json, ref index1);
    }

    protected static JsonParser.Token NextToken(char[] json, ref int index)
    {
      JsonParser.EatWhitespace(json, ref index);
      if (index == json.Length)
        return JsonParser.Token.None;
      char ch = json[index];
      ++index;
      switch (ch)
      {
        case '"':
          return JsonParser.Token.StringType;
        case ',':
          return JsonParser.Token.ItemsSeparator;
        case '-':
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
          return JsonParser.Token.NumberType;
        case ':':
          return JsonParser.Token.PropertySeparator;
        case '[':
          return JsonParser.Token.ArrayBegin;
        case ']':
          return JsonParser.Token.ArrayEnd;
        case '{':
          return JsonParser.Token.ObjectBegin;
        case '}':
          return JsonParser.Token.ObjectEnd;
        default:
          --index;
          int num = json.Length - index;
          if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
          {
            index += 5;
            return JsonParser.Token.BooleanFalse;
          }
          if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
          {
            index += 4;
            return JsonParser.Token.BooleanTrue;
          }
          if (num < 4 || json[index] != 'n' || json[index + 1] != 'u' || json[index + 2] != 'l' || json[index + 3] != 'l')
            return JsonParser.Token.None;
          index += 4;
          return JsonParser.Token.NullType;
      }
    }

    protected enum Token
    {
      None,
      ObjectBegin,
      ObjectEnd,
      ArrayBegin,
      ArrayEnd,
      PropertySeparator,
      ItemsSeparator,
      StringType,
      NumberType,
      BooleanTrue,
      BooleanFalse,
      NullType,
    }
  }
}
