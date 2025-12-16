using Antlr4.Runtime;
using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router.Script
{
    public class EfeuScript
    {
        public static EfeuValue Run(string script)
        {
            var inputStream = new AntlrInputStream(script);
            var lexer = new EfeuGrammarLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new EfeuGrammarParser(tokens);

            EfeuGrammarParser.ScriptContext tree = parser.script();

            EfeuScriptVisitor efeuScriptVisitor = new EfeuScriptVisitor();
            Func<EfeuScriptScope, EfeuValue> run = efeuScriptVisitor.Visit(tree);

            EfeuValue result = run(new EfeuScriptScope());
            return result;
        }
    }
}
