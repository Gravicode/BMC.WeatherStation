// Decompiled with JetBrains decompiler
// Type: Json.NETMF.CharExtensions
// Assembly: Json.NetMF, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 734D45F7-78D8-43F9-94B6-41A5EBA2ADBB
// Assembly location: D:\experiment\BMC.DataLogger\packages\Json.NetMF.1.3.0.0\lib\netmf43\Json.NetMF.dll

namespace Json.TinyCLR
{
  internal static class CharExtensions
  {
    /// <summary>
    /// Converts a Unicode character to a string of its ASCII equivalent.
    /// Very simple, it works only on ordinary characters.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static string ConvertFromUtf32(int p) => ((char) p).ToString();
  }
}
