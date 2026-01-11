using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Script
{
    public record ParseError(
            int Line,
            int Column,
            string Message
        );

    public class EfeuScriptParseException : Exception
    {
        public readonly ParseError[] Errors;

        public EfeuScriptParseException(IEnumerable<ParseError> errors) : base($"Error while parsing script: \n { 
            string.Join("\n", errors.Select(i => $"[{i.Line}:{i.Column}] {i.Message}")) }")
        {
            Errors = errors.ToArray();
        }
    }
}
