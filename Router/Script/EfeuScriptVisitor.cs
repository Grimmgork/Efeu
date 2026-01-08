using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Efeu.Router.Value;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace Efeu.Router.Script
{
    internal class EfeuScriptVisitor : EfeuGrammarBaseVisitor<EfeuValue>
    {
        private EfeuScriptScope scope;

        public EfeuScriptVisitor(EfeuScriptScope scope)
        {
            this.scope = scope;
        }

        public override EfeuValue VisitScript([NotNull] EfeuGrammarParser.ScriptContext context)
        {
            return Visit(context.scope());
        }

        public override EfeuValue VisitScope([NotNull] EfeuGrammarParser.ScopeContext context)
        {
            EfeuScriptScope currentScope = scope;
            foreach (var assignment in context.assignment())
            {
                string name = assignment.CONST().GetText()[..^1];
                EfeuValue value = Visit(assignment.expression());
                scope = scope.Push(name, value);
            }
            EfeuValue result = Visit(context.expression());
            scope = currentScope;
            return result;
        }

        public override EfeuValue VisitThenExpr([NotNull] EfeuGrammarParser.ThenExprContext context)
        {
            return Visit(context.expression()) ? 
                Visit(context.then().expression()[0]) : Visit(context.then().expression()[1]);
        }

        public override EfeuValue VisitUnlessExpr([NotNull] EfeuGrammarParser.UnlessExprContext context)
        {
            return Visit(context.expression()) ?
                Visit(context.unless().expression()[1]) : Visit(context.unless().expression()[0]);
        }

        public override EfeuValue VisitIntegerExpr([NotNull] EfeuGrammarParser.IntegerExprContext context)
        {
            return long.Parse(context.INT().GetText());
        }

        public override EfeuValue VisitDecimalEpr([NotNull] EfeuGrammarParser.DecimalEprContext context)
        {
            return decimal.Parse(context.DECIMAL().GetText()[..^1]);
        }

        public override EfeuValue VisitFloatExpr([NotNull] EfeuGrammarParser.FloatExprContext context)
        {
            return float.Parse(context.FLOAT().GetText());
        }

        public override EfeuValue VisitNilExpr([NotNull] EfeuGrammarParser.NilExprContext context)
        {
            return default;
        }

        public override EfeuValue VisitStringExpr([NotNull] EfeuGrammarParser.StringExprContext context)
        {
            return JsonSerializer.Deserialize<string>(context.STRING().GetText())!; // todo escape sequences
        }

        public override EfeuValue VisitTrueExpr([NotNull] EfeuGrammarParser.TrueExprContext context)
        {
            return true;
        }

        public override EfeuValue VisitFalseExpr([NotNull] EfeuGrammarParser.FalseExprContext context)
        {
            return false;
        }

        public override EfeuValue VisitTraversal([NotNull] EfeuGrammarParser.TraversalContext context)
        {
            string name = context.GetText();
            return scope.Get(name);
        }

        public override EfeuValue VisitBinaryExpr([NotNull] EfeuGrammarParser.BinaryExprContext context)
        {
            var a = Visit(context.expression()[0]);
            var b = Visit(context.expression()[1]);
            return context.@operator().GetText() switch
            {
                "+" => a + b,
                "-" => a - b,
                "=" => a == b,
                "*" => a * b,
                "/" => a / b,
                "%" => a % b,
                _ => throw new NotImplementedException()
            };
        }

        public override EfeuValue VisitGroupExpr([NotNull] EfeuGrammarParser.GroupExprContext context)
        {
            return Visit(context.expression());
        }

        public override EfeuValue VisitArrayExpr([NotNull] EfeuGrammarParser.ArrayExprContext context)
        {
            var items = context.array_constructor().expression();
            return new EfeuArray(items.Select(i => Visit(i)));
        }

        public override EfeuValue VisitStructExpr([NotNull] EfeuGrammarParser.StructExprContext context)
        {
            var keys = context.struct_constructor().CONST().Select(i => i.GetText()[..^1]).ToArray();
            var values = context.struct_constructor().expression().ToArray();
            IDictionary<string, EfeuValue> fields = new Dictionary<string, EfeuValue>();
            for (int i = 0; i < keys.Length; i++)
            {
                if (!fields.TryAdd(keys[i], Visit(values[i])))
                {
                    throw new Exception();
                }
            }
            return new EfeuHash(fields);
        }

        public override EfeuValue VisitArrayModExpr([NotNull] EfeuGrammarParser.ArrayModExprContext context)
        {
            var data = Visit(context.expression());
            var items = context.with_array_mod().with_array_mod_item();

            return VisitArrayModExprRec(scope, data, items);
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
                        Visit(item.expression()[0]).ToInt(),
                        Visit(item.expression()[1]));
                }
            }
            return value;
        }

        public override EfeuValue VisitStructModExpr([NotNull] EfeuGrammarParser.StructModExprContext context)
        {
            var data = Visit(context.expression());
            var field = context.with_struct_mod().with_struct_mod_field();

            return VisitStructModExprRec(scope, data, field);
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
                        Visit(field.expression()));
                }
            }
            return value;
        }
    }
}
