using Antlr4.Runtime.Misc;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Script
{
    public enum EfeuBlockType
    {
        Eval,
        If,
        Unless,
        For,
        Map,
        Set,
        Call
    }

    internal class EfeuBlockListener : EfeuGrammarBaseListener
    {
        public EfeuBlockType Type;

        public string Name = "";

        public Func<InputEvaluationContext, EfeuValue> Input = (_) => default;

        public DataTraversal Variable;

        public string Iterator = "";

        public override void EnterExprStat([NotNull] EfeuGrammarParser.ExprStatContext context)
        {
            Type = EfeuBlockType.Eval;
            var visitor = new EfeuExpressionVisitor();
            Func<InputEvaluationContext, EfeuValue> expression = visitor.Visit(context.expr());
            Input = expression;
        }

        public override void EnterAssignmentStat([NotNull] EfeuGrammarParser.AssignmentStatContext context)
        {
            Type = EfeuBlockType.Set;
            Variable = context.traversal().GetText();
            var visitor = new EfeuExpressionVisitor();
            Func<InputEvaluationContext, EfeuValue> expression = visitor.Visit(context.expr());
            Input = expression;
        }

        public override void EnterForStat([NotNull] EfeuGrammarParser.ForStatContext context)
        {
            Type = EfeuBlockType.For;
            Iterator = context.variable().ID().GetText();
            var visitor = new EfeuExpressionVisitor();
            Input = visitor.Visit(context.expr());
        }
    }
}
