using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;

namespace PassAPic.Core.CloudImage
{
    public class CloudImageProviderCloudinary : ICloudImageProvider
    {
        public string SaveImageToCloud(Image image, string imageName)
        {
            String urlToReturn = "";

            Account account = new Account(
            "yerma",
            "416993185845278",
            "yNhkrrPZlG5BxZIoqsN67E5yKmw");

            Cloudinary cloudinary = new Cloudinary(account);

            var serverUploadFolder = Path.GetTempPath();
            image.Save(Path.Combine(serverUploadFolder, imageName));

            var localFilePath = Path.Combine(serverUploadFolder, imageName);

            if (File.Exists(localFilePath))
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(localFilePath)
                };
                var uploadResult = cloudinary.Upload(uploadParams);

                urlToReturn = uploadResult.Uri.AbsoluteUri;
            }

            if (File.Exists(localFilePath))
            { File.Delete(localFilePath); }

            return urlToReturn;
        }
    }
   
}
