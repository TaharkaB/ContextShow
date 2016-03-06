namespace UWPOxfordFaceIdentification
{
  using Microsoft.ProjectOxford.Face;
  using System;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.UI.Popups;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
      this.faceClient = new FaceServiceClient(Keys.FaceKey);
    }
    async void OnSubmitFaceForUser(object sender, RoutedEventArgs e)
    {
      await this.cameraDisplay.TakePhotoAsync();

      // We can have many groups of up to 1000 members but we just hard-code
      // a single group called 'MyPeople'.
      var personGroupId = await this.CreateOrGetPersonGroupAsync(
          "MyPeople");

      // We then go to grab an id for a person with the right name in that
      // group.
      var personName = this.txtPersonName.Text;

      var personId = await this.CreateOrGetPersonAsync(
        personGroupId,
        personName);

      await AccountMap.SetNameForGuidAsync(personId, personName);

      // We then register this face as belonging to that person in that
      // group.
      var result = await this.faceClient.AddPersonFaceAsync(
        personGroupId.ToString(),
        personId,
        this.cameraDisplay.CapturedPhotoStream.AsStreamForRead());

      this.cameraDisplay.ResetVisuals();
    }
    async void OnIdentifyUser(object sender, RoutedEventArgs e)
    {
      await this.cameraDisplay.TakePhotoAsync();

      // We ask Oxford to detect, it will store the face for 24 hours and
      // will give us an Id to use.
      var detectedFaceId = await this.DetectFirstFaceInCapturedPhotoAsync();

      if (detectedFaceId != Guid.Empty)
      {
        var personGroupId = await this.CreateOrGetPersonGroupAsync("MyPeople");

        var results = await this.faceClient.IdentifyAsync(
          personGroupId,
          new Guid[] { detectedFaceId },
          1); // only 1 candidate being asked for

        if (results == null)
        {
          await this.MessageDialog("no results", "that didn't work - got zero results");
        }
        else
        {
          var candidate = results.First().Candidates[0];

          var name = await AccountMap.GetNameForGuidAsync(candidate.PersonId);

          this.cameraDisplay.ShowLegend($"We think this is {name}");

          await Task.Delay(TimeSpan.FromSeconds(5));

          this.cameraDisplay.ResetVisuals();
        }      
      }
    }
    async Task<Guid> DetectFirstFaceInCapturedPhotoAsync()
    {
      Guid faceId = Guid.Empty;

      var detectionResult = await this.faceClient.DetectAsync(
        this.cameraDisplay.CapturedPhotoStream.AsStreamForRead(),
        true);

      var firstResult = detectionResult.FirstOrDefault();

      if (firstResult != null)
      {
        faceId = firstResult.FaceId;
      }
      return (faceId);
    }
    async void OnTrain(object sender, RoutedEventArgs e)
    {
      var personGroupId = await this.CreateOrGetPersonGroupAsync("MyPeople");

      await this.faceClient.TrainPersonGroupAsync(personGroupId);

      var status = await this.faceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

      await this.MessageDialog(
        "training status",
        $"group is now at training status {status.Status}");
    }
    async Task MessageDialog(string title, string content)
    {
      var messageDialog = new MessageDialog(content, title);      

      await messageDialog.ShowAsync();
    }
    async Task<string> CreateOrGetPersonGroupAsync(string personGroupName)
    {
      Guid groupId;

      var allGroups = await this.faceClient.GetPersonGroupsAsync();

      groupId =
        allGroups
        .Where(group => group.Name == personGroupName)
        .Select(group => Guid.Parse(group.PersonGroupId))
        .FirstOrDefault();

      if (groupId == Guid.Empty)
      {
        groupId = Guid.NewGuid();

        await this.faceClient.CreatePersonGroupAsync(
          groupId.ToString(),
          personGroupName);
      }
      return (groupId.ToString());
    }
    async Task<Guid> CreateOrGetPersonAsync(string personGroupId,
      string personName)
    {
      Guid personId;

      var allPeople = await this.faceClient.GetPersonsAsync(personGroupId);

      personId =
        allPeople
        .Where(person => person.Name == personName)
        .Select(person => person.PersonId)
        .FirstOrDefault();

      if (personId == Guid.Empty)
      {
        var result = await this.faceClient.CreatePersonAsync(
          personGroupId.ToString(),
          personName);

        personId = result.PersonId;
      }
      return (personId);
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      // just to keep the control that I wrote happy, no other purpose.
      this.cameraDisplay.SetFrameProcessor(async (bitmap) => await Task.Delay(16));

      this.cameraDisplay.ShowCamera(true);
    }
    FaceServiceClient faceClient;
  }
}
