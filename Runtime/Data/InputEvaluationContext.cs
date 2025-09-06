using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Efeu.Runtime.Function;
using Efeu.Runtime.Model;

namespace Efeu.Runtime.Data
{
    public class InputEvaluationContext
    {
        public readonly Func<int, SomeData> GetOutput;

        public readonly SomeData LastOutput;

        public readonly Func<string, SomeData> GetVariable;

        public InputEvaluationContext(Func<int, SomeData> getOutput, Func<string, SomeData> getVariable, SomeData lastOutput)
        {
            GetOutput = getOutput;
            GetVariable = getVariable;
            LastOutput = lastOutput;
        }
    }
}
