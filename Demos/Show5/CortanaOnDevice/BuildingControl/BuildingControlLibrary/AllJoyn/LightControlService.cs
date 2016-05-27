namespace BuildingControl.AllJoyn
{
  using com.taulty.LightControl;
  using Model;
  using Newtonsoft.Json;
  using Services;
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.Devices.AllJoyn;
  using Windows.Foundation;

  public class LightControlService : ILightControlService
  {
    public IAsyncOperation<LightControlGetBuildingDefinitionJsonResult> GetBuildingDefinitionJsonAsync(
      AllJoynMessageInfo info)
    {
      return (this.InternalGetBuildingDefinitionJsonAsync().AsAsyncOperation());
    }

    private async Task<LightControlGetBuildingDefinitionJsonResult> InternalGetBuildingDefinitionJsonAsync()
    {
      var json = JsonConvert.SerializeObject(BuildingPersistence.Instance);

      return (LightControlGetBuildingDefinitionJsonResult.CreateSuccessResult(json));
    }

    public IAsyncOperation<LightControlSwitchBuildingResult> SwitchBuildingAsync(
      AllJoynMessageInfo info,
      bool interfaceMemberOnOff)
    {
      return (this.SwitchBuildingAsync(interfaceMemberOnOff).AsAsyncOperation());
    }
    async Task<LightControlSwitchBuildingResult> SwitchBuildingAsync(bool interfaceMemberOnOff)
    {
      BuildingPersistence.Instance?.SwitchAllLights(interfaceMemberOnOff);

      await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);

      return (LightControlSwitchBuildingResult.CreateSuccessResult());
    }

    public IAsyncOperation<LightControlSwitchRoomResult> SwitchRoomAsync(
      AllJoynMessageInfo info,
      string interfaceMemberRoomName,
      bool interfaceMemberOnOff)
    {
      return (this.InternalSwitchRoomAsync(interfaceMemberRoomName, interfaceMemberOnOff).AsAsyncOperation());
    }
    async Task<LightControlSwitchRoomResult> InternalSwitchRoomAsync(
      string interfaceMemberRoomName,
      bool interfaceMemberOnOff)
    {
      var room = BuildingPersistence.Instance?.GetRoomByName(interfaceMemberRoomName);

      room?.SwitchLights(interfaceMemberOnOff);

      await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);

      return (LightControlSwitchRoomResult.CreateSuccessResult());
    }

    public IAsyncOperation<LightControlToggleRoomLightResult> ToggleRoomLightAsync(
      AllJoynMessageInfo info, 
      string interfaceMemberRoomName, 
      int interfaceMemberLightIndex)
    {
      return (this.InternalToggleRoomLightAsync(
        interfaceMemberRoomName,
        interfaceMemberLightIndex).AsAsyncOperation());
    }
    async Task<LightControlToggleRoomLightResult> InternalToggleRoomLightAsync(
      string interfaceMemberRoomName,
      int interfaceMemberLightIndex)
    {
      var room = BuildingPersistence.Instance?.GetRoomByName(interfaceMemberRoomName);

      room?.ToggleLight(interfaceMemberLightIndex);

      await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);

      return (LightControlToggleRoomLightResult.CreateSuccessResult());
    }
  }
}