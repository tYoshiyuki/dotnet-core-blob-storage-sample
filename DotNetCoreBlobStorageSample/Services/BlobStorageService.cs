using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreBlobStorageSample.Services
{
    /// <summary>
    /// Azure Blob Storageサービスのインターフェースです
    /// </summary>
    public interface IBlobStorageService
    {
        CloudBlockBlob GetBlob(string containerName, string blobName);
        Task<List<CloudBlockBlob>> GetBlobs(string containerName, string prefix);
    }

    /// <summary>
    /// Azure Blob Storageサービスです
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        public BlobStorageService(string connectionString)
        {
            SetConnectionString(connectionString);
        }

        /// <summary>
        /// CloudStorageAccountです
        /// </summary>
        private CloudStorageAccount cloudStorageAccount;

        /// <summary>
        /// ConnectionStringを設定します
        /// </summary>
        /// <param name="connectionString"></param>
        public void SetConnectionString(string connectionString)
        {
            cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
        }

        /// <summary>
        /// Blobを取得します
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="blobName">Blob名</param>
        /// <returns></returns>
        public CloudBlockBlob GetBlob(string containerName, string blobName)
        {
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync().Wait();

            return container.GetBlockBlobReference(blobName);
        }

        /// <summary>
        /// Blobのリストを取得します
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <param name="prefix">Blobのプレフィックス</param>
        /// <returns></returns>
        public async Task<List<CloudBlockBlob>> GetBlobs(string containerName, string prefix)
        {
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync().Wait();
            var results = new List<CloudBlockBlob>();

            BlobContinuationToken continuationToken = null;
            do
            {
                var listResults = await container.ListBlobsSegmentedAsync(prefix, continuationToken);
                continuationToken = listResults.ContinuationToken;
                results.AddRange(listResults.Results.Select(_ => (CloudBlockBlob) _));
            } while (continuationToken != null);
            
            return results;
        }
    }
}
