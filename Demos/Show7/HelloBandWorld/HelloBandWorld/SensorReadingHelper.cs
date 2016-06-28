namespace HelloBandWorld
{
  using Microsoft.Band;
  using Microsoft.Band.Sensors;
  using System;
  using System.ComponentModel;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Windows.UI.Core;
  class FormattedSensorReadingEventArgs : EventArgs
  {
    public FormattedSensorReadingEventArgs(string reading)
    {
      this.Reading = reading;
    }
    public string Reading { get; }
  }
  interface ISensorReadingHelper
  {
    Task<bool> InitialiseAsync();
    Task ShutdownAsync();
    string Name { get; }
    string Value { get; }
  }
  class SensorReadingHelper<T> : ISensorReadingHelper, INotifyPropertyChanged where T : IBandSensorReading
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public SensorReadingHelper(
      string name,
      IBandSensor<T> sensor,
      Func<T, string> stringFormatter)
    {
      this.sensor = sensor;
      this.stringFormatter = stringFormatter;
      this.Name = name;
      this.syncContext = SynchronizationContext.Current;
    }
    public string Name
    {
      get; private set;
    }
    public string Value
    {
      get; private set;
    }
    public async Task<bool> InitialiseAsync()
    {
      bool available = false;

      if (sensor.IsSupported)
      {
        var consent = sensor.GetCurrentUserConsent();

        available = (consent == UserConsent.Granted);

        if (!available)
        {
          available = await sensor.RequestUserConsentAsync();
        }
      }
      if (available)
      {
        // We could change the reporting interval but let's not!
#if LOTS_OF_DATA_LOW_BATTERY
        var interval = sensor.SupportedReportingIntervals.Min();
        sensor.ReportingInterval = interval;
#endif

        sensor.ReadingChanged += OnReadingChanged;
        await sensor.StartReadingsAsync();
      }
      return (available);
    }
    public async Task ShutdownAsync()
    {
      this.sensor.ReadingChanged -= this.OnReadingChanged;
      await this.sensor.StopReadingsAsync();
    }
    void OnReadingChanged(object sender, BandSensorReadingEventArgs<T> e)
    {
      this.Value = this.stringFormatter(e.SensorReading);

      SendOrPostCallback action = _ =>
      {
        this.PropertyChanged?.Invoke(
          this,
          new PropertyChangedEventArgs("Value"));
      };
      if (this.syncContext != null)
      {
        this.syncContext.Post(action, null);
      }
      else
      {
        action(null);
      }
    }
    IBandSensor<T> sensor;
    Func<T, string> stringFormatter;
    SynchronizationContext syncContext;
  }
}
