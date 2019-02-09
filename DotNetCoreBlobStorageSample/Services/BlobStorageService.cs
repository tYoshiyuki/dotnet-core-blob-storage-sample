using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreBlobStorageSample.Services
{
    public interface IBlobStorageService
    {
        CloudBlockBlob GetBlob(string containerName, string blobName);
        Task<List<CloudBlockBlob>> GetBlobs(string containerName, string prefix);

    }

    public class BlobStorageService : IBlobStorageService
    {
        public BlobStorageService(string connectionString)
        {
            SetConnectionString(connectionString);
        }

        private CloudStorageAccount cloudStorageAccount;

        public void SetConnectionString(string connectionString)
        {
            cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public CloudBlockBlob GetBlob(string containerName, string blobName)
        {
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync().Wait();

            return container.GetBlockBlobReference(blobName);
        }

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
