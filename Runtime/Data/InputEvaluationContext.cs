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
        public readonly Func<int, EfeuValue> GetOutput;

        public readonly EfeuValue LastOutput;

        public readonly Func<string, EfeuValue> GetVariable;

        public InputEvaluationContext(Func<int, EfeuValue> getOutput, Func<string, EfeuValue> getVariable, EfeuValue lastOutput)
        {
            GetOutput = getOutput;
            GetVariable = getVariable;
            LastOutput = lastOutput;
        }
    }
}
