namespace BuildingControl.Model
{
  using System;
  using System.Collections.Generic;

  public class Room
  {
    public RoomType RoomType { get; set; }
    public List<Light> Lights { get; set; }

    public void SwitchLights(bool onOff)
    {
      foreach (var light in this.Lights)
      {
        light.IsOn = onOff;
      }
    }
    public void ToggleLight(int interfaceMemberLightIndex)
    {
      if ((interfaceMemberLightIndex >= 0) && 
        (interfaceMemberLightIndex < this.Lights.Count))
      {
        this.Lights[interfaceMemberLightIndex].IsOn =
          !this.Lights[interfaceMemberLightIndex].IsOn;
      }
    }
  }
}
