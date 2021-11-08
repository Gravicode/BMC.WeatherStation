// Decompiled with JetBrains decompiler
// Type: Json.NETMF.StringExtensions
// Assembly: Json.NetMF, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 734D45F7-78D8-43F9-94B6-41A5EBA2ADBB
// Assembly location: D:\experiment\BMC.DataLogger\packages\Json.NetMF.1.3.0.0\lib\netmf43\Json.NetMF.dll

namespace Json.TinyCLR
{
  internal static class StringExtensions
  {
    public static bool EndsWith(this string s, string value) => s.IndexOf(value) == s.Length - value.Length;

    public static bool StartsWith(this string s, string value) => s.IndexOf(value) == 0;

    public static bool Contains(this string s, string value) => s.IndexOf(value) >= 0;
  }
}
