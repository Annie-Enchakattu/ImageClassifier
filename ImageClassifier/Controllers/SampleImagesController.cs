using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ImageClassifier.Models;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Cognitive.CustomVision;
using System.Text;

namespace ImageClassifier.Controllers
{
    public class SampleImagesController : ApiController
    {
        static List<ImageData> imageFilesData = new List<ImageData>();
        string rootPath = "/Images/INat/";

        public IEnumerable<ImageData> Get(int startIndex, int length)
        {
            List<ImageData> sampleImages = new List<ImageData>();
            if (imageFilesData == null
                || imageFilesData.Count == 0)
            {
                GetSampleImages();
            }
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                sampleImages.Add(imageFilesData[i]);
            }
            return sampleImages;
        }

        public List<ImageData> GetAllImagesForSpecies(string className, string speciesName)
        {
            string rootFolderPath = System.Web.HttpContext.Current.Server.MapPath(rootPath);
            string directory = string.Concat(rootFolderPath, "\\", className, "\\", speciesName);
            var images = new DirectoryInfo(directory).GetFiles();
            List<ImageData> imagesData = new List<ImageData>();
            foreach (var img in images)
            {
                var imgData = new ImageData();
                imgData.ClassName = className;
                imgData.SpeciesName = speciesName;
                imgData.URL = string.Concat("/Images/INat/", className, "/", speciesName + "/" + img.Name);
                imagesData.Add(imgData);
            }
            return imagesData;
        }
        private void GetSampleImages()
        {
            string rootFolderPath = System.Web.HttpContext.Current.Server.MapPath(rootPath);
            var directories = new DirectoryInfo(rootFolderPath).GetDirectories();
            foreach (var directory in directories)
            {
                var subDirectories = directory.GetDirectories();
                foreach (var subdirectory in subDirectories)
                {
                    var imgData = new ImageData();
                    imgData.ClassName = directory.Name;
                    imgData.SpeciesName = subdirectory.Name;
                    var image = subdirectory.GetFiles().FirstOrDefault();
                    imgData.URL = string.Concat("/Images/INat/", directory.Name,  "/" , subdirectory.Name ,  "/" , image.Name);
                    imageFilesData.Add(imgData);
                }
            }
        }
        private string GetImageName(string filename)
        {
            var name = filename.Split('.');
            return name[0];
        }
        private string GetFileName(string path)
        {
            var startIndex = path.LastIndexOf("\\") + 1;
            string name = path.Substring(startIndex);
            return name;
        }

    }
}
