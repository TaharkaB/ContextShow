namespace GeofencingBackgroundTasks
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.Devices.Geolocation;
  using Windows.Devices.Geolocation.Geofencing;
  using Windows.Storage;

  internal class PositionLogger
  {
    internal PositionLogger(string fileName)
    {
      this.fileName = fileName;
    }
    internal async Task Log(GeofenceState newState, Geoposition position)
    {
      StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
        this.fileName, CreationCollisionOption.OpenIfExists);

      PositionLogEntry entry = PositionLogEntry.FromFenceDetails(newState,
        position);

      await FileIO.AppendLinesAsync(file, new string[] { entry.ToLogEntry() });
    }
    internal async Task<IEnumerable<PositionLogEntry>> ReadEntriesAsync()
    {
      List<PositionLogEntry> list = new List<PositionLogEntry>();

      try
      {
        StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(
          this.fileName);

        IList<string> lines = await FileIO.ReadLinesAsync(file);

        list.AddRange(
          lines.Select(
            line => PositionLogEntry.FromLogEntry(line)
          )
        );
      }
      catch (FileNotFoundException)
      {

      }
      return (list);
    }
    internal async Task TruncateLogAsync()
    {
      try
      {
        StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(
          fileName);

        await file.DeleteAsync();
      }
      catch (FileNotFoundException)
      {

      }
    }
    string fileName;
  }
}
