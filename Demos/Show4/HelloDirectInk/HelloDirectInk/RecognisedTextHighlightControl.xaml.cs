namespace HelloDirectInk
{
  using Windows.UI.Xaml.Controls;

  public sealed partial class RecognisedTextHighlightControl : UserControl
  {
    public RecognisedTextHighlightControl(string text)
    {
      this.InitializeComponent();
      this.txtRecognised.Text = text;
    }
  }
}
