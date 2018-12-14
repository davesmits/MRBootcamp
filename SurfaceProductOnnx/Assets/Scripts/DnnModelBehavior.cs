using CustomVision;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using Windows.ApplicationModel;
using Windows.Storage;
#endif

public class DnnModelBehavior : MonoBehaviour
{
    private const bool ShouldUseGpu = true;
#if ENABLE_WINMD_SUPPORT
    private StorageFile _file;
#endif
    private ObjectDetection _dnnModel;
    private MediaCapturer _mediaCapturer;
    private UserInput _user;

    private string _previousDominantResult;
    private bool _isRunning = false;

    public TextMesh StatusBlock;
    public float ProbabilityThreshold = 0.6f;

    async void Start()
    {
        try
        {
            // Get components
            _user = GetComponent<UserInput>();

            // Load model
            StatusBlock.text = $"Loading ONNX ...";
#if ENABLE_WINMD_SUPPORT

            _file = await Package.Current.InstalledLocation.GetFileAsync("model.onnx");
            _dnnModel = new ObjectDetection(new[] { "Arc Mouse", "Surface Book", "Surface Pro" });
            await _dnnModel.Init(_file);
            StatusBlock.text = $"Loaded model. Starting camera...";


            // Configure camera to return frames fitting the model input size
            _mediaCapturer = new MediaCapturer();
            await _mediaCapturer.StartCapturing(416, 416);
            StatusBlock.text = $"Camera started. Running!";

            // Run processing loop in separate parallel Task
            _isRunning = true;
            await Task.Run(async () =>
            {
                while (_isRunning)
                {
                    using (var videoFrame = _mediaCapturer.GetLatestFrame())
                    {
                        if (videoFrame != null)
                        {
                            await EvaluateFrame(videoFrame);
                        }
                    }
                }
            });
#endif
        }
        catch (Exception ex)
        {
#if ENABLE_WINMD_SUPPORT
            string filename = _file != null ? _file.Name : "nofile";
            StatusBlock.text = $"File: {filename}, Error init: {ex.Message}";
#endif
            Debug.LogError(ex);
        }
    }

#if ENABLE_WINMD_SUPPORT
    private async Task EvaluateFrame(Windows.Media.VideoFrame videoFrame)
    {
        try
        {
            var result = await _dnnModel.PredictImageAsync(videoFrame);

            var first = result.FirstOrDefault();
            if (first != null && first.Probability > .5)
            {
                // Further process and surface results to UI on the UI thread
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    // Measure distance between user's head and gaze ray hit point => distance to object
                    var distMessage = string.Empty;
                    if (_user.GazeHitDistance < 1)
                    {
                        distMessage = string.Format("{0:f0} {1}", _user.GazeHitDistance * 100, "centimeter");
                    }
                    else
                    {
                        distMessage = string.Format("{0:f1} {1}", _user.GazeHitDistance, "meter");
                    }

                    // Prepare strings for text and update labels
                    var labelText = $"Found: {first.TagName}";
                    
                    StatusBlock.text = labelText;

                }, false);
            }
        }
        catch (Exception ex)
        {
            //IsRunning = false;
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                //StatusBlock.text = $"Error loop: {ex.Message}";
                //Debug.LogError(ex);
                //Debug.LogError(videoFrame.Direct3DSurface == null ? "D3D null" : "D3D set");
                //if (videoFrame.Direct3DSurface != null)
                //{
                //    Debug.LogError(videoFrame.Direct3DSurface.Description.Format);
                //    Debug.LogError(videoFrame.Direct3DSurface.Description.Width);
                //    Debug.LogError(videoFrame.Direct3DSurface.Description.Height);
                //}
            }, false);
        }
    }

#endif

    private async void OnDestroy()
    {
        _isRunning = false;
        if (_mediaCapturer != null)
        {
            await _mediaCapturer.StopCapturing();
        }
    }
}