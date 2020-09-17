using System;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using McMaster.Extensions.CommandLineUtils;

namespace BlobSasGenerator
{
    class Program
    {
        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0)]
        private string BlobName { get; }

        [Argument(1)]
        private string BlobContainerName { get; }

        [Argument(2)]
        private string AccountName { get; }

        [Argument(3)]
        private string AccountKey { get; }

        private void OnExecute()
        {
            var connectionString =
                $"DefaultEndpointsProtocol=https;AccountName={AccountName};AccountKey={AccountKey};EndpointSuffix=core.windows.net";

            var container = new BlobContainerClient(connectionString, BlobContainerName);
            var key = new StorageSharedKeyCredential(AccountName, AccountKey);

            Console.Write(GetBlobSasUri(container, BlobName, key));

        }

        private static string GetBlobSasUri(BlobContainerClient container,
            string blobName, StorageSharedKeyCredential key, string storedPolicyName = null)
        {
            // Create a SAS token that's valid for one hour.
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container.Name,
                BlobName = blobName,
                Resource = "b"
            };

            if (storedPolicyName == null)
            {
                sasBuilder.StartsOn = DateTimeOffset.UtcNow;
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            // Use the key to get the SAS token.
            var sasToken = sasBuilder.ToSasQueryParameters(key).ToString();

            Console.WriteLine("SAS for blob is: {0}", sasToken);
            Console.WriteLine();

            return $"{container.GetBlockBlobClient(blobName).Uri}?{sasToken}" ;
        }
    }
}
