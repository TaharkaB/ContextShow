namespace UWPOxfordFace
{
  using Microsoft.ProjectOxford.Emotion;
  using Microsoft.ProjectOxford.Face;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }
    async void OnFace(object sender, RoutedEventArgs e)
    {
      if (this.cameraDisplay.CapturedPhotoStream != null)
      {
        // This class comes from NuGet packaged Microsoft.ProjectOxford.Face
        // which wraps up the REST API for us.
        FaceServiceClient client = new FaceServiceClient(Keys.FaceKey);

        var results = await client.DetectAsync(
          this.cameraDisplay.CapturedPhotoStream.AsStreamForRead(),
          true,
          true,
          new FaceAttributeType[]
          {
          FaceAttributeType.Age,
          FaceAttributeType.FacialHair,
          FaceAttributeType.Gender,
          FaceAttributeType.HeadPose,
          FaceAttributeType.Smile
          }
        );

        if (results != null)
        {
          foreach (var face in results)
          {
            // Draw a rectangle around the face
            this.cameraDisplay.DrawBox(face.FaceRectangle);

            // Draw all of the facial landmarks (eyes, mouth, etc).
            foreach (var landmark in face.FaceLandmarks.AsDictionary())
            {
              var name = landmark.Key;
              var coordinate = landmark.Value;

              this.cameraDisplay.DrawLandmark(name, coordinate);
            }

            // Draw the extra attributes we get back...
            var attributes = face.FaceAttributes;

            var beard = LabelFromConfidenceValue(
              "beard", attributes.FacialHair.Beard);

            var moustache = LabelFromConfidenceValue(
              "moustache", attributes.FacialHair.Moustache);

            var sideburns = LabelFromConfidenceValue(
              "sideburns", attributes.FacialHair.Sideburns);

            var smile = LabelFromConfidenceValue(
              "smile", attributes.Smile);

            string legend =
              $"{attributes.Age} year old " +
              $"{attributes.Gender} with " +
              $"{smile}, {beard}, {moustache}, {sideburns}";

            this.cameraDisplay.ShowLegend(legend);
          }
        }
      }
    }
    async void OnEmotion(object sender, RoutedEventArgs e)
    {
      if (this.cameraDisplay.CapturedPhotoStream != null)
      {
        var emotionClient = new EmotionServiceClient(Keys.EmotionKey);

        var results = await emotionClient.RecognizeAsync(
          this.cameraDisplay.CapturedPhotoStream.AsStreamForRead());

        var legend = new StringBuilder();

        foreach (var person in results)
        {
          var emotionScores = person.Scores.AsDictionary();

          var labelledScores = 
            emotionScores
            .OrderByDescending(entry => entry.Value)
            .Select(
              entry => new KeyValuePair<string, string>(
               entry.Key,
                LabelFromConfidenceValue(entry.Key, entry.Value)));

          var listOfScores = string.Join(
              ", ",
              labelledScores.Select(entry => entry.Value));

          legend.AppendLine(listOfScores);
        }
        this.cameraDisplay.ShowLegend(legend.ToString());
      }
    }
    static string LabelFromConfidenceValue(string label, double confidence)
    {
      var returnLabel = label;

      if (confidence < 0.5)
      {
        returnLabel = $"no {label}";
      }
      return (returnLabel);
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      // just to keep the control that I wrote happy, no other purpose.
      this.cameraDisplay.SetFrameProcessor(async (bitmap) => await Task.Delay(16));

      this.cameraDisplay.ShowCamera(true);
    }
    async void OnTake(object sender, RoutedEventArgs e)
    {
      if (this.cameraDisplay.CapturedPhotoStream != null)
      {
        this.cameraDisplay.ResetVisuals();
      }
      else
      {
        await this.cameraDisplay.TakePhotoAsync();
      }
    }
  }
}
