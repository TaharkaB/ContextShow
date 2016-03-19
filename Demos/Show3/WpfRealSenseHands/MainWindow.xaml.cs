namespace WpfRealSenseHands
{
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows;

  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Assuming it's ok to begin this process here on the UI
      // thread and then move it to our other thread...
      this.senseManager = PXCMSenseManager.CreateInstance();

      this.InitialiseRenderers();

      this.senseManager.Init();

      using (var power = this.senseManager.session.CreatePowerManager())
      {
        power.SetState(PXCMPowerState.State.STATE_PERFORMANCE);
      }
      this.senseManager.captureManager.device.SetMirrorMode(
        PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL);

      // Move most work onto a background thread.
      Task.Run(this.RunFrames);
    }
    async Task RunFrames()
    {
      while (true)
      {
        while (this.senseManager.AcquireFrame(true).Succeeded())
        {
          var sample = this.senseManager.QuerySample();

          // Do the capturing of data on this thread.
          foreach (var sampleProcessor in this.sampleProcessors)
          {
            sampleProcessor.ProcessFrame(sample);
          }
          // Do the drawing on the UI thread BUT waiting for it to
          // happen here which isn't ideal but we are ok with it
          // for now.
          await this.Dispatcher.InvokeAsync(
            () =>
            {
              foreach (var sampleProcessor in this.sampleProcessors)
              {
                sampleProcessor.DrawUI(sample);
              }
            }
          );
          this.senseManager.ReleaseFrame();
        }
      }
    }
    void InitialiseRenderers()
    {
      this.sampleProcessors = this.BuildSampleProcessors();

      foreach (var renderer in this.sampleProcessors)
      {
        renderer.Initialise(this.senseManager);
      }
    }
    List<ISampleProcessor> BuildSampleProcessors()
    {
      List<ISampleProcessor> list = new List<ISampleProcessor>();

      foreach (var control in this.parentGrid.Children)
      {
        var renderer = control as ISampleProcessor;
        if (renderer != null)
        {
          list.Add(renderer);
        }
      }
      return (list);
    }
    IEnumerable<ISampleProcessor> sampleProcessors;
    PXCMSenseManager senseManager;
  }
}