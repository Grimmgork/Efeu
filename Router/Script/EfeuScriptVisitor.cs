using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static System.Formats.Asn1.AsnWriter;

namespace Efeu.Router.Script
{
    internal class EfeuScriptVisitor : EfeuGrammarBaseVisitor<Func<EfeuScriptScope, EfeuValue>>
    {
        public readonly List<EfeuScriptExecutionException> Errors = new();

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
            return scope => scope.Get(name)();
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitBinaryExpr([NotNull] EfeuGrammarParser.BinaryExprContext context)
        {
            var a = Visit(context.expression()[0]);
            var b = Visit(context.expression()[1]);
            return context.@operator().GetText() switch
            {
                "+" => (scope) => a(scope) + b(scope),
                "-" => (scope) => a(scope) - b(scope),
                "=" => (scope) => a(scope) == b(scope),
                "*" => (scope) => a(scope) * b(scope),
                "/" => (scope) => a(scope) / b(scope),
                "%" => (scope) => a(scope) % b(scope),
                _ => throw new NotImplementedException()
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

        public override Func<EfeuScriptScope, EfeuValue> VisitArrayExpr([NotNull] EfeuGrammarParser.ArrayExprContext context)
        {
            var items = context.array_constructor().expression();
            return (scope) => new EfeuArray(items.Select(i => Visit(i)(scope)));
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitStructExpr([NotNull] EfeuGrammarParser.StructExprContext context)
        {
            var keys = context.struct_constructor().CONST().Select(i => i.GetText()[..^1]).ToArray();
            var values = context.struct_constructor().expression().ToArray();
            return (scope) =>
            {
                IDictionary<string, EfeuValue> fields = new Dictionary<string, EfeuValue>();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (!fields.TryAdd(keys[i], Visit(values[i])(scope)))
                    {
                        throw new Exception();
                    }
                }
                return new EfeuHash(fields);
            };
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitArrayModExpr([NotNull] EfeuGrammarParser.ArrayModExprContext context)
        {
            var data = Visit(context.expression());
            var items = context.with_array_mod().with_array_mod_item();

            return scope => VisitArrayModExprRec(scope, data(scope), items);
        }

        private EfeuValue VisitArrayModExprRec(EfeuScriptScope scope, EfeuValue value, EfeuGrammarParser.With_array_mod_itemContext[] items)
        {
            foreach (var item in items)
            {
                if (item.with_array_mod() != null)
                {
                    value = VisitArrayModExprRec(scope, value, item.with_array_mod().with_array_mod_item());
                }
                else if (item.with_struct_mod() != null)
                {
                    value = VisitStructModExprRec(scope, value, item.with_struct_mod().with_struct_mod_field());
                }
                else
                {
                    value = value.Call(
                        Visit(item.expression()[0])(scope).ToInt(),
                        Visit(item.expression()[1])(scope));
                }
            }
            return value;
        }

        public override Func<EfeuScriptScope, EfeuValue> VisitStructModExpr([NotNull] EfeuGrammarParser.StructModExprContext context)
        {
            var data = Visit(context.expression());
            var field = context.with_struct_mod().with_struct_mod_field();

            return scope => VisitStructModExprRec(scope, data(scope), field);
        }

        private EfeuValue VisitStructModExprRec(EfeuScriptScope scope, EfeuValue value, EfeuGrammarParser.With_struct_mod_fieldContext[] fields)
        {
            foreach (var field in fields)
            {
                if (field.with_array_mod() != null)
                {
                    value = VisitArrayModExprRec(scope, value, field.with_array_mod().with_array_mod_item());
                }
                else if (field.with_struct_mod() != null)
                {
                    value = VisitStructModExprRec(scope, value, field.with_struct_mod().with_struct_mod_field());
                }
                else
                {
                    value = value.Call(
                        field.CONST().GetText()[..^1],
                        Visit(field.expression())(scope));
                }
            }
            return value;
        }
    }
}
