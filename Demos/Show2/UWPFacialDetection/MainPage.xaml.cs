namespace UWPFacialDetection
{
  using System;
  using System.Threading.Tasks;
  using Windows.Graphics.Imaging;
  using Windows.Media.FaceAnalysis;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.DataContext = this;
      this.cameraDisplay.SetFrameProcessor(this.OnProcessFrameAsync);
    }
    async Task OnProcessFrameAsync(SoftwareBitmap bitmap)
    {
      if (this.faceDetector == null)
      {
        this.faceDetector = await FaceDetector.CreateAsync();
      }
      var faces = await this.faceDetector.DetectFacesAsync(bitmap);

      this.cameraDisplay.ShowCamera(faces.Count > 0);

      if (faces.Count > 0)
      {
        foreach (var face in faces)
        {
          this.cameraDisplay.HighlightFace(face.FaceBox);
        }
      }
    }
    FaceDetector faceDetector;
  }
}
