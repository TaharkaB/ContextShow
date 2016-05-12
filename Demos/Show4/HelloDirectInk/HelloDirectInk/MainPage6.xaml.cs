namespace HelloDirectInk
{
  using System;
  using System.Threading.Tasks;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    async void OnRecognizeAsync(object sender, RoutedEventArgs e)
    {
      await this.OnRecognizeAsync(InkRecognitionTarget.Recent);
    }
    async void OnRecognizeSelectedAsync(object sender, RoutedEventArgs e)
    {
      await this.OnRecognizeAsync(InkRecognitionTarget.Selected);
    }
    async Task OnRecognizeAsync(InkRecognitionTarget target)
    {
      // Note that you can interrogate this object for language support but we're
      // leaving this set to the default.
      var inkRecognizerContainer = new InkRecognizerContainer();

      // We ask for recognition and we get back a 'list' of results where
      // each one of those contains a 
      var recognitionResults = await inkRecognizerContainer.RecognizeAsync(
          this.inkStrokeContainer, target);

      // We could just iterate through the recognition results now but, instead,
      // we update the stroke container with those results so that it will store
      // them for us.
      this.inkStrokeContainer.UpdateRecognitionResults(recognitionResults);

      this.UpdateStrokesView();
    }
    void DrawRecognisedStrokes()
    {
      foreach (var recognitionResult in
         this.inkStrokeContainer.GetRecognitionResults())
      {
        var bestRecognisedText =
          recognitionResult.GetTextCandidates()[0];

        RecognisedTextHighlightControl highlight =
          new RecognisedTextHighlightControl(bestRecognisedText);

        highlight.Width = recognitionResult.BoundingRect.Width + 10;
        highlight.Height = recognitionResult.BoundingRect.Height + 10;

        Canvas.SetLeft(highlight, recognitionResult.BoundingRect.Left - 5);
        Canvas.SetTop(highlight, recognitionResult.BoundingRect.Top - 5);

        this.backingCanvas.Children.Add(highlight);
      }
    }
  }
}
