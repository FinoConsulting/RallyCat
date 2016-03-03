using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace RallyCat.Core.Services
{
    public class AzureService
    {
        private readonly RallyBackgroundDataService _RallyBackgroundDataService;

        public AzureService(RallyBackgroundDataService backgroundDataService)
        {
            _RallyBackgroundDataService = backgroundDataService;
        }

        public String Upload(Image img, String fileName)
        {
            var azureName         = _RallyBackgroundDataService.RallyGlobalConfiguration.AzureBlobName;
            var azureToken        = _RallyBackgroundDataService.RallyGlobalConfiguration.AzureToken;
            var azureContainerRef = _RallyBackgroundDataService.RallyGlobalConfiguration.AzureBlobContainerRef;
            var kanbanImageFormat = _RallyBackgroundDataService.RallyGlobalConfiguration.KanbanImageFormat;

            var storageCreds      = new StorageCredentials(azureName, azureToken);
            var storageAcct       = new CloudStorageAccount(storageCreds, false);
            var blobClient        = storageAcct.CreateCloudBlobClient();

            var bcon              = blobClient.GetContainerReference(azureContainerRef);
            var cbb               = bcon.GetBlockBlobReference(fileName + DateTime.Now.ToString("o").Replace(":", "_") + ".jpg");

            cbb.Properties.ContentType = kanbanImageFormat;

            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                var buffer = stream.GetBuffer();
                cbb.UploadFromByteArray(buffer, 0, buffer.Count());
            }
            return cbb.Uri.AbsoluteUri;
        }
    }
}