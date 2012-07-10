using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework
{
    public static class ByteArrayExtensions
    {
        public static byte[] StripUTF8BOMs(this byte[] bytes)
        {
            while (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                bytes = bytes.Skip(3).ToArray();

            return bytes;
        }
    }
}
