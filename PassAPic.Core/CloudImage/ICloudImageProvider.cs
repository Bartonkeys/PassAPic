using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PassAPic.Core.CloudImage
{
    public interface ICloudImageProvider
    {
        string SaveImageToCloud(Image image, string imageName);
    }
}
