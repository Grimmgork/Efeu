using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efeu.Router.Script
{
    internal class EfeuScriptVisitor : EfeuGrammarBaseVisitor<Func<EfeuScriptScope, EfeuValue>>
    {
        public override Func<EfeuScriptScope, EfeuValue> VisitScript([NotNull] EfeuGrammarParser.ScriptContext context)
        {
            return Visit(context.scope());
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitScope([NotNull] EfeuGrammarParser.ScopeContext context)
        {
            return scope =>
            {
                EfeuScriptScope newScope = new EfeuScriptScope(scope);
                foreach (var assigment in context.assignment())
                {
                    string name = assigment.CONST().GetText()[..^1];
                    newScope.Assign(name, () => Visit(assigment.expression())(newScope));
                }

                return Visit(context.expression())(newScope);
            };
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitThenExpr([NotNull] EfeuGrammarParser.ThenExprContext context)
        {
            var boolExpr = Visit(context.expression());
            var thenExpression = Visit(context.then().expression()[0]);
            var elseExpression = Visit(context.then().expression()[1]);
            return (scope) => boolExpr(scope) ? thenExpression(scope) : elseExpression(scope);
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitUnlessExpr([NotNull] EfeuGrammarParser.UnlessExprContext context)
        {
            var boolExpr = Visit(context.expression());
            var unlessExpression = Visit(context.unless().expression()[0]);
            var elseExpression = Visit(context.unless().expression()[1]);
            return (scope) => boolExpr(scope) ? elseExpression(scope) : unlessExpression(scope);
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitIntegerExpr([NotNull] EfeuGrammarParser.IntegerExprContext context)
        {
            return (_) => long.Parse(context.INT().GetText());
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitDecimalEpr([NotNull] EfeuGrammarParser.DecimalEprContext context)
        {
            return (_) => decimal.Parse(context.DECIMAL().GetText()[..^1]);
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitFloatExpr([NotNull] EfeuGrammarParser.FloatExprContext context)
        {
            return (_) => float.Parse(context.FLOAT().GetText());
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitNilExpr([NotNull] EfeuGrammarParser.NilExprContext context)
        {
            return (_) => default;
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitStringExpr([NotNull] EfeuGrammarParser.StringExprContext context)
        {
            return (_) => JsonSerializer.Deserialize<string>(context.STRING().GetText())!; // todo escape sequences
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitTrueExpr([NotNull] EfeuGrammarParser.TrueExprContext context)
        {
            return (_) => true;
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitFalseExpr([NotNull] EfeuGrammarParser.FalseExprContext context)
        {
            return (_) => false;
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitTraversal([NotNull] EfeuGrammarParser.TraversalContext context)
        {
            string name = context.GetText();
            return (scope) => scope.Get(name)();
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitBinaryExpr([NotNull] EfeuGrammarParser.BinaryExprContext context)
        {
            var a = Visit(context.expression()[0]);
            var b = Visit(context.expression()[1]);
            return context.@operator().GetText() switch
            {
                "+" => (context) => a(context) + b(context),
                "-" => (context) => a(context) - b(context),
                "=" => (context) => a(context) == b(context),
                "*" => (context) => a(context) * b(context),
                "/" => (context) => a(context) / b(context),
                "%" => (context) => a(context) % b(context)
            };
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitMethodExpr([NotNull] EfeuGrammarParser.MethodExprContext context)
        {
            return base.VisitMethodExpr(context);
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitGroupExpr([NotNull] EfeuGrammarParser.GroupExprContext context)
        {
            return Visit(context.expression());
        }

    }
}
