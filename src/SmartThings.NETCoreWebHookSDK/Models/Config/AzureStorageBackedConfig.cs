namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.Config
{
    public class AzureStorageBackedConfig<T>
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string CacheBlobName { get; set; }
    }
}