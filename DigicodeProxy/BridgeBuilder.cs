using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DigicodeProxy
{
    class BridgeBuilder : ISocketComponent
    {
        Socket s;
        List<Authorization> authorizations = new List<Authorization>();
        BridgeRouter router;
        double authorization_duration;

        public BridgeBuilder(BridgeRouter router, double authorization_duration, ushort port)
        {
            this.router = router;
            this.authorization_duration = authorization_duration;

            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(new IPEndPoint(IPAddress.Any, port));
            s.Listen(0);
        }

        public void add_authorization(Authorization a)
        {
            authorizations.Add(a);
        }

        public List<Socket> get_sockets_to_read()
        {
            List<Socket> l = new List<Socket>();
            l.Add(s);

            return l;
        }

        public bool use_socket(Socket s)
        {
            if (this.s == s)
            {
                Socket c = s.Accept();
                string address = ((IPEndPoint)c.RemoteEndPoint).Address.ToString();

                int i = authorizations.FindIndex(a => a.get_address() == address);
                if (i != -1)
                {
                    try
                    {
                        Socket l = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        l.Connect(new IPEndPoint(IPAddress.Loopback, authorizations[i].get_port()));

                        router.add_bridge(new Bridge(l, c));
                    }
                    catch (SocketException)
                    {

                    }
                    authorizations.RemoveAt(i);
                }

                return true;
            }

            return false;
        }

        public void remove_expired_authorizations()
        {
            authorizations.RemoveAll(a => Time.Get_Time() - a.get_creation_time() > authorization_duration);
        }
    }
}
