namespace Rebel.Framework.Linq.ResultBinding
{
    public abstract class ObjectBinder
    {
        public abstract object Execute(SourceFieldBinder sourceFieldBinder);
    }
}