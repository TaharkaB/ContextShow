namespace HelloDirectInk
{
  using System.Linq;
  using Windows.UI;
  using Windows.UI.Core;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Shapes;

  public sealed partial class MainPage : Page
  {
    void AddProcessingForRightButtonStrokes()
    {
      this.inkPresenter.InputProcessingConfiguration.RightDragAction =
        InkInputRightDragAction.LeaveUnprocessed;

      this.inkPresenter.UnprocessedInput.PointerPressed += OnUnhandledPointerPressed;
      this.inkPresenter.UnprocessedInput.PointerMoved += OnUnhandledPointerMoved;
      this.inkPresenter.UnprocessedInput.PointerReleased += OnUnhandledPointerReleased;
    }
    void OnUnhandledPointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
    {
      this.backingCanvas.Children.Clear();

      this.selectionLine = 
        this.MakeBackingCanvasLine(Colors.Silver, 3, true);

      this.backingCanvas.Children.Add(this.selectionLine);

      this.selectionLine.Points.Add(args.CurrentPoint.Position);
    }
    void OnUnhandledPointerMoved(InkUnprocessedInput sender, PointerEventArgs args)
    {
      if (this.selectionLine != null)
      {         
        this.selectionLine.Points.Add(args.CurrentPoint.Position);
      }
    }
    void OnUnhandledPointerReleased(InkUnprocessedInput sender, PointerEventArgs args)
    {
      if (this.selectionLine != null)
      {
        this.selectionLine.Points.Add(args.CurrentPoint.Position);
        this.inkStrokeContainer.SelectWithPolyLine(this.selectionLine.Points);
        this.UpdateStrokesView();
      }
      this.selectionLine = null;
    }
    void HighlightSelectedStrokes()
    {
      this.backingCanvas.Children.Clear();

      foreach (var selectedStroke in
        this.inkStrokeContainer.GetStrokes().Where(s => s.Selected))
      {
        var rectangle = this.MakeBackingCanvasRectangle(
          selectedStroke.BoundingRect,
          Colors.Silver,
          3,
          true);

        this.backingCanvas.Children.Add(rectangle);
      }
    }
    Polyline selectionLine;
  }
}
