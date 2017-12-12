using ImageClassifier.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Cognitive.CustomVision;
using System.IO;
using System.Web;

namespace ImageClassifier.Controllers
{
    public class PredictImageController : ApiController
    {
        [HttpPost]
        public List<PredictionData> GetPredictionOfFile()
        {
            var file = System.Web.HttpContext.Current.Request.Files["Image"];
            return PredictImage(file.InputStream);
        }

        public List<PredictionData> GetImagePrediction(string imagePath)
        {
            FileStream fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(imagePath), FileMode.Open, FileAccess.Read);
            return PredictImage(fileStream);
        }

        private static List<PredictionData> PredictImage(Stream stream)
        {
            var predictionKey = System.Web.Configuration.WebConfigurationManager.AppSettings["CustomVisionAI-PredictionKey"];
            //var predictionKey = "65d6c49a72a14c0fb068ea8f30218e26";
            PredictionEndpointCredentials predictionEndpointCredentials = new PredictionEndpointCredentials(predictionKey);
            PredictionEndpoint endpoint = new PredictionEndpoint(predictionEndpointCredentials);

            //var projectId = new Guid("faf45e97-218b-418c-8dc0-641e7ba3f535");
            var projectId = new Guid(System.Web.Configuration.WebConfigurationManager.AppSettings["CustomVisionAI-ProjectId"]);

            var result = endpoint.PredictImage(projectId, stream);

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