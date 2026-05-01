using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using ArcCreate.Utility.Parser;
using Jace;
using Jace.Execution;
using Random = UnityEngine.Random;

namespace ArcCreate.ChartFormat.Grammar
{
    public class UniversalChartVisitor : UniversalAffChartBaseVisitor<object>
    {
        public UniversalChartVisitor()
        {
            ExprStringEngine.AddFunction("randint",
                (min, max) => (int)Random.Range((float)min, (float)max));
            ExprStringEngine.AddFunction("rand",
                (min, max) => Random.Range((float)min, (float)max));
        }

        public static string TrimQuotes(string raw) =>
            raw.Length >= 2 && (raw[0] == '\'' || raw[0] == '"') && raw[0] == raw[^1]
                ? raw[1..^1]
                : raw;

        public AntlrEventSegment VisitChartTyped(UniversalAffChartParser.ChartContext context) =>
            (AntlrEventSegment)VisitChart(context);

        /// <returns><see cref="AntlrEventSegment"/></returns>
        public override object VisitChart(UniversalAffChartParser.ChartContext context)
        {
            return VisitBodyTyped(context.body());
        }

        private static readonly CalculationEngine ExprStringEngine =
            new(CultureInfo.CurrentCulture, ExecutionMode.Interpreted);

        private readonly Dictionary<string, double> exprStringCalculationVariables = new();

        public AntlrValue VisitValueTyped(UniversalAffChartParser.ValueContext context) =>
            (AntlrValue)VisitValue(context);

        /// <returns><see cref="AntlrValue"/></returns>
        public override object VisitValue(UniversalAffChartParser.ValueContext context)
        {
            if (context.ExprString() != null)
            {
                var raw = context.ExprString().GetText();
                var content = raw.Trim('`');

                if (!Evaluator.TryFloat(content, out float eval, ExprStringEngine))
                {
                    throw new AntlrParseException($"Unable to evaluate expression: {context.GetText()}",
                        context.GetText(), RawEventType.AntlrValue, context.Start);
                }

                return AntlrValue.FromAlgebraic(raw, eval, context.Start.Line, context.Start.Column);
            }

            if (context.Word() != null && context.Equal() != null && context.value() != null)
            {
                var key = context.Word().GetText();
                var value = VisitValueTyped(context.value());

                if (value.Type == AntlrValueType.KeyValuePair)
                {
                    throw new AntlrParseException("Nested key-value pair is not allowed", context.GetText(),
                        RawEventType.AntlrValue, context.Start);
                }

                return AntlrValue.FromKeyValuePair($"{key}={value.Raw}", new Tuple<string, AntlrValue>(key, value),
                    context.Start.Line, context.Start.Column);
            }

            if (context.String() != null)
            {
                var raw = context.String().GetText();
                var content = TrimQuotes(raw);
                return AntlrValue.FromString(raw, content, context.Start.Line, context.Start.Column);
            }

            if (context.Word() != null)
            {
                var raw = context.Word().GetText();
                return AntlrValue.FromString(raw, raw, context.Start.Line, context.Start.Column);
            }

            if (context.expr() != null)
            {
                return VisitExprTyped(context.expr());
            }

            if (context.GetText() == string.Empty)
            {
                return AntlrValue.GetEmpty(context.Start);
            }


            throw new AntlrParseException("Unknown value type", context.GetText(), RawEventType.AntlrValue,
                context.Start);
        }

        public AntlrValue VisitExprTyped(UniversalAffChartParser.ExprContext context) => (AntlrValue)VisitExpr(context);

        /// <returns><see cref="AntlrValue"/></returns>
        public override object VisitExpr(UniversalAffChartParser.ExprContext context)
        {
            // maybe try refactoring with Evaluator class
            // evaluate: context.GetText()

            var parentStart = (context.Parent as ParserRuleContext)!.Start;

            if (context.LParen() != null && context.expr().Length == 1 && context.RParen() != null)
            {
                return VisitExprTyped(context.expr(0));
            }

            if (context.Minus() != null && context.expr().Length == 1)
            {
                var child = VisitExprTyped(context.expr(0));

                return AntlrValue.FromAlgebraic($"-{child.Raw}", -child.GetAlgebraicValue(),
                    context.Start.Line,
                    context.Start.Column + parentStart.Column);
            }

            if (context.expr().Length == 2)
            {
                var left = VisitExprTyped(context.expr(0));
                var right = VisitExprTyped(context.expr(1));

                var (result, op) = ParseExpr(left, right);

                return AntlrValue.FromAlgebraic($"{left.Raw}{op}{right.Raw}", result,
                    context.Start.Line,
                    context.Start.Column + parentStart.Column);
            }

            if (context.Int() != null)
            {
                var raw = context.Int().GetText();
                var num = Convert.ToInt32(raw);

                return AntlrValue.FromInteger(raw, num,
                    context.Start.Line,
                    context.Start.Column + parentStart.Column);
            }

            if (context.Float() != null)
            {
                var raw = context.Float().GetText();
                var num = Convert.ToDouble(raw);

                return AntlrValue.FromAlgebraic(raw, num,
                    context.Start.Line,
                    context.Start.Column + parentStart.Column);
            }

            throw new AntlrParseException("Unable to evaluate basic expr", context.GetText(), RawEventType.AntlrExpr,
                context.Start, parentStart);

            Tuple<double, string> ParseExpr(AntlrValue left, AntlrValue right)
            {
                // there is no operator priority here

                if (context.Multiply() != null)
                {
                    return new Tuple<double, string>(left.GetAlgebraicValue() * right.GetAlgebraicValue(),
                        context.Multiply().GetText());
                }

                if (context.Divide() != null)
                {
                    return new Tuple<double, string>(left.GetAlgebraicValue() / right.GetAlgebraicValue(),
                        context.Divide().GetText());
                }

                if (context.Mod() != null)
                {
                    return new Tuple<double, string>(left.GetAlgebraicValue() % right.GetAlgebraicValue(),
                        context.Mod().GetText());
                }

                if (context.Plus() != null)
                {
                    return new Tuple<double, string>(left.GetAlgebraicValue() + right.GetAlgebraicValue(),
                        context.Plus().GetText());
                }

                if (context.Minus() != null)
                {
                    return new Tuple<double, string>(left.GetAlgebraicValue() - right.GetAlgebraicValue(),
                        context.Minus().GetText());
                }

                if (context.Pow() != null)
                {
                    return new Tuple<double, string>(Math.Pow(left.GetAlgebraicValue(), right.GetAlgebraicValue()),
                        context.Pow().GetText());
                }

                throw new AntlrParseException(
                    $"Unable to evaluate the basic expression (unknown operator '{context.GetChild(1).GetText()}')",
                    context.GetText(), RawEventType.AntlrExpr, context.Start, parentStart);
            }
        }

        public List<AntlrValue> VisitValuesTyped(UniversalAffChartParser.ValuesContext context) =>
            (List<AntlrValue>)VisitValues(context);

        /// <returns><see cref="List{AntlrValue}"/></returns>
        public override object VisitValues(UniversalAffChartParser.ValuesContext context)
        {
            return context.value().Select(VisitValueTyped).ToList();
        }

        public AntlrEvent VisitEventTyped(UniversalAffChartParser.EventContext context) =>
            (AntlrEvent)VisitEvent(context);

        /// <returns><see cref="AntlrEvent"/></returns>
        public override object VisitEvent(UniversalAffChartParser.EventContext context)
        {
            var name = context.Word()?.GetText() ?? "";

            var ctxValues = context.values();
            var values = VisitValuesTyped(ctxValues);

            var ctxSubEvents = context.subEvents();

            var subEvents = ctxSubEvents != null ? VisitSubEventsTyped(ctxSubEvents) : new List<AntlrEvent>();

            var ctxProperties = context.properties();
            var propDict = new Dictionary<string, AntlrValue>();

            if (ctxProperties != null)
            {
                var properties = VisitPropertiesTyped(ctxProperties);
                foreach (var (key, value) in properties.Select(x => x.GetKeyValuePair()))
                {
                    propDict.Add(key.ToLower(), value);
                }
            }

            var ctxSegment = context.segment();
            var segment = ctxSegment != null ? VisitSegmentTyped(ctxSegment) : null;

            return new AntlrEvent(
                context.GetText(),
                name,
                values,
                subEvents,
                propDict,
                segment,
                context.Start);
        }

        public AntlrEvent VisitItemTyped(UniversalAffChartParser.ItemContext context) => (AntlrEvent)VisitItem(context);

        /// <returns><see cref="AntlrEvent"/></returns>
        public override object VisitItem(UniversalAffChartParser.ItemContext context)
        {
            return VisitEventTyped(context.@event());
        }

        public AntlrValue VisitPropertyTyped(UniversalAffChartParser.PropertyContext context) =>
            (AntlrValue)VisitProperty(context);

        /// <returns><see cref="AntlrValue"/></returns>
        public override object VisitProperty(UniversalAffChartParser.PropertyContext context)
        {
            var key = context.Word().GetText();
            if (context.Colon() != null && context.value() != null)
            {
                var value = VisitValueTyped(context.value());

                return AntlrValue.FromKeyValuePair($"{key}={value.Raw}", new Tuple<string, AntlrValue>(key, value),
                    context.Start.Line,
                    context.Start.Column);
            }

            return AntlrValue.FromKeyValuePair(key, new Tuple<string, AntlrValue>(key, null),
                context.Start.Line,
                context.Start.Column);
        }

        public List<AntlrValue> VisitPropertiesTyped(UniversalAffChartParser.PropertiesContext context) =>
            (List<AntlrValue>)VisitProperties(context);

        /// <returns><see cref="List{AntlrValue}"/></returns>
        public override object VisitProperties(UniversalAffChartParser.PropertiesContext context)
        {
            return context == null
                ? new List<AntlrValue>()
                : context.property().Select(VisitPropertyTyped).ToList();
        }

        public List<AntlrEvent> VisitSubEventsTyped(UniversalAffChartParser.SubEventsContext context) =>
            (List<AntlrEvent>)VisitSubEvents(context);

        /// <returns><see cref="List{AntlrEvent}"/></returns>
        public override object VisitSubEvents(UniversalAffChartParser.SubEventsContext context)
        {
            return context.@event().Select(VisitEventTyped).ToList();
        }

        public AntlrEventSegment VisitSegmentTyped(UniversalAffChartParser.SegmentContext context) =>
            (AntlrEventSegment)VisitSegment(context);

        /// <returns><see cref="AntlrEventSegment"/></returns>
        public override object VisitSegment(UniversalAffChartParser.SegmentContext context)
        {
            return VisitBodyTyped(context.body());
        }

        public AntlrEventSegment VisitBodyTyped(UniversalAffChartParser.BodyContext context) =>
            (AntlrEventSegment)VisitBody(context);

        /// <returns><see cref="AntlrEventSegment"/></returns>
        public override object VisitBody(UniversalAffChartParser.BodyContext context)
        {
            var items = context.item().Select(VisitItemTyped).ToList();

            return new AntlrEventSegment(items, context.Start);
        }
    }
}