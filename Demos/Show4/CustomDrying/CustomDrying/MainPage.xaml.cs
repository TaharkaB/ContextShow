namespace CustomDrying
{
  using Microsoft.Graphics.Canvas;
  using Microsoft.Graphics.Canvas.Geometry;
  using Microsoft.Graphics.Canvas.UI.Xaml;
  using System;
  using System.Collections.Generic;
  using System.Numerics;
  using Windows.Foundation;
  using Windows.UI;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();

      // Have to switch on custom drying early in the process and before
      // the InkCanvas has Loaded.
      this.inkSync = this.inkCanvas.InkPresenter.ActivateCustomDrying();

      // The InkPresenter is no longer going to draw ink for us once it
      // has 'dried' so we need to step in, collect it and then make sure
      // that we draw it ourselves.
      this.inkCanvas.InkPresenter.StrokesCollected += OnStrokesCollected;

      // We need somewhere to put the ink strokes.
      this.allInkStrokes = new InkStrokeContainer();

      var attr = this.inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();

      attr.Size = new Size(4, 4);

      this.inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(attr);
    }
    void OnStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
    {
      // We tell the framework that we are ready to begin 'drying'.
      // Keep track of the strokes that need to be dried.
      this.wetInkStrokes = this.inkSync.BeginDry();

      // Ensure that our over-all collection of strokes is up to date.
      this.allInkStrokes.AddStrokes(args.Strokes);

      // Cause our control to repaint itself...
      this.canvasControl.Invalidate();
    }
    /// <summary>
    /// Note that the official sample for Win2D and ink drying does more
    /// than this to get a smoother experience on the inking but I'm
    /// ducking that in the aims of simplicity.
    /// </summary>
    void OnDraw(
      CanvasControl sender,
      CanvasDrawEventArgs args)
    {
      // We use a render target. This lets us avoid having to draw all
      // the ink all the time but, instead, keep around this 'background image'
      // which we draw to incrementally.
      this.CreateRenderTarget();

      var centre = new Vector2(
        (float)sender.ActualWidth / 2.0f,
        (float)sender.ActualHeight / 2.0f);

      // Draw a circle at the 'back' in the z-order.
      args.DrawingSession.FillCircle(
        centre,
        Math.Min((float)sender.ActualWidth / 2.5f, (float)sender.ActualHeight / 2.5f),
        Colors.Red);

      // Draw the ink 'over' it.
      using (var session = this.renderTarget.CreateDrawingSession())
      {
        // If we only need to add some newly 'wet' ink then we do that.
        var strokes = this.wetInkStrokes;

        if (strokes == null)
        {
          strokes = this.allInkStrokes.GetStrokes();
        }
        this.DrawInk(session, strokes);

        if (this.wetInkStrokes != null)
        {
          this.wetInkStrokes = null;
          this.inkSync.EndDry();
        }
      }
      // Draw what we've drawn to the render target to the screen.
      args.DrawingSession.DrawImage(this.renderTarget);

      // Draw a rectangle 'over' that.
      args.DrawingSession.FillRectangle(
        new Windows.Foundation.Rect(
          centre.X - 100,
          centre.Y - 100,
          200,
          200),
        Colors.Blue);
    }
    void DrawInk(CanvasDrawingSession session,
      IReadOnlyList<InkStroke> strokes)
    {
      if (!this.drawWithGeometry)
      {
        // Win2D already knows how to draw ink so let it do it the
        // regular way.
        session.DrawInk(strokes);
      }
      else
      {
        this.DrawInkWithGeometry(session, strokes);
      }
    }
    void DrawInkWithGeometry(
      CanvasDrawingSession ds,
      IReadOnlyList<InkStroke> strokes)
    {
      var strokeStyle = new CanvasStrokeStyle
      {
        DashStyle = CanvasDashStyle.Dash
      };

      var geometry = CanvasGeometry.CreateInk(ds, strokes).Outline();

      // We could pick up the colour(s) from the strokes but we
      // don't here.
      ds.DrawGeometry(geometry, Colors.Black, 1, strokeStyle);
    }
    void OnDrawingModeChanged(object sender, RoutedEventArgs e)
    {
      this.drawWithGeometry = !this.drawWithGeometry;
    }
    void CreateRenderTarget()
    {
      if (renderTarget == null)
      {
        renderTarget = new CanvasRenderTarget(
          this.canvasControl, this.canvasControl.Size);

        using (var session = renderTarget.CreateDrawingSession())
        {
          session.Clear(Colors.Transparent);
        }
      }
    }
    /// <summary>
    /// The render target is sized for the control so needs to get
    /// re-created when the control sizes.
    /// </summary>
    void OnCanvasControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.renderTarget?.Dispose();
      this.renderTarget = null;
    }
    bool drawWithGeometry;
    CanvasRenderTarget renderTarget;
    InkSynchronizer inkSync;
    IReadOnlyList<InkStroke> wetInkStrokes;
    InkStrokeContainer allInkStrokes;
  }
}
