using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace DigicodeProxy
{
    class Program
    {
        public static void Main(string[] args)
        {
            ushort keypad_port = 7777;
            ushort bridge_port = 6666;
            uint token_length = 256;

            Socket keypad = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            keypad.Bind(new IPEndPoint(IPAddress.Any, keypad_port));

            Socket bridge_builder = null;

            List<BridgeSpecification> bridge_specifications = new List<BridgeSpecification>();
            bridge_specifications.Add(new BridgeSpecification(0, 25565, new List<string>(new string[] { "test" })));

            List<Authorization> authorizations = new List<Authorization>();
            List<Bridge> bridges = new List<Bridge>();

            bool on = true;
            while (on)
            {
                List<Socket> rl = new List<Socket>();

                rl.Add(keypad);
                if (bridge_builder != null)
                    rl.Add(bridge_builder);
                add_sockets_to_list(bridges, rl);

                Socket.Select(rl, null, null, 1000000);

                remove_expired_authorizations(authorizations);

                foreach (Socket s in rl)
                {
                    if (s == keypad)
                    {
                        Request r = get_request(s, token_length);
                        handle_request(r, bridge_specifications, authorizations, token_length);
                    }
                    else if (s == bridge_builder)
                    {
                        handle_connection_routing(s, authorizations, bridges);
                    }
                    else
                    {
                        handle_bridge(s, bridges);
                    }
                }

                if (authorizations.Count == 0 && bridge_builder != null)
                {
                    bridge_builder.Close();
                    bridge_builder = null;
                }
                else if (authorizations.Count > 0 && bridge_builder == null)
                {
                    bridge_builder = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    bridge_builder.Bind(new IPEndPoint(IPAddress.Any, bridge_port));
                    bridge_builder.Listen(0);
                }
            }
        }

        private static void handle_bridge(Socket s, List<Bridge> bridges)
        {
            // TODO: handle network failure
            for (int i = 0; i < bridges.Count; ++i)
            {
                Bridge b = bridges[i];
                if (b.get_local_end() == s || b.get_remote_end() == s)
                {
                    if (b.get_remote_end() == s)
                        b.transfer_to_local();
                    else
                        b.transfer_to_remote();

                    if (b.is_closed())
                        bridges.RemoveAt(i);
                    break;
                }
            }
        }

        private static void handle_connection_routing(Socket s, List<Authorization> authorizations, List<Bridge> bridges)
        {
            // TODO: handle network failure
            Socket c = s.Accept();

            string address = ((IPEndPoint)c.RemoteEndPoint).Address.ToString();

            int i = authorizations.FindIndex(a => a.get_address() == address);
            if (i != -1)
            {
                ushort port = authorizations[i].get_port();

                Socket l = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                l.Connect(new IPEndPoint(IPAddress.Loopback, port));

                bridges.Add(new Bridge(l, c));

                authorizations.RemoveAt(i);
            }
        }

        private static void add_sockets_to_list(List<Bridge> bridges, List<Socket> l)
        {
            foreach (Bridge b in bridges)
            {
                l.Add(b.get_local_end());
                l.Add(b.get_remote_end());
            }
        }

        private static Request get_request(Socket s, uint token_length)
        {
            byte[] b = new byte[2048];
            EndPoint e = new IPEndPoint(IPAddress.Any, 0);

            int size = s.ReceiveFrom(b, ref e);
            IPEndPoint rep = (IPEndPoint)e;
            // TODO: handle exception : buffer too small

            if (size == sizeof(uint) + token_length)
            {
                uint id = BitConverter.ToUInt32(b, 0);
                byte[] token_data = new byte[token_length];
                for (ulong i = 0; i < token_length; ++i)
                    token_data[i] = b[i + sizeof(uint)];

                string address = rep.Address.ToString();

                return new Request(id, new Token(token_data), address);
            }

            throw new Exception("invalid request format");
        }

        private static void handle_request(Request r, List<BridgeSpecification> bridge_specifications, List<Authorization> authorizations, uint token_length)
        {
            BridgeSpecification s = bridge_specifications.Find(x => x.get_id() == r.get_id());

            if (s == null)
                return;

            ulong salt = get_salt_from_current_time();

            if (s.get_passwords().Find(p => new Token(p, salt, token_length).Equals(r.get_token())) != null)
                authorizations.Add(new Authorization(r.get_address(), s.get_port()));
        }

        private static ulong get_salt_from_current_time()
        {
            return (ulong)DateTime.UtcNow.Subtract(DateTime.Parse("0001-01-01T00:00:00Z")).TotalMinutes;
        }

        private static void remove_expired_authorizations(List<Authorization> authorizations)
        {
            double t = DateTime.UtcNow.Subtract(DateTime.Parse("0001-01-01T00:00:00Z")).TotalMilliseconds;

            authorizations.RemoveAll(a => t - a.get_creation_date() > 60000);
        }
    }
}
