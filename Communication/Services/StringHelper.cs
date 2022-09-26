using System;
using System.Text;

namespace Communication.Core
{
    public static class StringHelper
    {
        public static string GetHexString(Memory<byte> rawBytes)
        {
            StringBuilder stringBuilder = new();
            for (int i = 0; i < rawBytes.Length; i++)
            {
                stringBuilder.Append(rawBytes.Span[i].ToString("X2")).Append(' ');
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }
    }
}