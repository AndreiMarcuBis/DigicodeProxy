using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigicodeProxy
{
    class Request
    {
        uint id;
        Token token;
        string address;

        public Request(uint id, Token token, string address)
        {
            this.id = id;
            this.token = token;
            this.address = address;
        }

        public uint get_id()
        {
            return id;
        }

        public Token get_token()
        {
            return token;
        }

        public string get_address()
        {
            return address;
        }
    }
}
