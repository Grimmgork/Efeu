using Antlr4.Runtime;
using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router.Script
{
    public class EfeuScript
    {
        public static EfeuValue Run(string script, EfeuScriptScope scope)
        {
            var inputStream = new AntlrInputStream(script);
            var lexer = new EfeuGrammarLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new EfeuGrammarParser(tokens);

            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();

            CollectingErrorListener errorListener = new CollectingErrorListener();
            parser.AddErrorListener(errorListener);

            EfeuGrammarParser.ScriptContext tree = parser.script();

            Console.WriteLine(tree.ToStringTree(parser));

            if (parser.NumberOfSyntaxErrors > 0 )
            {
                throw new EfeuScriptParseException(errorListener.Errors);
            }

            EfeuScriptVisitor efeuScriptVisitor = new EfeuScriptVisitor(scope);
            EfeuValue result = efeuScriptVisitor.Visit(tree);
            return result;
        }

        private class CollectingErrorListener : BaseErrorListener
        {
            public readonly List<ParseError> Errors = new();

            public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                Errors.Add(new ParseError(line, charPositionInLine, msg));
            }
        }
    }
}
