namespace WpfRealSenseHands.Controls
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;

  public partial class DepthMappedVideoControl : UserControl, ISampleProcessor
  {
    public DepthMappedVideoControl()
    {
      InitializeComponent();
    }
    public void Initialise(PXCMSenseManager senseManager)
    {
      this.senseManager = senseManager;

      senseManager.EnableStream(
        PXCMCapture.StreamType.STREAM_TYPE_COLOR, 0, 0, 0).ThrowOnFail();

      senseManager.EnableStream(
        PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 0, 0, 0).ThrowOnFail();
    }
    public void ProcessFrame(PXCMCapture.Sample sample)
    {
      this.currentColorImage = null;

      if ((sample?.depth != null) && (sample?.color != null))
      {
        this.mappedImage = 
          this.Projection.CreateColorImageMappedToDepth(
            sample.depth, 
            sample.color);

        PXCMImage.ImageData colorMappedImageData;

        if (this.mappedImage.AcquireAccess(
          PXCMImage.Access.ACCESS_READ,
          PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32,
          out colorMappedImageData).IsSuccessful())
        {
          this.InitialiseImageDimensions();
          this.currentColorImage = colorMappedImageData;
        }
        this.mappedImage.ReleaseAccess(colorMappedImageData);
      }
    }
    public void DrawUI(PXCMCapture.Sample sample)
    {
      if (this.currentColorImage != null)
      {
        this.InitialiseImage();

        this.writeableBitmap.WritePixels(
          this.imageDimensions,
          this.currentColorImage.planes[0],
          this.imageDimensions.Width * this.imageDimensions.Height * 4,
          this.imageDimensions.Width * 4);

        sample.color.ReleaseAccess(this.currentColorImage);
        this.mappedImage.Dispose();
        this.currentColorImage = null;
      }
    }
    void InitialiseImageDimensions()
    {
      if (!this.imageDimensions.HasArea)
      {
        this.imageDimensions.Width = this.mappedImage.info.width;
        this.imageDimensions.Height = this.mappedImage.info.height;
      }
    }
    void InitialiseImage()
    {
      if (this.writeableBitmap == null)
      {
        this.writeableBitmap = new WriteableBitmap(
          this.imageDimensions.Width,
          this.imageDimensions.Height,
          96,
          96,
          PixelFormats.Bgra32,
          null);

        this.displayImage.Source = this.writeableBitmap;
      }
    }
    PXCMProjection Projection
    {
      get
      {
        if (this.projection == null)
        {
          this.projection = 
            this.senseManager.captureManager.device.CreateProjection();
        }
        return (this.projection);
      }
    }
    PXCMSenseManager senseManager;
    PXCMImage.ImageData currentColorImage;
    Int32Rect imageDimensions;
    WriteableBitmap writeableBitmap;
    PXCMProjection projection;
    PXCMImage mappedImage;
  }
}
