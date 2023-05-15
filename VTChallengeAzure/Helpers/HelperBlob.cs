using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using VTChallengeAzure.Services;

namespace VTChallengeAzure.Helpers {
    public class HelperBlob {

        private ServiceStorageBlob service;
        private string containerName;

        public HelperBlob(ServiceStorageBlob service) {
            this.service = service;
            this.containerName = "usuariosblobs";
        }

        public async Task<string> GetUri(string blobName) {
            BlobContainerClient blobContainerClient = await this.service.GetContainerAsync(containerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

            BlobSasBuilder sasBuilder = new BlobSasBuilder() {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddHours(1),
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            var uri = blobClient.GenerateSasUri(sasBuilder);
            return uri.ToString();
        }

    }
}
