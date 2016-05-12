namespace HelloDirectInk
{
  using System;
  using Windows.Foundation;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    void OnClearSelection(object sender, RoutedEventArgs e)
    {
      foreach (var stroke in this.inkStrokeContainer.GetStrokes())
      {
        stroke.Selected = false;
      }
      this.UpdateStrokesView();
    }
    void OnMoveSelection(object sender, RoutedEventArgs e)
    {
      // Move the ink 50,50 down, just as an example
      this.inkStrokeContainer.MoveSelected(
        new Point(50, 50));

      this.UpdateStrokesView();
    }
    void OnCopySelection(object sender, RoutedEventArgs e)
    {
      this.inkStrokeContainer.CopySelectedToClipboard();
    }
    void OnPaste(object sender, RoutedEventArgs e)
    {
      this.inkStrokeContainer.PasteFromClipboard(
        new Point(0, 0));

      this.UpdateStrokesView();
    }
    void OnSelectAll(object sender, RoutedEventArgs e)
    {
      foreach (var stroke in this.inkStrokeContainer.GetStrokes())
      {
        stroke.Selected = true;
      }
      this.UpdateStrokesView();
    }
    async void OnSave(object sender, RoutedEventArgs e)
    {
      using (var outputFileStream =
        await FileDialogExtensions.PickFileForSaveAsync(
          "ink file", ".isf", "myInkFile"))
      {
        await this.inkPresenter.StrokeContainer.SaveAsync(outputFileStream);
      }
    }
    async void OnLoad(object sender, RoutedEventArgs e)
    {
      using (var inputFileStream =
        await FileDialogExtensions.PickFileForReadAsync(".isf"))
      {
        await this.inkPresenter.StrokeContainer.LoadAsync(inputFileStream);
      }
    }
    void OnDeleteSelected(object sender, RoutedEventArgs e)
    {
      this.inkStrokeContainer.DeleteSelected();
      this.UpdateStrokesView();
    }
  }
}
