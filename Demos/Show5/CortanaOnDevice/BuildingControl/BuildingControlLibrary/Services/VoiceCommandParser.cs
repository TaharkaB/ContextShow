namespace BuildingControlLibrary.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using Windows.Media.SpeechRecognition;

  public class BuildingCommandDetails
  {
    public string RulePath { get; set; }
    public string Building { get; internal set; }
    public string Room { get; internal set; }
    public bool? OnOff { get; internal set; }
  }
  public static class BuildingVoiceCommandParser
  {
    public static BuildingCommandDetails Parse(SpeechRecognitionResult speechResult)
    {
      BuildingCommandDetails details = new BuildingCommandDetails();

      // Which rule were we invoked with?
      var rulePath = speechResult.RulePath;

      if (speechResult.RulePath?.Count > 0)
      {
        details.RulePath = speechResult.RulePath[0];

        var properties = speechResult.SemanticInterpretation.Properties;
        IReadOnlyList<string> buildingList = null;
        IReadOnlyList<string> roomList = null;
        IReadOnlyList<string> onOffList = null;

        properties.TryGetValue("building", out buildingList);
        properties.TryGetValue("room", out roomList);
        properties.TryGetValue("onOff", out onOffList);

        details.Building = buildingList?.FirstOrDefault();
        details.Room = roomList?.FirstOrDefault();
        var onOff = onOffList?.FirstOrDefault();

        if (onOff != null)
        {
          details.OnOff = onOff == "On";
        }
      }
      return (details);
    }
  }
}
