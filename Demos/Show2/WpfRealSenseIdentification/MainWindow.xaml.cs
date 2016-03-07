
namespace WpfRealSenseIdentification
{
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Linq;
  using WpfRealSenseIdentification;

  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.session = PXCMSession.CreateInstance();

      this.senseManager = this.session.CreateSenseManager();

      this.senseManager.captureManager.SetRealtime(false);

      this.senseManager.captureManager.FilterByStreamProfiles(
        PXCMCapture.StreamType.STREAM_TYPE_COLOR, 1280, 720, 0);

      this.InitialiseRenderers();

      // this will fail enless we have at least one control
      // in the renderer list which switches on some kind
      // of modular data.
      this.senseManager.Init(
        new PXCMSenseManager.Handler()
        {
          onModuleProcessedFrame = this.OnModuleProcessedFrame
        }).ThrowOnFail();

      this.senseManager.StreamFrames(false);
    }
    void InitialiseRenderers()
    {
      this.renderers = this.BuildRenderers();

      foreach (var renderer in this.renderers)
      {
        renderer.Initialise(this.senseManager);
      }
    }
    void ForAllRenderers(int moduleId, Action<IFrameProcessor> action)
    {
      foreach (var renderer in this.renderers.Where(
        r => (r.RealSenseModuleId == -1) || (r.RealSenseModuleId == moduleId)))
      {
        action(renderer);
      }
    }
    List<IFrameProcessor> BuildRenderers()
    {
      List<IFrameProcessor> list = new List<IFrameProcessor>();

      foreach (var control in this.parentGrid.Children)
      {
        IFrameProcessor renderer = control as IFrameProcessor;
        if (renderer != null)
        {
          list.Add(renderer);
        }
      }
      return (list);
    }
    pxcmStatus OnModuleProcessedFrame(int mid, PXCMBase module, PXCMCapture.Sample sample)
    {
      ForAllRenderers(
        mid,
        r => r.ProcessFrame(sample));

      Dispatcher.InvokeAsync(() =>
      {
        ForAllRenderers(mid, r => r.DrawUI(sample));
      }
      );
      return (pxcmStatus.PXCM_STATUS_NO_ERROR);
    }
    IEnumerable<IFrameProcessor> renderers;
    PXCMSenseManager senseManager;
    PXCMSession session;
  }
}
