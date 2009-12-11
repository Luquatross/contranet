using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DeviantArt.Chat.Library
{
    public static class Utils
    {
        public static string SHA1(string str)
        {
            return BitConverter.ToString(SHA1Managed.Create().ComputeHash(Encoding.Default.GetBytes(str))).Replace("-", "");
        }
    }
}
