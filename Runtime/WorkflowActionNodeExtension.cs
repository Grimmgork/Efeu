using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Efeu.Runtime.Data;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public static class WorkflowActionNodeExtension
    {
        public static WorkflowActionNode Lower(this WorkflowActionNode node)
        {
            var inputStream = new AntlrInputStream(node.Script);
            var lexer = new EfeuGrammarLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new EfeuGrammarParser(tokens);

            var tree = parser.line(); // start rule

            EfeuBlockListener listener = new EfeuBlockListener();
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(listener, tree);

            // listener.Input(context);

            Console.WriteLine(tree.ToStringTree(parser));
            // Console.WriteLine(listener.Type + $": {listener.Input(new InputEvaluationContext((_) => default, (_) => default, default))}");

            WorkflowActionNode result = new WorkflowActionNode();
            result.Type = WorkflowActionNodeType.Call;
            return result;
        }
    }
}
