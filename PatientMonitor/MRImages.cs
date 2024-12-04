using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace PatientMonitor
{
    internal class MRImages
    {
        private int maxImages = 10;
        private BitmapImage anImage;
        private List<BitmapImage> imageList = new List<BitmapImage>();
        private int currentImageIndex = 0;
        private string stringBase = "";
        private string imageExtension = "";

        public BitmapImage AnImage { get { return anImage; } }
        public int MaxImages { set { maxImages = value; } }

        public MRImages() { }

        public void LoadImages(string imageFile)
        {
            stringBase = imageFile.Substring(0, imageFile.Length - 6);
            Console.WriteLine("stringBase: " + stringBase);
            imageExtension = Path.GetExtension(imageFile);
            Console.WriteLine("imageExtension: " + imageExtension);

            imageList.Clear();
            for (int i = 0; i < maxImages; i++)
            {
                int n = i;
                string fileIndex = "0" + n;
                Console.WriteLine("fileIndex: " + fileIndex);
                string fileExt = ".bmp";
                string imageName = stringBase + fileIndex + fileExt;
                Console.WriteLine("imageName: " + imageName);
                if (File.Exists(imageName))
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(imageName, UriKind.Absolute));
                    imageList.Add(bitmap);
                }
            }

            if (imageList.Count > 0)
            {
                currentImageIndex = 0;
                anImage = imageList[currentImageIndex];
            }
        }

        public void ForwardImages()
        {
            if (imageList.Count == 0) return;

            currentImageIndex = (currentImageIndex + 1) % imageList.Count;
            anImage = imageList[currentImageIndex];
        }

        public void BackImages()
        {
            if (imageList.Count == 0) return;

            currentImageIndex = (currentImageIndex - 1 + imageList.Count) % imageList.Count;
            anImage = imageList[currentImageIndex];
        }
    }
}
