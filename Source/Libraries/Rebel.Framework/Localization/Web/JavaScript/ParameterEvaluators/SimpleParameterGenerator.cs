using System;
using Rebel.Framework.Localization.Processing.ParameterEvaluators;

namespace Rebel.Framework.Localization.Web.JavaScript.ParameterEvaluators
{
    public class SimpleParameterGenerator : PatternProcessorGenerator<SimpleParameterEvaluator>
    {        

        public override void WriteEvaluator(SimpleParameterEvaluator proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {            
            WriteGetParameter(writer, writer.Json.Serialize(proc.ParameterName));            
        }
    }
}
