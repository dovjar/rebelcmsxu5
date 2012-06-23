using System;
using RebelCms.Cms.Web.Context;
using RebelCms.Framework.Context;
using RebelCms.Framework.Tasks;

namespace RebelCms.Cms.Web.Tasks
{
    public abstract class AbstractWebTask : AbstractTask
    {
        protected AbstractWebTask(IRebelCmsApplicationContext applicationContext) : base(applicationContext.FrameworkContext)
        {
            ApplicationContext = applicationContext;
        }

        public IRebelCmsApplicationContext ApplicationContext { get; private set; }
    }
}