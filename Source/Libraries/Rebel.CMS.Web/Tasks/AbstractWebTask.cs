using System;
using Rebel.Cms.Web.Context;
using Rebel.Framework.Context;
using Rebel.Framework.Tasks;

namespace Rebel.Cms.Web.Tasks
{
    public abstract class AbstractWebTask : AbstractTask
    {
        protected AbstractWebTask(IRebelApplicationContext applicationContext) : base(applicationContext.FrameworkContext)
        {
            ApplicationContext = applicationContext;
        }

        public IRebelApplicationContext ApplicationContext { get; private set; }
    }
}