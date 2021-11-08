// Decompiled with JetBrains decompiler
// Type: Json.NETMF.JsonSerializer
// Assembly: Json.NetMF, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 734D45F7-78D8-43F9-94B6-41A5EBA2ADBB
// Assembly location: D:\experiment\BMC.DataLogger\packages\Json.NetMF.1.3.0.0\lib\netmf43\Json.NetMF.dll

using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Json.TinyCLR
{
  /// <summary>
  /// JSON.NetMF - JSON Serialization and Deserialization library for .NET Micro Framework
  /// </summary>
  public class JsonSerializer
  {
    private DateTimeFormat datefield;

    public JsonSerializer(DateTimeFormat dateTimeFormat = DateTimeFormat.Default) => this.DateFormat = dateTimeFormat;

    /// <summary>
    /// Gets/Sets the format that will be used to display
    /// and parse dates in the Json data.
    /// </summary>
    public DateTimeFormat DateFormat
    {
      get => this.datefield;
      set => this.datefield = value;
    }

    /// <summary>Convert an object to a JSON string.</summary>
    /// <param name="o">The value to convert. Supported types are: Boolean, String, Byte, (U)Int16, (U)Int32, Float, Double, Decimal, Array, IDictionary, IEnumerable, Guid, Datetime, DictionaryEntry, Object and null.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    /// <remarks>For objects, only public properties with getters are converted.</remarks>
    public string Serialize(object o) => JsonSerializer.SerializeObject(o, this.DateFormat);

    /// <summary>Desrializes a Json string into an object.</summary>
    /// <param name="json"></param>
    /// <returns>An ArrayList, a Hashtable, a double, a long, a string, null, true, or false</returns>
    public object Deserialize(string json) => JsonSerializer.DeserializeString(json);

    /// <summary>Deserializes a Json string into an object.</summary>
    /// <param name="json"></param>
    /// <returns>An ArrayList, a Hashtable, a double, a long, a string, null, true, or false</returns>
    public static object DeserializeString(string json) => JsonParser.JsonDecode(json);

    /// <summary>Convert an object to a JSON string.</summary>
    /// <param name="o">The value to convert. Supported types are: Boolean, String, Byte, (U)Int16, (U)Int32, Float, Double, Decimal, Array, IDictionary, IEnumerable, Guid, Datetime, DictionaryEntry, Object and null.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    /// <remarks>For objects, only public properties with getters are converted.</remarks>
    public static string SerializeObject(object o, DateTimeFormat dateTimeFormat = DateTimeFormat.Default)
    {
      if (o == null)
        return "null";
      Type type = o.GetType();
      switch (type.Name)
      {
        case "Boolean":
          return !(bool) o ? "false" : "true";
        case "String":
        case "Char":
        case "Guid":
          return "\"" + JsonSerializer.SerializeString(o as string) + "\"";
        case "Single":
        case "Double":
        case "Decimal":
        case "Float":
        case "Byte":
        case "SByte":
        case "Int16":
        case "UInt16":
        case "Int32":
        case "UInt32":
        case "Int64":
        case "UInt64":
          return o.ToString();
        case "DateTime":
          switch (dateTimeFormat)
          {
            case DateTimeFormat.Ajax:
              return "\"" + DateTimeExtensions.ToASPNetAjax((DateTime) o) + "\"";
            default:
              return "\"" + DateTimeExtensions.ToIso8601((DateTime) o) + "\"";
          }
        default:
          if (o is IDictionary && !type.IsArray)
            return JsonSerializer.SerializeIDictionary(o as IDictionary, dateTimeFormat);
          if (o is IEnumerable)
            return JsonSerializer.SerializeIEnumerable(o as IEnumerable, dateTimeFormat);
          if ((object) type == (object) typeof (DictionaryEntry))
          {
            DictionaryEntry dictionaryEntry = o as DictionaryEntry;
            return JsonSerializer.SerializeIDictionary((IDictionary) new Hashtable()
            {
              {
                dictionaryEntry.Key,
                dictionaryEntry.Value
              }
            }, dateTimeFormat);
          }
          if (!type.IsClass)
            return (string) null;
          Hashtable hashtable = new Hashtable();
          foreach (MethodInfo method in type.GetMethods())
          {
            if (StringExtensions.StartsWith(method.Name, "get_") && !method.IsAbstract && (object) method.ReturnType != (object) typeof (Delegate) && (object) method.ReturnType != (object) typeof (MulticastDelegate) && (object) method.ReturnType != (object) typeof (MethodInfo) && (object) method.DeclaringType != (object) typeof (Delegate) && (object) method.DeclaringType != (object) typeof (MulticastDelegate))
            {
              object obj = method.Invoke(o, (object[]) null);
              hashtable.Add((object) method.Name.Substring(4), obj);
            }
          }
          return JsonSerializer.SerializeIDictionary((IDictionary) hashtable, dateTimeFormat);
      }
    }

    /// <summary>Convert an IEnumerable to a JSON string.</summary>
    /// <param name="enumerable">The value to convert.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    protected static string SerializeIEnumerable(
      IEnumerable enumerable,
      DateTimeFormat dateTimeFormat = DateTimeFormat.Default)
    {
      StringBuilder stringBuilder = new StringBuilder("[");
      foreach (object o in enumerable)
      {
        if (stringBuilder.Length > 1)
          stringBuilder.Append(",");
        stringBuilder.Append(JsonSerializer.SerializeObject(o, dateTimeFormat));
      }
      stringBuilder.Append("]");
      return stringBuilder.ToString();
    }

    /// <summary>Convert an IDictionary to a JSON string.</summary>
    /// <param name="dictionary">The value to convert.</param>
    /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
    protected static string SerializeIDictionary(
      IDictionary dictionary,
      DateTimeFormat dateTimeFormat = DateTimeFormat.Default)
    {
      StringBuilder stringBuilder = new StringBuilder("{");
      foreach (DictionaryEntry dictionaryEntry in (IEnumerable) dictionary)
      {
        if (stringBuilder.Length > 1)
          stringBuilder.Append(",");
        stringBuilder.Append("\"" + dictionaryEntry.Key + "\"");
        stringBuilder.Append(":");
        stringBuilder.Append(JsonSerializer.SerializeObject(dictionaryEntry.Value, dateTimeFormat));
      }
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    /// <summary>
    /// Safely serialize a String into a JSON string value, escaping all backslash and quote characters.
    /// </summary>
    /// <param name="str">The string to serialize.</param>
    /// <returns>The serialized JSON string.</returns>
    protected static string SerializeString(string str)
    {
      if (str.IndexOf('\\') < 0 && str.IndexOf('"') < 0)
        return str;
      StringBuilder stringBuilder = new StringBuilder(str.Length + 1);
      foreach (char ch in str.ToCharArray())
      {
        switch (ch)
        {
          case '"':
          case '\\':
            stringBuilder.Append('\\');
            break;
        }
        stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }
  }
}
