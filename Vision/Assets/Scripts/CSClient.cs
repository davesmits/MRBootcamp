#if WINDOWS_UWP
using Newtonsoft.Json;
#endif
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CognitveServices
{
    public class CSClient
    {
#if WINDOWS_UWP
        private const string subscriptionKey = "b005114fd8514f369c04a83bf9636536";
        private const string uriBaseFace =
            "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect";

        private const string uriBaseReconization =
            "https://westeurope.api.cognitive.microsoft.com/vision/v2.0/analyze";

        private const string uriBaseSurfaceProductReconization =
            "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/";

        private const string projectId = "6230861a-d80c-43f2-8253-c66bbefddb93";
        private const string predictionKey = "7b5fdaebf29549678ecfb300beb5fb0c";

        public async Task<Face[]> FaceAnalysesAsync(byte[] softwareBitmap)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.
            string uri = uriBaseFace + "?" + requestParameters;





            using (ByteArrayContent content = new ByteArrayContent(softwareBitmap))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                var response = await client.PostAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                    return null;

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Face[]>(contentString);
            }
        }

        public async Task<VisionResult> ObjectAnalysesASync(byte[] softwareBitmap)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            // The Analyze Image method returns information about the following
            // visual features:
            // Categories:  categorizes image content according to a
            //              taxonomy defined in documentation.
            // Description: describes the image content with a complete
            //              sentence in supported languages.
            // Color:       determines the accent color, dominant color, 
            //              and whether an image is black & white.
            string requestParameters =
                "visualFeatures=Categories,Description,Color";

            // Assemble the URI for the REST API method.
            string uri = uriBaseReconization + "?" + requestParameters;


            // Add the byte array as an octet stream to the request body.
            using (ByteArrayContent content = new ByteArrayContent(softwareBitmap))
            {
                // This example uses the "application/octet-stream" content type.
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Asynchronously call the REST API method.
                var response = await client.PostAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                    return null;

                string contentString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VisionResult>(contentString);

            }

        }

        public async Task<SurfaceProductPrediction> SurfaceProductAnalysesAsync(byte[] softwareBitmap)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-key", predictionKey);


            var uri = $"{uriBaseSurfaceProductReconization}{projectId}/image";


            using (var content = new ByteArrayContent(softwareBitmap))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await client.PostAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                    return null;

                string contentString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SurfaceProductPrediction>(contentString);
            }

        }
#endif
    }

    public class Face
    {
        public string faceId { get; set; }
        public Facerectangle faceRectangle { get; set; }
        public Faceattributes faceAttributes { get; set; }
    }

    public class Facerectangle
    {
        public int top { get; set; }
        public int left { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Faceattributes
    {
        public float smile { get; set; }
        public Headpose headPose { get; set; }
        public string gender { get; set; }
        public float age { get; set; }
        public Facialhair facialHair { get; set; }
        public string glasses { get; set; }
        public Emotion emotion { get; set; }
        public Blur blur { get; set; }
        public Exposure exposure { get; set; }
        public Noise noise { get; set; }
        public Makeup makeup { get; set; }
        public object[] accessories { get; set; }
        public Occlusion occlusion { get; set; }
        public Hair hair { get; set; }
    }

    public class Headpose
    {
        public float pitch { get; set; }
        public float roll { get; set; }
        public float yaw { get; set; }
    }

    public class Facialhair
    {
        public float moustache { get; set; }
        public float beard { get; set; }
        public float sideburns { get; set; }
    }

    public class Emotion
    {
        public float anger { get; set; }
        public float contempt { get; set; }
        public float disgust { get; set; }
        public float fear { get; set; }
        public float happiness { get; set; }
        public float neutral { get; set; }
        public float sadness { get; set; }
        public float surprise { get; set; }
    }

    public class Blur
    {
        public string blurLevel { get; set; }
        public float value { get; set; }
    }

    public class Exposure
    {
        public string exposureLevel { get; set; }
        public float value { get; set; }
    }

    public class Noise
    {
        public string noiseLevel { get; set; }
        public float value { get; set; }
    }

    public class Makeup
    {
        public bool eyeMakeup { get; set; }
        public bool lipMakeup { get; set; }
    }

    public class Occlusion
    {
        public bool foreheadOccluded { get; set; }
        public bool eyeOccluded { get; set; }
        public bool mouthOccluded { get; set; }
    }

    public class Hair
    {
        public float bald { get; set; }
        public bool invisible { get; set; }
        public Haircolor[] hairColor { get; set; }
    }

    public class Haircolor
    {
        public string color { get; set; }
        public float confidence { get; set; }
    }


    public class VisionResult
    {
        public Category[] categories { get; set; }
        public Color color { get; set; }
        public Description description { get; set; }
        public string requestId { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Color
    {
        public string dominantColorForeground { get; set; }
        public string dominantColorBackground { get; set; }
        public string[] dominantColors { get; set; }
        public string accentColor { get; set; }
        public bool isBwImg { get; set; }
        public bool isBWImg { get; set; }
    }

    public class Description
    {
        public string[] tags { get; set; }
        public Caption[] captions { get; set; }
    }

    public class Caption
    {
        public string text { get; set; }
        public float confidence { get; set; }
    }

    public class Metadata
    {
        public int width { get; set; }
        public int height { get; set; }
        public string format { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public float score { get; set; }
    }


    public class SurfaceProductPrediction
    {
        public string id { get; set; }
        public string project { get; set; }
        public string iteration { get; set; }
        public System.DateTime created { get; set; }
        public Prediction[] predictions { get; set; }
    }

    public class Prediction
    {
        public float probability { get; set; }
        public string tagId { get; set; }
        public string tagName { get; set; }
        public Boundingbox boundingBox { get; set; }
    }

    public class Boundingbox
    {
        public float left { get; set; }
        public float top { get; set; }
        public float width { get; set; }
        public float height { get; set; }
    }
}
