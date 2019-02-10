using ChainingAssertion;
using DotNetCoreBlobStorageSample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RimDev.Automation.StorageEmulator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DotNetCoreBlobStorageSample.Test.Services
{
    public class BlobStorageServiceTest : IDisposable
    {
        private AzureStorageEmulatorAutomation _emulator { get; }
        private IBlobStorageService _service { get; }
        private readonly string _containerName = "test";
        private readonly string _testDataPath = "./file";
        private readonly List<string> _testFiles = new List<string> { "1001_test.txt", "1002_test.txt", "1003_test.txt" };

        /// <summary>
        /// Setup
        /// </summary>
        public BlobStorageServiceTest()
        {
            _emulator = new AzureStorageEmulatorAutomation();
            _emulator.Start();

            if (!AzureStorageEmulatorAutomation.IsEmulatorRunning()) throw new Exception("Azure Storage Emulatorの起動に失敗しました");

            // 設定ファイルの読み込み
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // サービスの構成
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IBlobStorageService>(factory => new BlobStorageService(configuration["ConnectionStrings:StorageConnection"]));
            _service = serviceCollection.BuildServiceProvider().GetService<IBlobStorageService>();

            // ファイル生成
            Directory.CreateDirectory(_testDataPath);
            foreach (var fileName in _testFiles)
            {
                File.WriteAllText(Path.Combine(_testDataPath, fileName), "This is test.");
            }
        }

        /// <summary>
        /// Teardown
        /// </summary>
        public void Dispose()
        {
            _emulator.Stop();
            Directory.Delete(_testDataPath, true);
        }

        [Fact]
        public async Task GetBlob_正常取得()
        {
            // Arrange
            await _service.GetContainer(_containerName).DeleteIfExistsAsync();
            var blobName = _testFiles.First();
            var blob = _service.GetBlob(_containerName, blobName);
            await blob.UploadFromFileAsync(Path.Combine(_testDataPath, blobName));

            // Act
            blob = _service.GetBlob(_containerName, blobName);

            // Assert
            blob.Name.Is(blobName);
        }

        [Fact]
        public async Task GetBlobs_全件正常取得()
        {
            // Arrange
            await _service.GetContainer(_containerName).DeleteIfExistsAsync();
            foreach (var fileName in _testFiles)
            {
                var blob = _service.GetBlob(_containerName, fileName);
                await blob.UploadFromFileAsync(Path.Combine(_testDataPath, fileName));
            }

            // Act
            var blobs = await _service.GetBlobs(_containerName);

            // Assert
            blobs.Count().Is(3);
        }

        [Fact]
        public async Task GetBlobs_条件付き正常取得()
        {
            // Arrange
            await _service.GetContainer(_containerName).DeleteIfExistsAsync();
            foreach (var fileName in _testFiles)
            {
                var blob = _service.GetBlob(_containerName, fileName);
                await blob.UploadFromFileAsync(Path.Combine(_testDataPath, fileName));
            }

            // Act
            var blobs = await _service.GetBlobs(_containerName, "1001");

            // Assert
            blobs.Count().Is(1);
        }

    }
}
