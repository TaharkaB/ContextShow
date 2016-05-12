namespace HelloDirectInk
{
  using System.Collections.Generic;
  using System.Linq;
  using Windows.Foundation;
  using Windows.UI;
  using Windows.UI.Core;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    void AddProcessingForRightButtonStrokes()
    {
      this.inkPresenter.InputProcessingConfiguration.RightDragAction =
        InkInputRightDragAction.LeaveUnprocessed;

      this.inkPresenter.UnprocessedInput.PointerPressed += OnUnhandledPointerReleased;
      this.inkPresenter.UnprocessedInput.PointerMoved += OnUnhandledPointerMoved;
      this.inkPresenter.UnprocessedInput.PointerReleased += OnUnhandledPointerPressed;
    }
    void OnUnhandledPointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
    {
      this.backingCanvas.Children.Clear();
      this.selectionPoints = new List<Point>();
      this.selectionPoints.Add(args.CurrentPoint.Position);
    }
    void OnUnhandledPointerMoved(InkUnprocessedInput sender, PointerEventArgs args)
    {
      if (this.selectionPoints != null)
      {
        var lastPoint = this.selectionPoints[this.selectionPoints.Count - 1];

        var currentPoint = args.CurrentPoint.Position;

        // We should really build up a polyline, this is a cheap version.
        var line =
          this.MakeBackingCanvasLine(
            lastPoint, currentPoint, Colors.Silver, 3, true);

        this.backingCanvas.Children.Add(line);
         
        this.selectionPoints.Add(args.CurrentPoint.Position);
      }
    }
    void OnUnhandledPointerReleased(InkUnprocessedInput sender, PointerEventArgs args)
    {
      if (this.selectionPoints != null)
      {
        this.selectionPoints.Add(args.CurrentPoint.Position);
        this.inkStrokeContainer.SelectWithPolyLine(this.selectionPoints);
        this.UpdateStrokesView();
      }
      this.selectionPoints = null;
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
    List<Point> selectionPoints;
  }
}
