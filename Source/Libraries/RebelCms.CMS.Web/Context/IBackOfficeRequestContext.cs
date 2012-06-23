using System.Web;
using System.Web.Mvc;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.IO;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.Packaging;

namespace RebelCms.Cms.Web.Context
{

    

    /// <summary>
    /// Encapsulates information specific to a request handled by RebelCms back-office
    /// </summary>
    public interface IBackOfficeRequestContext : IRoutableRequestContext
    {

        SpriteIconFileResolver DocumentTypeIconResolver { get; }
        SpriteIconFileResolver ApplicationIconResolver { get; }
        IResolver<Icon> DocumentTypeThumbnailResolver { get; }


        IPackageContext PackageContext { get; }

    }
}