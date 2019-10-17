using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RHCCore.Security
{
    public static class Hashing
    {
        public static string EncryptSHA256(string text)
        {
            SHA256 cipher = SHA256.Create();
            byte[] buffer = cipher.ComputeHash(Encoding.UTF8.GetBytes(text));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in buffer)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}
