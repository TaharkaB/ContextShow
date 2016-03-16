namespace WpfRealSenseIdentification.Controls
{
  using System.IO;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows.Shapes;
  using UWPOxfordFaceIdentification;

  using RecognitionType = PXCMFaceConfiguration.RecognitionConfiguration.RecognitionRegistrationMode;

  public partial class FaceControl : UserControl, IFrameProcessor
  {
    public FaceControl()
    {
      InitializeComponent();
    }
    // This control is making use of the face module from RealSense.
    public int RealSenseModuleId
    {
      get { return (PXCMFaceModule.CUID); }
    }
    public void Initialise(PXCMSenseManager senseManager)
    {
      this.senseManager = senseManager;

      // We configure by switching on the face module.
      this.senseManager.EnableFace();

      // Now, grab that module.
      this.faceModule = this.senseManager.QueryFace();

      // Configure it...
      using (var config = faceModule.CreateActiveConfiguration())
      {
        // We want face detection. Only doing 1 face for now.
        config.detection.isEnabled = true;
        config.detection.maxTrackedFaces = 1;

        // We want face recognition.
        var recognitionConfig = config.QueryRecognition();
        recognitionConfig.Enable();

        recognitionConfig.SetAccuracyThreshold(ACCURACY_THRESHOLD);

        recognitionConfig.SetRegistrationMode(
          RecognitionType.REGISTRATION_MODE_ON_DEMAND);

        // Try and restore any previously saved faces from a local file
        try
        {
          byte[] persistedFaceDb = File.ReadAllBytes(PERSISTED_FACE_DB);
          recognitionConfig.SetDatabaseBuffer(persistedFaceDb);
        }
        catch (FileNotFoundException)
        {

        }     
        config.ApplyChanges().ThrowOnFail();
      }
      this.faceData = this.faceModule.CreateOutput();
    }
    public void ProcessFrame(PXCMCapture.Sample sample)
    {
      if (this.faceData.Update().Succeeded())
      {
        var firstFace = this.faceData.QueryFaces().FirstOrDefault();

        if (firstFace != null)
        {
          // face recognition. have we already identified?
          if (string.IsNullOrEmpty(this.identifiedUserName))
          {
            var recognition = firstFace.QueryRecognition();

            // does the camera recognise the user?
            if (recognition.IsRegistered())
            {
              // ask the camera for the ID it knows the user under
              var id = recognition.QueryUserID();

              // map that ID to a name (stored in a local file)
              var name = AccountMap.GetNameForId(id);

              this.identifiedUserName = $"identified {name}";
            }
            else if (!string.IsNullOrEmpty(this.userNameToRegister))
            {
              // ask the camera to come up with an ID for this user
              int userId = recognition.RegisterUser();

              // store the map between their name and their id in a local file.
              AccountMap.SetNameForIdAsync(userId, this.userNameToRegister);

              // Write all stored faces to the local file
              this.SaveStoredFacesToFile();

              this.userNameToRegister = null;
            }
          }
        }
        else
        {
          this.identifiedUserName = string.Empty;
        }
      }
    }
    void SaveStoredFacesToFile()
    {
      var recognitionModule = this.faceData.QueryRecognitionModule();

      if (recognitionModule != null)
      {
        byte[] data = new byte[recognitionModule.QueryDatabaseSize()];
        if (recognitionModule.QueryDatabaseBuffer(data))
        {
          File.WriteAllBytes(PERSISTED_FACE_DB, data);
        }
      }
    }
    public void DrawUI(PXCMCapture.Sample sample)
    {
      this.txtUserIdentifier.Text = this.identifiedUserName;
    }
    void OnRegisterUser(object sender, RoutedEventArgs e)
    {
      this.userNameToRegister = this.txtUserToRegister.Text;
      this.txtUserToRegister.Text = string.Empty;
    }
    // Sharing these across threads is really not very nice but 
    // I kind of 'get away with it' for this demo.
    string identifiedUserName;
    string userNameToRegister;

    PXCMFaceData faceData;
    PXCMFaceModule faceModule;
    PXCMSenseManager senseManager;
    const int ACCURACY_THRESHOLD = 90;

    const string PERSISTED_FACE_DB = "faces.bin";
  }
}