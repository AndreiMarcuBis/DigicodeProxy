using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigicodeProxy
{
    class Time
    {
        public static double Get_Time()
        {
            return DateTime.UtcNow.Subtract(DateTime.Parse("0001-01-01T00:00:00Z")).TotalMilliseconds;
        }
    }
}
