namespace WpfRealSenseHands.Controls
{
  using System;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;

  public partial class DepthVideoControl : UserControl, ISampleProcessor
  {
    public DepthVideoControl()
    {
      InitializeComponent();
    }
    public void Initialise(PXCMSenseManager senseManager)
    {
      this.senseManager = senseManager;

      this.senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH,
        640, 480);
    }
    public void ProcessFrame(PXCMCapture.Sample sample)
    {
      this.currentDepthImage = null;

      PXCMImage.ImageData depthImage;

      if (sample.depth != null)
      {
        if (sample.depth.AcquireAccess(PXCMImage.Access.ACCESS_READ,
          PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out depthImage).Succeeded())
        {
          this.InitialiseImageDimensions(sample.depth);

          this.currentDepthImage = depthImage;
        }
      }
    }
    public void DrawUI(PXCMCapture.Sample sample)
    {
      if (this.currentDepthImage != null)
      {
        this.InitialiseImage();

        this.writeableBitmap.WritePixels(
          this.imageDimensions,
          this.currentDepthImage.planes[0],
          this.imageDimensions.Width * this.imageDimensions.Height * 4,
          this.imageDimensions.Width * 4);

        sample.depth.ReleaseAccess(this.currentDepthImage);
        this.currentDepthImage = null;
      }
    }
    void InitialiseImageDimensions(PXCMImage image)
    {
      if (!this.imageDimensions.HasArea)
      {
        this.imageDimensions.Width = image.info.width;
        this.imageDimensions.Height = image.info.height;

        this.senseManager.captureManager.device.SetMirrorMode(PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL);
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


    PXCMSenseManager senseManager;
    PXCMImage.ImageData currentDepthImage;
    Int32Rect imageDimensions;
    WriteableBitmap writeableBitmap;
  }
}
