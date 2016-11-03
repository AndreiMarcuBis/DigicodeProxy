using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DigicodeProxy
{
    class Token
    {
        byte[] data;

        public Token(byte[] data)
        {
            this.data = data.ToArray();
        }

        public Token(string password, ulong salt, uint token_length)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));

            uint useed = BitConverter.ToUInt32(hash, 0);
            useed += (uint)(salt % uint.MaxValue);

            int seed = (int)((long)useed - int.MaxValue);

            Random r = new Random(seed);

            data = new byte[token_length];
            r.NextBytes(data);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Token t = obj as Token;
            if (t == null)
                return false;

            return data.SequenceEqual(t.data);
        }

        public bool Equals(Token t)
        {
            if (t == null)
                return false;

            return data.SequenceEqual(t.data);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (byte b in data)
                hash += b;

            return hash;
        }
    }
}
