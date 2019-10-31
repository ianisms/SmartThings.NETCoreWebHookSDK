# State Management

I have included a state manager utility for helping you store/retrive state for your app.  There are 2 variations available in the SDK:  ```FileBackedStateManager```, and ```AzureStorageBackedStateManager```.  You can easily add your own by extending the abstract class ```StateManager<T>```.

## FileBackedStateManager Usage

To use the FileBackedStateManager, you must add a ```FileBackedConfig``` to your service collection like so: ```.Configure<FileBackedConfig<FileBackedStateManager<MyState>>>(config.GetSection("FileBackedStateManager.FileBackedConfig"))```. ```MyState``` would be your custom state object.  You can then inject the ```StateManager``` like so: ```.AddFileBackedStateManager<MyState>()```.

### Example Config

```json
  "FileBackedStateManager.FileBackedConfig": {
    "BackingStorePath": "mysmartappdata/stateManager.dat"
  },
```

## AzureStorageBackedStateManager Usage

To use the AzureStorageBackedStateManager, you must add a ```AzureStorageBackedConfig``` to your service collection like so: ```.Configure<AzureStorageBackedConfig<AzureStorageBackedStateManager<MyState>>>(config.GetSection("AzureStorageBackedStateManager.AzureStorageConfig"))```. ```MyState``` would be your custom state object.  You can then inject the ```StateManager``` like so: ```.AddAzureStorageStateManager<MyState>()```.

### Example Config

```json
"AzureStorageBackedStateManager.AzureStorageConfig": {
    "ConnectionString": "<YOURCONNECTIONSTRING>",
    "ContainerName": "mysmartappdata",
    "CacheBlobName": "stateManager.data"
  },
```
