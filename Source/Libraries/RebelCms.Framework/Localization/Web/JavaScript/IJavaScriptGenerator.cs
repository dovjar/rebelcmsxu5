using System;
using System.IO;

namespace RebelCms.Framework.Localization.Web.JavaScript
{
    public interface IJavaScriptGenerator
    {
        void WritePrerequisites(TextWriter writer);

        void WriteEvaluator(object patternProcessor, JavaScriptExpressionWriter writer, params Action[] argumentWriters);
    }
}
