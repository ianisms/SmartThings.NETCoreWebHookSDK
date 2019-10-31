# Installed App Management

I have included an installed app manager utility for helping you store/retrive app configs for your app.  There are 2 variations available in the SDK:  ```FileBackedInstalledAppManager```, and ```AzureStorageBackedInstalledAppManager```.    You can easily add your own by extending the abstract class ```InstalledAppManager```.  The installed app manager is used throughout the SDK and at least one must be configured.

## FileBackedInstalledAppManager Usage

To use the FileBackedInstalledAppManager, you must add a ```FileBackedConfig``` to your service collection like so: ```.Configure<FileBackedConfig<FileBackedInstalledAppManager>>(config.GetSection("FileBackedInstalledAppManager.FileBackedConfig"))```.  You can then inject the ```InstalledAppManager``` like so: ```.AddFileBackedInstalledAppManager()```.

### Example Config

```
  "FileBackedInstalledAppManager.FileBackedConfig": {
    "BackingStorePath": "mysmartappdata/installedAppManager.dat"
  },
```

## AzureStorageBackedInstalledAppManager Usage

To use the AzureStorageBackedInstalledAppManager, you must add a ```AzureStorageBackedConfig``` to your service collection like so: ```.Configure<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>>(config.GetSection("AzureStorageBackedInstalledAppManager.AzureStorageConfig"))```.  You can then inject the ```InstalledAppManager``` like so: ```.AddAzureStorageInstalledAppManager()```.

### Example Config

```
"AzureStorageBackedInstalledAppManager.AzureStorageConfig": {
    "ConnectionString": "<YOURCONNECTIONSTRING>",
    "ContainerName": "mysmartappdata",
    "CacheBlobName": "installedAppManager.data"
  },
```
