namespace BackgroundComponent
{
  using BuildingControl.Services;
  using BuildingControlLibrary.Services;
  using System;
  using System.Threading.Tasks;
  using Windows.ApplicationModel.AppService;
  using Windows.ApplicationModel.Background;
  using Windows.ApplicationModel.VoiceCommands;
  using Windows.Media.SpeechRecognition;

  public sealed class BackgroundTask : IBackgroundTask
  {
    public async void Run(IBackgroundTaskInstance taskInstance)
    {
      var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

      if (triggerDetails?.Name == "buildingCommandService")
      {
        var deferral = taskInstance.GetDeferral();

        // TODO: come back and fix up cancellation here as we haven't got any.

        // Load up our stored local building configuration if not already loaded.
        if (BuildingPersistence.Instance == null)
        {
          BuildingPersistence.Instance = await BuildingPersistence.LoadAsync();
        }
        // Start building a list of remote building configurations if not already
        // started..
        RemoteLightControlManager.Initialise(BuildingPersistence.Instance.Name);

        // Get the connection to the 'Cortana' end of this conversation.
        var voiceConnection =
          VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

        // We should handle voiceConnection.VoiceCommandCompleted here, we 
        // don't yet.

        // What command has been issued?
        var voiceCommand = await voiceConnection.GetVoiceCommandAsync();

        // Is this the command that we understand?
        if (voiceCommand.CommandName == "backgroundSwitchLights")
        {
          await this.ProcessCommandAsync(voiceConnection, voiceCommand.SpeechRecognitionResult);
        }
        else
        {
          // Need to report an error
          await ReportErrorAsync(voiceConnection, "the command name passed does not match across XML/code");
        }
        deferral.Complete();
      }
    }
    async Task ProcessCommandAsync(
      VoiceCommandServiceConnection voiceConnection,
      SpeechRecognitionResult speechRecognitionResult)
    {
      // Extract the details of the building, room, and onOff value.
      var commandDetails =
        BuildingVoiceCommandParser.Parse(speechRecognitionResult);

      if ((commandDetails?.RulePath != null) &&
          (!string.IsNullOrEmpty(commandDetails.Building)) &&
          (commandDetails.OnOff.HasValue))
      {
        // If we didn't get a room then try and get one now just to show how disambiguation
        // works.
        if (string.IsNullOrEmpty(commandDetails.Room))
        {
        }
        // We may still not have a room, that's fine if we get this far without one.
        if (commandDetails.Building == BuildingPersistence.Instance.Name)
        {
          // Local building, relatively easy.
          if (string.IsNullOrEmpty(commandDetails.Room))
          {
            BuildingPersistence.Instance.SwitchAllLights(commandDetails.OnOff.Value);
          }
          else
          {
            var room = BuildingPersistence.Instance.GetRoomByName(commandDetails.Room);
            if (room != null)
            {
              room.SwitchLights(commandDetails.OnOff.Value);
            }
          }
          await BuildingPersistence.SaveAsync(BuildingPersistence.Instance);
        }
      }
      else
      {
        await ReportErrorAsync(voiceConnection, 
          "we got a result from the speech parser that we were not expecting");
      }
    }
    static async Task ReportErrorAsync(VoiceCommandServiceConnection voiceConnection, string error)
    {
      await voiceConnection.ReportFailureAsync(
        VoiceCommandResponse.CreateResponse(
          new VoiceCommandUserMessage()
          {
            DisplayMessage = error,
            SpokenMessage = error
          }
        )
      );
    }
  }
}
