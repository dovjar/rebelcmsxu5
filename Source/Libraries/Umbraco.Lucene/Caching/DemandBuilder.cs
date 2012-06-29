namespace Umbraco.Lucene.Caching
{
    #region Imports

    using Umbraco.Framework.Context;
    using Umbraco.Framework.DependencyManagement;

    #endregion

    public class DemandBuilder : IDependencyDemandBuilder
    {
        /// <summary>Builds the dependency demands required by this implementation. </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="context">The context for this building session containing configuration etc.</param>
        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            containerBuilder
                .ForFactory(x => new IndexConfiguration(context.MapPath("~/App_Data/DiskCaches/Lucene/")))
                .KnownAsSelf()
                .ScopedAs.Singleton();

            //containerBuilder
            //    .ForFactory(x => new IndexController(x.Resolve<IndexConfiguration>(), null))
            //    .KnownAsSelf()
            //    .OnActivated((ctx, ctrlr) =>
            //    {
            //        var frameworkContext = ctx.Resolve<IFrameworkContext>();
            //        ctrlr.SetFrameworkContext(frameworkContext);
            //    })
            //    .ScopedAs.Singleton();

            containerBuilder
                .ForFactory(x => new IndexController(x.Resolve<IndexConfiguration>(), x.Resolve<IFrameworkContext>))
                .KnownAsSelf()
                //.OnActivated((ctx, ctrlr) =>
                //{
                //    var frameworkContext = ctx.Resolve<IFrameworkContext>();
                //    ctrlr.SetFrameworkContext(frameworkContext);
                //})
                .ScopedAs.Singleton();
        }
    }
}