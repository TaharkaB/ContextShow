using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AnniversaryInk
{
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.inkCanvas.InkPresenter.InputDeviceTypes =
        CoreInputDeviceTypes.Mouse |
        CoreInputDeviceTypes.Pen |
        CoreInputDeviceTypes.Touch;
    }
  }
}
