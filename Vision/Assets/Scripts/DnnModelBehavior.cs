using CognitveServices;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

public class DnnModelBehavior : MonoBehaviour
{
    private const bool ShouldUseGpu = true;

    private MediaCapturer _mediaCapturer;
    private UserInput _user;

    private readonly string _previousDominantResult;
    private bool _isRunning = false;

    public TextMesh StatusBlock;
    public float ProbabilityThreshold = 0.6f;

    private async void Start()
    {
        try
        {
            // Get components
            _user = GetComponent<UserInput>();

            // Load model
            StatusBlock.text = $"Starting";
#if ENABLE_WINMD_SUPPORT



            // Configure camera to return frames fitting the model input size
            _mediaCapturer = new MediaCapturer();
            await _mediaCapturer.StartCapturing();
            StatusBlock.text = $"Camera started. Running!";

            // Run processing loop in separate parallel Task
            _isRunning = true;
            await Task.Run(async () =>
            {
                var client = new CSClient();
                while (_isRunning)
                {
                    using (var videoFrame = _mediaCapturer.GetLatestFrame())
                    {
                        var bitmap = videoFrame?.SoftwareBitmap;
                        if (bitmap != null)
                        {
                            var data = await EncodedBytes(bitmap, Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId);
                            var result = await client.ObjectAnalysesASync(data);

                            StringBuilder builder = new StringBuilder();
                            if (result != null)
                            {
                                foreach (var item in result.categories)
                                {
                                    builder.AppendLine($"Object: {item.name}, Score: {item.score}");
                                }
                            }
                            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                            {
                                StatusBlock.text = builder.ToString();
                            }, true);
                        }


                    }
                }
            });
#endif
        }
        catch (Exception ex)
        {
            StatusBlock.text = $"Error init: {ex.Message}";
            Debug.LogError(ex);
        }
    }

#if ENABLE_WINMD_SUPPORT
    private async Task<byte[]> EncodedBytes(Windows.Graphics.Imaging.SoftwareBitmap soft, Guid encoderId)
    {
        byte[] array = null;

        // First: Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)
        // Next:  Use ReadAsync on the in-mem stream to get byte[] array

        using (var ms = new InMemoryRandomAccessStream())
        {
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, ms);
            encoder.SetSoftwareBitmap(soft);

            try
            {
                await encoder.FlushAsync();
            }
            catch (Exception ex) { return new byte[0]; }

            array = new byte[ms.Size];
            await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
        }
        return array;
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