using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using HgCo.WindowsLive.SkyDrive;
using System.IO;

namespace PassAPic.Core.CloudImage
{
    /****
     * NB This implementation does NOT work at present
     * There is no publicly visible URL to return from a OneDrive file upload
     */
    class CloudImageProviderOneDrive : ICloudImageProvider
    {
        public string SaveImageToCloud(Image image, string imageName)
        {
            String urlToReturn = null;

            var client = new SkyDriveServiceClient();

            client.LogOn("yermalimited@hotmail.com", "Y)rm91234");
            WebFolderInfo wfInfo = new WebFolderInfo();

            WebFolderInfo[] wfInfoArray = client.ListRootWebFolders();

            wfInfo = wfInfoArray[0];
            client.Timeout = 1000000000;

            var serverUploadFolder = Path.GetTempPath();
            image.Save(Path.Combine(serverUploadFolder, imageName));

            var localFilePath = Path.Combine(serverUploadFolder, imageName);


            //string fn = @"test.txt";
            if (File.Exists(localFilePath))
            {
                var webFileInfo = client.UploadWebFile(localFilePath, wfInfo);
                urlToReturn = webFileInfo.Path;
            }

            return urlToReturn;
        }



        public string SaveImageToCloud(string imagePath, string imageName)
        {
            String urlToReturn = null;

            var client = new SkyDriveServiceClient();

            client.LogOn("yermalimited@hotmail.com", "Y)rm91234");
            WebFolderInfo wfInfo = new WebFolderInfo();

            WebFolderInfo[] wfInfoArray = client.ListRootWebFolders();

            wfInfo = wfInfoArray[0];
            client.Timeout = 1000000000;

            //string fn = @"test.txt";
            if (File.Exists(imagePath))
            {
                var webFileInfo = client.UploadWebFile(imagePath, wfInfo);
                urlToReturn = webFileInfo.Path;
            }

            return urlToReturn;
        }
    }
}
