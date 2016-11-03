using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigicodeProxy
{
    class BridgeSpecification
    {
        uint id;
        ushort port;
        List<string> passwords;

        public BridgeSpecification(uint id, ushort port, List<string> passwords)
        {
            this.id = id;
            this.port = port;
            this.passwords = new List<string>(passwords);
        }

        public ushort get_port()
        {
            return port;
        }

        public uint get_id()
        {
            return id;
        }

        public List<string> get_passwords()
        {
            return new List<string>(passwords);
        }
    }
}
