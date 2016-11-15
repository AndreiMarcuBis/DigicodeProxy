using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigicodeProxy
{
    class PredictedException : Exception
    {
        public PredictedException(string m) : base(m)
        {

        }
    }
}
