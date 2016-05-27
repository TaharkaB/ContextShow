using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Windows.Devices.Gpio;

namespace BuildingControl.Model
{
  public class Light : NamedObject, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public Light()
    {
      this.syncContext = SynchronizationContext.Current;
    }
    public RoomType Room { get; set; }

    public bool IsOn
    {
      get
      {
        return (this.isOn);
      }
      set
      {
        if (this.isOn != value)
        {
          this.isOn = value;

          this.syncContext?.Post(
            _ =>
            {
              this.PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs("IsOn"));
            },
            null
          );
          this.SwitchPin();
        }
      }
    }     
    public void SwitchPin()
    {
      if (this.GpioPinNumber != -1)
      {
        if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent(
          "Windows.Devices.DevicesLowLevelContract", 1))
        {
          this.InitialiseGpioPin();

          this.gpioPin?.Write(
            this.IsOn ? GpioPinValue.Low : GpioPinValue.High);
        }
      }
    }
    void InitialiseGpioPin()
    {
      GpioController gpioController = null;

      try { gpioController = GpioController.GetDefault(); }
      catch (FileNotFoundException) { };

      if (gpioController != null)
      {
        GpioPin localPin = null;
        GpioOpenStatus openStatus = GpioOpenStatus.PinUnavailable;

        if (gpioController.TryOpenPin(
          this.GpioPinNumber, GpioSharingMode.Exclusive, out localPin, out openStatus))
        {
          this.gpioPin = localPin;
          this.gpioPin.SetDriveMode(GpioPinDriveMode.Output);
        }
      }
    }

    // Bit 'cheap and cheerful' but makes my life easier later on
    // without having to go and build full on view models for
    // everything.
    public int Id { get; set; }

    public int GpioPinNumber { get; set; }

    bool isOn;

    SynchronizationContext syncContext;

    GpioPin gpioPin;
  }
}