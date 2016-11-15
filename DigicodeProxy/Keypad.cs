using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace DigicodeProxy
{
    class Keypad : ISocketComponent
    {
        Socket s;
        BridgeBuilder builder;
        List<BridgeSpecification> specifications;
        uint token_length;
        double token_duration;

        public Keypad(ushort port, BridgeBuilder builder, List<BridgeSpecification> specifications, uint token_length, double token_duration)
        {
            this.builder = builder;
            this.specifications = specifications;
            this.token_length = token_length;
            this.token_duration = token_duration;

            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch (SocketException)
            {
                throw new PredictedException("can't create or bind keypad socket");
            }
            catch (System.Security.SecurityException)
            {
                throw new PredictedException("can't bind keypad socket on specific port for security reason");
            }
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
                Request r = get_request(s);
                if (r != null)
                {
                    BridgeSpecification bs = specifications.Find(x => x.get_id() == r.get_id());

                    if (bs != null)
                    {
                        ulong salt = get_salt_from_current_time();

                        if (bs.get_passwords().Find(p => new Token(p, salt, token_length).Equals(r.get_token())) != null)
                            builder.add_authorization(new Authorization(r.get_address(), bs.get_port()));
                    }
                }

                return true;
            }

            return false;
        }

        private Request get_request(Socket s)
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

            return null;
        }


        private ulong get_salt_from_current_time()
        {
            return (ulong)(Time.Get_Time() / token_duration);
        }

    }
}
