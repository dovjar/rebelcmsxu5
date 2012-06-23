namespace RebelCms.Framework.Localization.Processing
{
   
    public interface IValueFormatter
    {
        string FormatValue(ParameterValue value, EvaluationContext context);
    }
      
}
