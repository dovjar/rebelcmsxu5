using System;
using Rebel.Framework.Localization.Processing.ValueFormatters;

namespace Rebel.Framework.Localization.Web.JavaScript.ValueFormatters
{

    /// <summary>
    /// Uses MicrosoftAjaxGlobalization.js for string.Format functionality
    /// </summary>
    public class StringFormatGenerator : PatternProcessorGenerator<StringFormatFormatter>
    {
        
        public override void WriteEvaluator(StringFormatFormatter proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            if (!string.IsNullOrEmpty(proc.FormatExpression))
            {
                writer.Output.Write("sf(");
                writer.Output.Write(writer.Json.Serialize("{0:" + proc.FormatExpression + "}"));
                writer.Output.Write(",");
                argumentWriters[0]();
                writer.Output.Write(")");
            }
            else
            {
                argumentWriters[0]();
            }
        }
    }
}
