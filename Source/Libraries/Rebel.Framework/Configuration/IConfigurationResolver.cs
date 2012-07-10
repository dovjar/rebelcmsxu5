namespace Rebel.Framework.Configuration
{
    public interface IConfigurationResolver
    {
        object GetConfigSection(string name);
    }
}