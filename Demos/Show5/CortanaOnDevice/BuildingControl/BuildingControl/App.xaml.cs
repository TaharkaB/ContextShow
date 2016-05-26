using BuildingControl.Pages;
using BuildingControl.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BuildingControl
{
  sealed partial class App : Application
  {
    public App()
    {
      this.InitializeComponent();
      this.Suspending += OnSuspending;
    }
    bool IsIoTCore =>
      AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT";

    protected override async void OnLaunched(LaunchActivatedEventArgs e)
    {
      // Blowing up for me on IoT Core 14342 right now so stopping it.
      if (!IsIoTCore)
      {
        await RegisterVoiceCommandsAsync();
      }
      await CreateUIAndLoadBuildingDataAsync(e.PrelaunchActivated);
    }

    static async Task CreateUIAndLoadBuildingDataAsync(
      bool prelaunchActivated)
    {
      Frame rootFrame = Window.Current.Content as Frame;

      // Do not repeat app initialization when the Window already has content,
      // just ensure that the window is active
      if (rootFrame == null)
      {
        // Create a Frame to act as the navigation context and navigate to the first page
        rootFrame = new Frame();
        Navigator.Frame = rootFrame;

        // Place the frame in the current Window
        Window.Current.Content = rootFrame;
      }

      if (prelaunchActivated == false)
      {
        if (rootFrame.Content == null)
        {
          BuildingPersistence.Instance = await BuildingPersistence.LoadAsync();

          Type pageType = typeof(MonitorPage);

          if (BuildingPersistence.Instance == null)
          {
            pageType = typeof(SetupBuildingPage);
          }
          rootFrame.Navigate(pageType, null);
        }
        Window.Current.Activate();
      }
    }
    async void OnSuspending(object sender, SuspendingEventArgs e)
    {
      var deferral = e.SuspendingOperation.GetDeferral();

      await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);

      deferral.Complete();
    }
    static async Task RegisterVoiceCommandsAsync()
    {
      var storageFile =
        await StorageFile.GetFileFromApplicationUriAsync(
          new Uri("ms-appx:///CortanaCommands.xml"));

      await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(
        storageFile);
    }
    protected async override void OnActivated(IActivatedEventArgs args)
    {
      base.OnActivated(args);

      if (args.Kind == ActivationKind.VoiceCommand)
      {
        // Create the UI. We assume that the configuration has been done
        // before any voice commands are issued. Wouldn't be ok for a real
        // app but is ok for us.
        await CreateUIAndLoadBuildingDataAsync(false);

        VoiceCommandActivatedEventArgs voiceArgs = 
          (VoiceCommandActivatedEventArgs)args;

        // Which rule were we invoked with?
        var rulePath = voiceArgs.Result.RulePath;

        if (rulePath.Count > 0)
        {
          switch (rulePath[0])
          {
            case "showLights":
              var building =
                voiceArgs.Result.SemanticInterpretation.Properties["building"][0];

              if (!string.IsNullOrEmpty(building))
              {
                await MonitorPage.Instance.SwitchToBuildingAsync(building);
              }
              break;
            default:
              break;
          }
        }
      }
    }
    public string WaitingFilter { get; set; }
  }
}
