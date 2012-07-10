using System;
using Rebel.Framework.Localization.Processing.ValueFormatters;

namespace Rebel.Framework.Localization.Web.JavaScript.ValueFormatters
{
    public class DefaultGenerator : PatternProcessorGenerator<DefaultFormatter>
    {

        public override void WriteEvaluator(DefaultFormatter proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            writer.Output.Write("dv(");
            argumentWriters[0]();
            writer.Output.Write(")");
        }
    }
}
