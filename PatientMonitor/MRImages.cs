using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PatientMonitor
{
    internal class MRImages
    {
        public Bitmap AnImage { get; private set; }

        public void LoadImages(string imageFile)
        {
            AnImage = new Bitmap(imageFile);
        }
    }
}
