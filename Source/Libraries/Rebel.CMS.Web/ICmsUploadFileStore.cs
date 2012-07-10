namespace Rebel.Cms.Web
{
    using Rebel.Hive.ProviderGrouping;
    using Rebel.Hive.RepositoryTypes;

    [RepositoryType("storage://file-uploader")]
    public interface ICmsUploadFileStore : IFileStore
    {
    }
}