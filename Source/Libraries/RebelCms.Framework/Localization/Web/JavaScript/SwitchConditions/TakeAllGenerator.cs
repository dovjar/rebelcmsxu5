using System;
using RebelCms.Framework.Localization.Processing.SwitchConditions;

namespace RebelCms.Framework.Localization.Web.JavaScript.SwitchConditions
{
    public class TakeAllGenerator : PatternProcessorGenerator<TakeAllCondition>
    {        

        public override void WriteEvaluator(TakeAllCondition proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            writer.Output.Write("true");
        }
    }
}
