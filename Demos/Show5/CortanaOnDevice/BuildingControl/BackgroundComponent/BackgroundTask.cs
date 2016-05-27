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
  using BuildingControl.Model;
  using System.Collections.Generic;
  using Windows.Storage;
  using Newtonsoft.Json;
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

          await voiceConnection.ReportSuccessAsync(MakeResponse("all done"));
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
        // We may still not have a room, that's fine if we get this far without one.
        if (commandDetails.Building == BuildingPersistence.Instance.Name)
        {
          await ProcessLocalBuildingCommandAsync(voiceConnection, commandDetails);
        }
        else
        {
          await ProcessRemoteBuildingCommandAsync(voiceConnection, commandDetails);
        }
      }
      else
      {
        await ReportErrorAsync(voiceConnection, 
          "we got a result from the speech parser that we were not expecting");
      }
    }
    async Task ProcessRemoteBuildingCommandAsync(
      VoiceCommandServiceConnection voiceConnection, 
      BuildingCommandDetails commandDetails)
    {
      // We're on a remote building. Do we know it or not?
      var remoteConsumer = await RemoteLightControlManager.GetConsumerForRemoteBuildingAsync(
        commandDetails.Building, 5);

      if (remoteConsumer != null)
      {
        // If we've not been given a room, let the user choose one if they want to.
        if (string.IsNullOrEmpty(commandDetails.Room))
        {
          // We need to grab the room details - involves a network call and some
          // deserialization :-S
          var remoteBuildingJson = await remoteConsumer.GetBuildingDefinitionJsonAsync();
          var remoteBuilding = JsonConvert.DeserializeObject<Building>(remoteBuildingJson.Json);

          commandDetails.Room = await this.DisambiguateRoomAsync(
            voiceConnection,
            remoteBuilding.Rooms);
        }
        // We've found the building, this is a good sign!
        if (string.IsNullOrEmpty(commandDetails.Room))
        {
          await remoteConsumer.SwitchBuildingAsync(commandDetails.OnOff.Value);
        }
        else
        {
          await remoteConsumer.SwitchRoomAsync(
            commandDetails.Room, commandDetails.OnOff.Value);
        }
      }
      else
      {
        await ReportErrorAsync(
          voiceConnection,
          $"Sorry, I could not find a connection to the {commandDetails.Building}");
      }
    }
    async Task ProcessLocalBuildingCommandAsync(
      VoiceCommandServiceConnection voiceConnection,
      BuildingCommandDetails commandDetails)
    {
      if (string.IsNullOrEmpty(commandDetails.Room))
      {
        commandDetails.Room = await this.DisambiguateRoomAsync(
          voiceConnection,
          BuildingPersistence.Instance.Rooms);
      }
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
    async Task<string> DisambiguateRoomAsync(
      VoiceCommandServiceConnection voiceConnection,
      List<Room> rooms)
    {
      var firstMessage = new VoiceCommandUserMessage()
      {
        DisplayMessage = "which room did you want to change?"
      };
      firstMessage.SpokenMessage = firstMessage.DisplayMessage;

      var repeatMessage = new VoiceCommandUserMessage()
      {
        DisplayMessage = "sorry, I missed that - which room was it?"
      };
      repeatMessage.SpokenMessage = repeatMessage.DisplayMessage;

      var tiles = new List<VoiceCommandContentTile>();

      var tile = await MakeTileAsync("Building", "ms-appx:///Assets/House68x68.png");

      tiles.Add(tile);

      foreach (var room in rooms)
      {
        tile = await MakeTileAsync(
          room.RoomType.ToString(),
          $"ms-appx:///Assets/{room.RoomType}68x68.png");

        tiles.Add(tile);
      }

      var response = VoiceCommandResponse.CreateResponseForPrompt(
        firstMessage, repeatMessage, tiles);

      var choice = await voiceConnection.RequestDisambiguationAsync(response);

      var selectedRoom = 
        choice.SelectedItem.Title == "Building" ? null : choice.SelectedItem.Title;

      return (selectedRoom);
    }
    static async Task<VoiceCommandContentTile> MakeTileAsync(
      string title, string imageFilePath)
    {
      var imageFile = await StorageFile.GetFileFromApplicationUriAsync(
       new Uri(imageFilePath));

      return (
        new VoiceCommandContentTile()
        {
          Title = title,
          ContentTileType = VoiceCommandContentTileType.TitleWith68x68Icon,
          Image = imageFile
        });
    }      
    static async Task ReportErrorAsync(
      VoiceCommandServiceConnection voiceConnection, string error)
    {
      await voiceConnection.ReportFailureAsync(MakeResponse(error));
    }
    static VoiceCommandResponse MakeResponse(string message)
    {
      return (
         VoiceCommandResponse.CreateResponse(
          new VoiceCommandUserMessage()
          {
            DisplayMessage = message,
            SpokenMessage = message
          }
        )
      );
    }
  }
}
