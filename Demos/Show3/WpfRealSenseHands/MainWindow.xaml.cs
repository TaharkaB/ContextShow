namespace WpfRealSenseHands
{
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Linq;
  using System.Diagnostics;
  using System.Threading;
  using System.Windows.Threading;
  using System.Threading.Tasks;

  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      this.cancelTokenSource = new CancellationTokenSource();
      this.Loaded += OnLoaded;
      this.Closing += OnClosing;
    }
    void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      this.cancelTokenSource.Cancel();
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.senseManager = PXCMSenseManager.CreateInstance();

      this.senseManager.captureManager.SetRealtime(false);

      this.InitialiseRenderers();

      this.senseManager.Init(
        new PXCMSenseManager.Handler()
        {
          onNewSample = this.OnNewSample,
          onModuleProcessedFrame = this.OnModuleProcessedFrame
        }).ThrowOnFail();

      this.senseManager.StreamFrames(false);

      this.senseManager.captureManager.device.SetMirrorMode(
        PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL);
    }
    pxcmStatus OnNewSample(int moduleId, PXCMCapture.Sample sample)
    {
      foreach (var sampleProcessor in this.sampleProcessors.OfType<ISampleProcessor>())
      {
        if (!(sampleProcessor is IModuleProcessor))
        { 
          sampleProcessor.ProcessFrame(sample);
        }
      }
      Dispatcher.InvokeAsync(
        () =>
        {
          foreach (var sampleProcessor in this.sampleProcessors.OfType<ISampleProcessor>())
          {
            if (!(sampleProcessor is IModuleProcessor))
            {
              sampleProcessor.DrawUI(sample);
            }
          }
        }
      );
      return (pxcmStatus.PXCM_STATUS_NO_ERROR);
    }
    pxcmStatus OnModuleProcessedFrame(int mid, PXCMBase module, PXCMCapture.Sample sample)
    {
      foreach (var moduleProcessor in this.sampleProcessors.OfType<IModuleProcessor>())
      {
        if (moduleProcessor.RealSenseModuleId == mid)
        {
          moduleProcessor.ProcessFrame(sample);
        }
      }
      Dispatcher.InvokeAsync(
        () =>
        {
          foreach (var moduleProcessor in this.sampleProcessors.OfType<IModuleProcessor>())
          {
            if (moduleProcessor.RealSenseModuleId == mid)
            {
              moduleProcessor.DrawUI(sample);
            }
          }
        }
      );
      return (pxcmStatus.PXCM_STATUS_NO_ERROR);
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
    CancellationTokenSource cancelTokenSource;
    IEnumerable<ISampleProcessor> sampleProcessors;
    PXCMSenseManager senseManager;
  }
}