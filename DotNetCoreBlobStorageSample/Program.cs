using DotNetCoreBlobStorageSample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace DotNetCoreBlobStorageSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 設定ファイルの読み込み
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();

            // サービスの設定
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IBlobStorageService>(factory => new BlobStorageService(configuration["ConnectionStrings:StorageConnection"]));
            var service = serviceCollection.BuildServiceProvider().GetService<IBlobStorageService>();

            // サンプル実装
            var fileName = "201902_01_test_upload.txt";
            var filePath = Path.Combine("./", fileName);
            File.WriteAllText(filePath, "upload test!!");

            // Upload
            var block = service.GetBlob("sample", fileName);
            block.Properties.ContentType = "text/plain";
            await block.UploadFromFileAsync(filePath);

            // Get
            File.Delete(filePath);
            var blobs = await service.GetBlobs("sample", "201902");            
            foreach (var blob in blobs)
            {
                await blob.DownloadToFileAsync(filePath, FileMode.OpenOrCreate);
            }
        }
    }
}