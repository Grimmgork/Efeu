using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Signal
{
    public class CustomWorkflowSignal
    {
        public string Name = "";

        public DateTime Timestamp;

        public SomeData Payload;
    }
}
