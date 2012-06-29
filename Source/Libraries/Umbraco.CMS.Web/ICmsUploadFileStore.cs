namespace Umbraco.Cms.Web
{
    using Umbraco.Hive.ProviderGrouping;
    using Umbraco.Hive.RepositoryTypes;

    [RepositoryType("storage://file-uploader")]
    public interface ICmsUploadFileStore : IFileStore
    {
    }
}