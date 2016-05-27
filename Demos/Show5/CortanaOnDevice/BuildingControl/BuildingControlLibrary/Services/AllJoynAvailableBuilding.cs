using Windows.Devices.AllJoyn;

namespace BuildingControl.Services
{
  public class AllJoynRemoteBuilding
  {
    public string Name { get; set; }
    public AllJoynServiceInfo AllJoynInfo { get; set; }
  }
}
