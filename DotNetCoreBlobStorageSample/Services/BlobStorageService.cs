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
        string ConnectionString { set; }
        CloudBlobContainer GetContainer(string containerName);
        CloudBlockBlob GetBlob(string containerName, string blobName);
        Task<List<CloudBlockBlob>> GetBlobs(string containerName, string prefix = null);
    }

    /// <summary>
    /// Azure Blob Storageサービスです
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        public BlobStorageService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// ConnectionStringです
        /// </summary>
        public string ConnectionString
        {
            set
            {
                cloudStorageAccount = CloudStorageAccount.Parse(value);
            }
        }

        /// <summary>
        /// CloudStorageAccountです
        /// </summary>
        private CloudStorageAccount cloudStorageAccount;

        /// <summary>
        /// コンテナを取得します
        /// </summary>
        /// <param name="containerName">コンテナ名</param>
        /// <returns></returns>
        public CloudBlobContainer GetContainer(string containerName)
        {
            return cloudStorageAccount.CreateCloudBlobClient()
                .GetContainerReference(containerName);
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
        public async Task<List<CloudBlockBlob>> GetBlobs(string containerName, string prefix = null)
        {
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync().Wait();
            var results = new List<CloudBlockBlob>();

            BlobContinuationToken continuationToken = null;
            do
            {
                var listResults = prefix == null ? await container.ListBlobsSegmentedAsync(continuationToken)
                    : await container.ListBlobsSegmentedAsync(prefix, continuationToken);
                continuationToken = listResults.ContinuationToken;
                results.AddRange(listResults.Results.Select(_ => (CloudBlockBlob) _));
            } while (continuationToken != null);
            
            return results;
        }
    }
}
