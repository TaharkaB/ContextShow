namespace UWPFacialDetection
{
  using global::System;
  using global::System.Collections.Generic;
  using global::System.Threading.Tasks;
  using global::System.Runtime.InteropServices.WindowsRuntime;
  using global::System.Linq;
  using Windows.Devices.Enumeration;
  using Windows.Foundation;
  using Windows.Graphics.Imaging;
  using Windows.Media;
  using Windows.Media.Capture;
  using Windows.Media.FaceAnalysis;
  using Windows.Media.MediaProperties;
  using Windows.Storage;
  using Windows.Storage.Pickers;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Media.Imaging;
  using Windows.UI.Xaml.Controls;

  public sealed partial class CameraDisplay : UserControl
  {
    public CameraDisplay()
    {
      this.InitializeComponent();
      this.capturingTask = new TaskCompletionSource<bool>();
      this.Loaded += OnLoaded;
    }
    public void SetFrameProcessor(Func<SoftwareBitmap, Task> processor)
    {
      // We don't allow you to change this once it's set - it's a
      // one shot.
      if (this.faceProcessor != null)
      {
        throw new InvalidOperationException("Already set - sorry, can't change it");
      }
      this.faceProcessor = processor;

      Task.Run(
        async () =>
        {
          await this.capturingTask.Task;

          this.capturingTask = null;

          while (true)
          {
            if (!this.faceProcessingPaused)
            {
              await this.mediaCapture.GetPreviewFrameAsync(this.videoFrame);

              await this.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                  await this.faceProcessor(this.videoFrame.SoftwareBitmap);
                }
              );
            }
            else
            {
              // cheap and cheerful.
              await Task.Delay(1000);
            }
          }
        }
      );
    }
    public void HighlightFace(BitmapBounds faceBox)
    {
      if (this.canvasHighlight.Visibility != Visibility.Visible)
      {
        this.canvasHighlight.Visibility = Visibility.Visible;
      }
      if (this.HasFaceBoxMovedSinceLastFrame(faceBox))
      {
        // we have to scale this because the Canvas is scaled to take up the entire
        // width and height of the window but the face box is sized relative to the
        // size of the preview video stream.
        int scaledX = this.ScaleXVideoValueToCanvas((int)faceBox.X);
        int scaledY = this.ScaleYVideoValueToCanvas((int)faceBox.Y);
        int scaledWidth = this.ScaleXVideoValueToCanvas((int)faceBox.Width);
        int scaledHeight = this.ScaleYVideoValueToCanvas((int)faceBox.Height);

        Canvas.SetLeft(this.ellipseHighlight, scaledX);
        Canvas.SetTop(this.ellipseHighlight, scaledY);
        this.ellipseHighlight.Width = scaledWidth;
        this.ellipseHighlight.Height = scaledHeight;
      }
    }
    public void ShowCamera(bool on = true)
    {
      if (on && (this.gridCover.Visibility != Visibility.Collapsed))
      {
        this.gridCover.Visibility = Visibility.Collapsed;
        this.ellipseHighlight.Visibility = Visibility.Visible;
      }
      else if (!on && (this.gridCover.Visibility != Visibility.Visible))
      {
        this.gridCover.Visibility = Visibility.Visible;
        this.ellipseHighlight.Visibility = Visibility.Collapsed;
      }
    }
    public void ResetVisuals()
    {
      this.faceProcessingPaused = false;
      this.canvasHighlight.Visibility = Visibility.Collapsed;
      this.imgSnap.Visibility = Visibility.Collapsed;
      this.gridLegend.Visibility = Visibility.Collapsed;
      this.txtLegend.Text = string.Empty;
      this.gridCover.Visibility = Visibility.Visible;
      this.lastFaceBox = null;
    }
    public void ShowLegend(string legend)
    {
      this.txtLegend.Text = legend;
      this.gridLegend.Visibility = Visibility.Visible;
    }
    public async void Snap()
    {
      this.faceProcessingPaused = true;

      SoftwareBitmapSource source = new SoftwareBitmapSource();

      SoftwareBitmap bitmap = SoftwareBitmap.Convert(
        this.videoFrame.SoftwareBitmap, BitmapPixelFormat.Bgra8);

      await source.SetBitmapAsync(bitmap);

      this.imgSnap.Source = source;

      this.canvasHighlight.Visibility = Visibility.Collapsed;
      this.imgSnap.Visibility = Visibility.Visible;
    }
    public async Task SaveAsync()
    {
      var savePicker = new FileSavePicker();
      savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
      savePicker.SuggestedFileName = "picture.png";
      savePicker.FileTypeChoices.Add("PNG File", new string[] { ".png" });

      var file = await savePicker.PickSaveFileAsync();

      if (file != null)
      {
        var rtb = new RenderTargetBitmap();

        await rtb.RenderAsync(this.gridRenderGrid);

        var pixels = await rtb.GetPixelsAsync();

        using (var stream = await file.OpenAsync(
          FileAccessMode.ReadWrite))
        {
          var encoder = await BitmapEncoder.CreateAsync(
            BitmapEncoder.PngEncoderId, stream);

          encoder.SetPixelData(BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Straight,
            (uint)rtb.PixelWidth,
            (uint)rtb.PixelHeight,
            96d,
            96d,
            pixels.ToArray());

          await encoder.FlushAsync();
        }
      }
    }
    int ScaleXVideoValueToCanvas(int x)
    {
      return (this.ScaleVideoValueToCanvas(x, this.previewVideoSize.Width,
        this.canvasHighlight.ActualWidth));
    }
    int ScaleYVideoValueToCanvas(int y)
    {
      return (this.ScaleVideoValueToCanvas(y, this.previewVideoSize.Height,
        this.canvasHighlight.ActualHeight));
    }
    int ScaleVideoValueToCanvas(int value, double maxVideoValue, double maxCanvasValue)
    {
      double relativeValue = (value / maxVideoValue) * maxCanvasValue;
      return ((int)relativeValue);
    }
    async void OnLoaded(object sender, RoutedEventArgs e)
    {
      var cameras = await this.GetCameraDetailsAsync();

      var firstCamera = cameras.FirstOrDefault();

      if (firstCamera != null)
      {
        MediaCaptureInitializationSettings settings =
          new MediaCaptureInitializationSettings();

        settings.VideoDeviceId = firstCamera.Id;

        this.mediaCapture = new MediaCapture();

        await this.mediaCapture.InitializeAsync(settings);

        this.InitialiseFlowDirectionFromCameraLocation(firstCamera);

        this.captureElement.Source = this.mediaCapture;

        await this.mediaCapture.StartPreviewAsync();

        this.InialisePreviewSizesFromVideoDevice();

        this.InitialiseVideoFrameFromDetectorFormats();

        this.capturingTask.SetResult(true);
      }
    }
    void InitialiseFlowDirectionFromCameraLocation(DeviceInformation firstCamera)
    {
      var externalCamera =
        (firstCamera.EnclosureLocation?.Panel ==
        Windows.Devices.Enumeration.Panel.Unknown);

      var frontCamera =
        (!externalCamera && firstCamera.EnclosureLocation?.Panel ==
          Windows.Devices.Enumeration.Panel.Front);

      this.captureElement.FlowDirection =
      this.imgSnap.FlowDirection =
      this.canvasHighlight.FlowDirection =
        frontCamera ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }
    void InitialiseVideoFrameFromDetectorFormats()
    {
      var bitmapFormats = FaceDetector.GetSupportedBitmapPixelFormats();

      this.videoFrame = new VideoFrame(
        bitmapFormats.First(),
        (int)this.previewVideoSize.Width,
        (int)this.previewVideoSize.Height);
    }
    void InialisePreviewSizesFromVideoDevice()
    {
      var previewProperties =
        this.mediaCapture.VideoDeviceController.GetMediaStreamProperties(
        MediaStreamType.VideoPreview) as VideoEncodingProperties;

      this.previewVideoSize = new Size(
        previewProperties.Width, previewProperties.Height);
    }

    async Task<List<DeviceInformation>> GetCameraDetailsAsync()
    {
      var deviceInfo = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

      List<DeviceInformation> returnedList = new List<DeviceInformation>();

      if (deviceInfo != null)
      {
        returnedList = deviceInfo.OrderBy(
          di =>
          {
            var sortOrder = 2;

            if (di.EnclosureLocation != null)
            {
              sortOrder = di.EnclosureLocation.Panel ==
                Windows.Devices.Enumeration.Panel.Front ? 0 : 1;
            }
            return (sortOrder);
          }
        ).ToList();
      }
      return (returnedList);
    }
    bool HasFaceBoxMovedSinceLastFrame(BitmapBounds box)
    {
      bool changed = this.lastFaceBox == null;

      if (this.lastFaceBox.HasValue)
      {
        int x = (int)Math.Abs(this.lastFaceBox.Value.X - box.X);
        int y = (int)Math.Abs(this.lastFaceBox.Value.Y - box.Y);
        int w = (int)Math.Abs(this.lastFaceBox.Value.Width - box.Width);
        int h = (int)Math.Abs(this.lastFaceBox.Value.Height - box.Height);
        int delta = (int)Math.Max(x, y);
        delta = Math.Max(delta, w);
        delta = Math.Max(delta, h);

        changed = delta > MIN_SIZE_DELTA;
      }
      this.lastFaceBox = box;

      return (changed);
    }
    static readonly int MIN_SIZE_DELTA = 10;
    VideoFrame videoFrame;
    Func<SoftwareBitmap, Task> faceProcessor;
    MediaCapture mediaCapture;
    TaskCompletionSource<bool> capturingTask;
    Size previewVideoSize;
    BitmapBounds? lastFaceBox;
    volatile bool faceProcessingPaused;
  }
}
