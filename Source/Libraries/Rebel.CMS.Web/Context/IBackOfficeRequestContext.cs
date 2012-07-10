using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.IO;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Packaging;

namespace Rebel.Cms.Web.Context
{

    

    /// <summary>
    /// Encapsulates information specific to a request handled by Rebel back-office
    /// </summary>
    public interface IBackOfficeRequestContext : IRoutableRequestContext
    {

        SpriteIconFileResolver DocumentTypeIconResolver { get; }
        SpriteIconFileResolver ApplicationIconResolver { get; }
        IResolver<Icon> DocumentTypeThumbnailResolver { get; }


        IPackageContext PackageContext { get; }

    }
}