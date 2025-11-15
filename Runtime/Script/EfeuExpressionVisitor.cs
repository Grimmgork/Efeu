using Antlr4.Runtime.Misc;
using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Script
{
    internal class EfeuExpressionVisitor : EfeuGrammarBaseVisitor<Func<InputEvaluationContext, EfeuValue>>
    {
        public override Func<InputEvaluationContext, EfeuValue> VisitIntExpr([NotNull] EfeuGrammarParser.IntExprContext context)
        {
            return (input) => int.Parse(context.GetText());
        }

        public override Func<InputEvaluationContext, EfeuValue> VisitLine([NotNull] EfeuGrammarParser.LineContext context)
        {
            return Visit(context.stat());
        }
    }
}
