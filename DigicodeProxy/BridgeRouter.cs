using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DigicodeProxy
{
    class BridgeRouter : ISocketComponent
    {
        List<Bridge> bridges;

        public BridgeRouter()
        {
            bridges = new List<Bridge>();
        }

        public List<Socket> get_sockets_to_read()
        {
            List<Socket> l = new List<Socket>();

            foreach (Bridge b in bridges)
            {
                l.Add(b.get_local_end());
                l.Add(b.get_remote_end());
            }

            return l;
        }

        public void add_bridge(Bridge b)
        {
            bridges.Add(b);
        }

        public bool use_socket(Socket s)
        {
            for (int i = 0; i < bridges.Count; ++i)
                if (bridges[i].use_socket(s))
                {
                    if (bridges[i].is_closed())
                        bridges.RemoveAt(i);
                    return true;
                }

            return false;
        }
    }
}
