namespace GeofencingBackgroundTasks
{
  using System;
  using Windows.Devices.Geolocation;
  using Windows.Devices.Geolocation.Geofencing;
  public sealed class PositionLogEntry
  {
    private PositionLogEntry()
    {
    }
    internal static PositionLogEntry FromFenceDetails(
      GeofenceState newState, Geoposition position)
    {
      PositionLogEntry entry = new PositionLogEntry()
      {
        Date = DateTimeOffset.Now,
        Latitude = position.Coordinate.Latitude,
        Longitude = position.Coordinate.Longitude,
        Altitude = position.Coordinate.Altitude ?? 0.0d,
        State = newState
      };
      return (entry);
    }
    internal static PositionLogEntry FromLogEntry(string logFileEntry)
    {
      string[] pieces = logFileEntry.Split('|');
      PositionLogEntry entry = new PositionLogEntry()
      {
        Date = DateTimeOffset.Parse(pieces[0]),
        Latitude = double.Parse(pieces[1]),
        Longitude = double.Parse(pieces[2]),
        Altitude = double.Parse(pieces[3]),
        State = (GeofenceState)Enum.Parse(typeof(GeofenceState), pieces[4])
      };
      return (entry);
    }
    public string ToLogEntry()
    {
      return (string.Format(
       "{0}|{1}|{2}|{3}|{4}",
       this.Date.ToUniversalTime(),
       this.Latitude, this.Longitude, this.Altitude, this.State));
    }
    public override string ToString()
    {
      return ($"{this.State} [{this.Latitude:F2},{this.Longitude:F2}]");
    }
    public GeofenceState State { get; internal set; }   
    public DateTimeOffset Date { get; internal set; }
    public double Latitude { get; internal set; }
    public double Longitude { get; internal set; }
    public double Altitude { get; internal set; }
  }
}
