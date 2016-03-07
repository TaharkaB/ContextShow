namespace WpfRealsenseFacialDetection.Controls
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;

  public partial class ColorVideoControl : UserControl, IFrameProcessor
  {
    public ColorVideoControl()
    {
      InitializeComponent();
    }
    public int RealSenseModuleId
    {
      get
      {
        return (-1);
      }
    }
    public void Initialise(PXCMSenseManager senseManager)
    {
    }
    public void ProcessFrame(PXCMCapture.Sample sample)
    {
      this.currentColorImage = null;

      PXCMImage.ImageData colorImage;

      if (sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ,
        PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out colorImage).Succeeded())
      {
        this.InitialiseImageDimensions(sample.color);

        this.currentColorImage = colorImage;
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
        this.currentColorImage = null;
      }
    }
    void InitialiseImageDimensions(PXCMImage image)
    {
      if (!this.imageDimensions.HasArea)
      {
        this.imageDimensions.Width = image.info.width;
        this.imageDimensions.Height = image.info.height;
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
    PXCMImage.ImageData currentColorImage;
    Int32Rect imageDimensions;
    WriteableBitmap writeableBitmap;
  }
}
