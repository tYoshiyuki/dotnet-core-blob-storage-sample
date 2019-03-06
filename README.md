# dotnet-core-blob-storage-sample
.NET CoreでAzure Blob Storageを操作するサンプル

# Feature
- .NET Core 2.2
- Azure Blob Storage
- Storage Emulator

# Usage
1. ServiceCollectionにBlobStorageServiceを登録し、アプリケーション内で利用する。
```cs
serviceCollection.AddSingleton<IBlobStorageService>(factory => new BlobStorageService(configuration["ConnectionStrings:StorageConnection"]));
```