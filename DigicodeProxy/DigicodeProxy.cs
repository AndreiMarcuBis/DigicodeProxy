using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace DigicodeProxy
{
    class DigicodeProxy
    {
        Thread t;
        volatile bool run;
        object tlock;

        ushort keypad_port;
        ushort bridge_port;
        uint token_length;
        double authorization_duration;
        double token_duration;
        int select_timeout;
        List<BridgeSpecification> specifications;

        public DigicodeProxy()
        {
            tlock = new object();

            keypad_port = 7777;
            bridge_port = 6666;
            token_length = 256;
            authorization_duration = 60000;
            token_duration = 60000;
            select_timeout = 1000000;
            specifications = new List<BridgeSpecification>();
            specifications.Add(new BridgeSpecification(0, 25565, new List<string>(new string[] { "minecraft" })));
        }

        public void start()
        {
            lock (tlock)
            {
                if (t == null)
                {
                    t = new Thread(new ThreadStart(main));
                    run = true;
                    t.Start();
                }
            }
        }

        public void stop()
        {
            lock (tlock)
            {
                run = false;
                t.Join();
                t = null;
            }
        }

        private void main()
        {
            BridgeRouter router = new BridgeRouter();
            BridgeBuilder builder = new BridgeBuilder(router, authorization_duration, bridge_port);
            Keypad keypad = new Keypad(keypad_port, builder, specifications, token_length, token_duration);

            List<ISocketComponent> socket_components = new List<ISocketComponent>();
            socket_components.Add(router);
            socket_components.Add(builder);
            socket_components.Add(keypad);

            while (run)
            {
                List<Socket> l = new List<Socket>();
                socket_components.ForEach(sc => l.AddRange(sc.get_sockets_to_read()));

                Socket.Select(l, null, null, select_timeout);

                builder.remove_expired_authorizations();

                foreach (Socket s in l)
                    foreach (ISocketComponent sc in socket_components)
                        if (sc.use_socket(s))
                            break;
            }
        }
    }
}
