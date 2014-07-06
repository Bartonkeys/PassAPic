
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PassAPic.Core.CloudImage
{
    public class CloudImageProviderAzureBlob : ICloudImageProvider
    {
        private const string ACCOUNT_NAME = "passapicstorage";
        private const string ACCOUNT_KEY = "FnbmT5cMYrO0FLN4doGG3om03b3FP730+YXEOVV85clahDRdOovqlX9O6X9pCGqOm5p1nnpu7NEylNsp9BFzRA==";
        public string SaveImageToCloud(Image image, string imageName)
        {

            String urlToReturn = "";

            try
            {

                StorageCredentials creds = new StorageCredentials(ACCOUNT_NAME, ACCOUNT_KEY);
                CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

                CloudBlobClient client = account.CreateCloudBlobClient();

                CloudBlobContainer passapicContainer = client.GetContainerReference("passapicImages");
                passapicContainer.CreateIfNotExists();

                var serverUploadFolder = Path.GetTempPath();
                image.Save(Path.Combine(serverUploadFolder, imageName));

                var localFilePath = Path.Combine(serverUploadFolder, imageName);

                if (File.Exists(localFilePath))
                {

                    CloudBlockBlob blob = passapicContainer.GetBlockBlobReference(imageName);
                    using (Stream file = System.IO.File.OpenRead(localFilePath))
                    {
                        blob.UploadFromStream(file);
                    }

                    urlToReturn = blob.Uri.AbsoluteUri;
                }

                if (File.Exists(localFilePath))
                { File.Delete(localFilePath); }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return urlToReturn;

           
        }

        
        public string SaveImageToCloud(string imagePath, string imageName)
        {
            String urlToReturn = "";

            try
            {

                StorageCredentials creds = new StorageCredentials(ACCOUNT_NAME, ACCOUNT_KEY);
                CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

                CloudBlobClient client = account.CreateCloudBlobClient();

                CloudBlobContainer passapicContainer = client.GetContainerReference("passapicimages");
                passapicContainer.CreateIfNotExists();

                if (File.Exists(imagePath))
                {

                    CloudBlockBlob blob = passapicContainer.GetBlockBlobReference(imageName);
                    using (Stream file = System.IO.File.OpenRead(imagePath))
                    {
                        blob.UploadFromStream(file);
                    }

                    urlToReturn = blob.Uri.AbsoluteUri;
                }

                if (File.Exists(imagePath))
                { File.Delete(imagePath); }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return urlToReturn;
        }
    }
   
}
