namespace RebelCms.Framework.Localization.Processing
{
    public interface IParameterEvaluator
    {
        ParameterValue GetValue(EvaluationContext context);
    }
      
}
