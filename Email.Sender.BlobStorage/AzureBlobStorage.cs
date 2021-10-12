using Email.Sender.CrossCutting.Envs;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Email.Sender.BlobStorage
{
    public class AzureBlobStorage : IBlobStorage
    {
        private readonly string blobConnectionString;
        private readonly string blobContainer;
        private readonly ConcurrentDictionary<string, string> containersVerified = new ConcurrentDictionary<string, string>();

        public AzureBlobStorage()
        {
            blobConnectionString = !string.IsNullOrWhiteSpace(EnvironmentVars.BlobConnectionString)
                ? EnvironmentVars.BlobConnectionString
                : throw new System.ArgumentException("Blob connection string was null");

            blobContainer = !string.IsNullOrWhiteSpace(EnvironmentVars.BlobContainer)
                ? EnvironmentVars.BlobContainer
                : throw new System.ArgumentException("Blob container string was null");
        }

        public async Task<Stream> GetStreamAsync(IBlob blob)
        {
            var cloudBlobContainer = await GetCloudBlobContainerAsync();
            var blobReference = cloudBlobContainer.GetBlobReference(GetBlobPath(blob));
            return await GetBlobStreamAsync(blobReference);
        }

        private async Task<CloudBlobContainer> GetCloudBlobContainerAsync()
        {
            var cloudBlobClient = CloudStorageAccount.Parse(blobConnectionString).CreateCloudBlobClient();
            if (!containersVerified.ContainsKey(blobContainer))
                await VerifyContainerAsync(cloudBlobClient);

            return cloudBlobClient.GetContainerReference(blobContainer);
        }

        private async Task VerifyContainerAsync(CloudBlobClient cloudBlobClient)
        {
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(blobContainer);
            if (!cloudBlobContainer.Exists())
            {
                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                await cloudBlobContainer.CreateIfNotExistsAsync();
            }

            containersVerified.TryAdd(blobContainer, cloudBlobContainer.Uri.ToString());
        }

        private async Task<Stream> GetBlobStreamAsync(CloudBlob blobReference)
        {
            var memoryStream = new MemoryStream();
            await Task.Factory.FromAsync(blobReference.BeginDownloadToStream, blobReference.EndDownloadToStream, memoryStream, null);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        private static string GetBlobPath(IBlob blob)
        {
            var types = blob.MimeType.Split('/');
            var extension = types.Length > 1 ? types[1] : types[0];
            return $"{blob.Owner}/{blob.Parent}/{blob.Member}/{blob.BlobId}.{extension}";
        }
    }
}