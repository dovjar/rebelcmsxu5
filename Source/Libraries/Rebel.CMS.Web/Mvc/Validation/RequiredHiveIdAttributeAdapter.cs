using System.Web.Mvc;

namespace Rebel.Cms.Web.Mvc.Validation
{
    public class RequiredHiveIdAttributeAdapter : RequiredAttributeAdapter
    {
        public RequiredHiveIdAttributeAdapter(ModelMetadata metadata, ControllerContext context, HiveIdRequiredAttribute attribute)
            : base(metadata, context, attribute)
        {
        }
    }
}