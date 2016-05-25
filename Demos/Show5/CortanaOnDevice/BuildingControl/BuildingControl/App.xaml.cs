using BuildingControl.Pages;
using BuildingControl.Services;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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

    protected override async void OnLaunched(LaunchActivatedEventArgs e)
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

      if (e.PrelaunchActivated == false)
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
    private async void OnSuspending(object sender, SuspendingEventArgs e)
    {
      var deferral = e.SuspendingOperation.GetDeferral();

      await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);

      deferral.Complete();
    }
  }
}
