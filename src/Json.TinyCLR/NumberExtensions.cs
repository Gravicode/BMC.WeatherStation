// Decompiled with JetBrains decompiler
// Type: Json.NETMF.UInt32Extensions
// Assembly: Json.NetMF, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 734D45F7-78D8-43F9-94B6-41A5EBA2ADBB
// Assembly location: D:\experiment\BMC.DataLogger\packages\Json.NetMF.1.3.0.0\lib\netmf43\Json.NetMF.dll

namespace Json.TinyCLR
{
  internal static class UInt32Extensions
  {
    public static bool TryParse(string str, NumberStyle style, out uint result)
    {
      ulong result1;
      bool sign;
      bool uint64Core = Helper.TryParseUInt64Core(str, style == NumberStyle.Hexadecimal, out result1, out sign);
      result = (uint) result1;
      return uint64Core && !sign;
    }
  }
}
