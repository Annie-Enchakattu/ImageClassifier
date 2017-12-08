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
        public IEnumerable<ImageData> Get(int startIndex)
        {
            List<ImageData> sampleImages = new List<ImageData>();
            if (imageFilesData == null 
                || imageFilesData.Count == 0)
            {
                GetImageData();
            }
            for(int i = startIndex; i < (startIndex + 4); i++)
            {
                sampleImages.Add(imageFilesData[i]);
            }
            return imageFilesData;
        }
        public List<PredictionData> Post([FromBody]string value)
        {
            List<PredictionData> error = new List<PredictionData>();
            try
            {
                return PredictImage(System.Web.HttpContext.Current.Server.MapPath(value));
            }
            catch(Exception ex)
            {
                var p = new PredictionData();
                p.Tag = "error" + ex.ToString();
                p.PercentageProbability = "0";
                p.Probability = 0;
                error.Add(p);
                return error;
            }
        }
        
        private List<string> GetImages()
        {
           List<string> imageFiles = 
                Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("/Images")).ToList();
           return imageFiles;
        }
        private void GetImageData()
        {
            var imageFiles = GetImages();
            //var imageDataList = new List<ImageData>();
            foreach(var path in imageFiles)
            {
                var d = new ImageData();
                d.URL = String.Concat("/Images/", GetFileName(path));
                d.Name = GetImageName(GetFileName(path));
                imageFilesData.Add(d);
            }
        }
        private string GetImageName(string filename)
        {
            var name = filename.Split('.');
            return name[0];
        }
        private string GetFileName(string path)
        {
            var startIndex = path.LastIndexOf("\\")+1;
            string name = path.Substring(startIndex);
            return name;
        }
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        static async Task MakePredictionRequest(string imageFilePath)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid subscription key.
            client.DefaultRequestHeaders.Add("Prediction-Key", "65d6c49a72a14c0fb068ea8f30218e26");

            // Prediction URL - replace this example URL with your valid prediction URL.
            
            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/faf45e97-218b-418c-8dc0-641e7ba3f535/image?iterationId=e89db699-c59f-4d27-a261-1e3ef5df181a";

            HttpResponseMessage response;

            // Request body. Try this sample with a locally stored image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }

        private static List<PredictionData> PredictImage(string path)
        {
            var predictionKey = System.Web.Configuration.WebConfigurationManager.AppSettings["CustomVisionAI-predictionKey"];
            //var predictionKey = "65d6c49a72a14c0fb068ea8f30218e26";
            PredictionEndpointCredentials predictionEndpointCredentials = new PredictionEndpointCredentials(predictionKey);
            PredictionEndpoint endpoint = new PredictionEndpoint(predictionEndpointCredentials);

            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

            //var projectId = new Guid("faf45e97-218b-418c-8dc0-641e7ba3f535");
            var projectId = new Guid(System.Web.Configuration.WebConfigurationManager.AppSettings["CustomVisionAI-projectId"]);

            var result = endpoint.PredictImage(projectId, fileStream);

            var predictionDataList = new List<PredictionData>();
            foreach (var c in result.Predictions)
            {
                var p = new PredictionData();
                p.Tag = c.Tag;
                var percentage = c.Probability * 100;
                p.PercentageProbability = (c.Probability * 100).ToString();
                p.Probability = c.Probability;
                predictionDataList.Add(p);
            }

            return predictionDataList.OrderByDescending(p => p.Probability).Take(5).ToList();
        }
    }
}
