using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace DigicodeProxy
{
    interface ISocketComponent
    {
        List<Socket> get_sockets_to_read();
        bool use_socket(Socket s);
    }
}
