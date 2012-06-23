using System;
using RebelCms.Framework.Localization.Processing.ParameterEvaluators;

namespace RebelCms.Framework.Localization.Web.JavaScript.ParameterEvaluators
{
    public class SimpleParameterGenerator : PatternProcessorGenerator<SimpleParameterEvaluator>
    {        

        public override void WriteEvaluator(SimpleParameterEvaluator proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {            
            WriteGetParameter(writer, writer.Json.Serialize(proc.ParameterName));            
        }
    }
}
