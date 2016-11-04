using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigicodeProxy
{
    class Authorization
    {
        string address;
        ushort port;
        double creation_date;

        public Authorization(string address, ushort port)
        {
            this.address = address;
            this.port = port;
            creation_date = DateTime.UtcNow.Subtract(DateTime.Parse("0001-01-01T00:00:00Z")).TotalMilliseconds;
        }

        public double get_creation_time()
        {
            return creation_date;
        }

        public string get_address()
        {
            return address;
        }

        public ushort get_port()
        {
            return port;
        }
    }
}
