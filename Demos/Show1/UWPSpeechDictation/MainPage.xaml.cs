namespace Demo1_SpeechDictation
{
  using System;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using System.Threading.Tasks;
  using Windows.Media.SpeechRecognition;
  using Windows.Media.SpeechSynthesis;
  using Windows.UI.Core;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public MainPage()
    {
      this.InitializeComponent();
      this.DataContext = this;
      this.mediaElement = new MediaElement();
      this.mediaElement.MediaEnded += OnMediaEnded;
      this.quarterSecondTimer = new DispatcherTimer()
      {
        Interval = TimeSpan.FromMilliseconds(WPM_TIMER_INTERVAL_MSECS)
      };
      this.quarterSecondTimer.Tick += OnTimerTick;
      this.Loaded += OnLoaded;
    }
    async void OnLoaded(object sender, RoutedEventArgs e)
    {
      await this.StartDictationAsync();
    }
    async Task StartDictationAsync()
    {
      this.speechRecognizer = new SpeechRecognizer();
      this.speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(25);
      this.speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(50);
      this.speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromMilliseconds(50);

      this.speechRecognizer.ContinuousRecognitionSession.ResultGenerated += OnResultGenerated;
      this.speechRecognizer.HypothesisGenerated += OnHypothesisGenerated;

      await speechRecognizer.CompileConstraintsAsync();

      await this.speechRecognizer.ContinuousRecognitionSession.StartAsync(
        SpeechContinuousRecognitionMode.Default);
    }

    async void OnHypothesisGenerated(SpeechRecognizer sender,
      SpeechRecognitionHypothesisGeneratedEventArgs args)
    {
      await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () =>
        {
          if (this.stopWatch == null)
          {
            this.stopWatch = new Stopwatch();
            this.stopWatch.Start();
            this.quarterSecondTimer.Start();
          }
          this.HypothesisedSpeech = args.Hypothesis.Text;
        }
      );
    }
    async void OnResultGenerated(SpeechContinuousRecognitionSession sender,
      SpeechContinuousRecognitionResultGeneratedEventArgs args)
    {
      await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () =>
        {
          this.HypothesisedSpeech = string.Empty;

          if (string.IsNullOrEmpty(this.FullSpeech))
          {
            this.FullSpeech = args.Result.Text;
          }
          else
          {
            this.FullSpeech += $" {args.Result.Text}";
          }
        }
      );
    }
    async Task SayAsync(string text)
    {
      if (this.speechSynthesizer == null)
      {
        var chosenVoice =
          SpeechSynthesizer.AllVoices.FirstOrDefault(
            v => v.Gender == VoiceGender.Female) ?? SpeechSynthesizer.DefaultVoice;

        this.speechSynthesizer = new SpeechSynthesizer()
        {
          Voice = chosenVoice
        };
      }
      this.currentStream = 
        await this.speechSynthesizer.SynthesizeTextToStreamAsync(text);

      this.mediaElement.SetSource(this.currentStream, string.Empty);
      this.mediaElement.Play();
    }
    void OnMediaEnded(object sender, RoutedEventArgs e)
    {
      // this is very clunky. we assume that we only have one thing to
      // say at a time which for this demo is ok but, generally, I
      // think you'd want a queue.
      this.currentStream?.Dispose();
      this.currentStream = null;
    }
    async void OnTimerTick(object sender, object e)
    {
      var words = WordCount(this.FullSpeech) + WordCount(this.HypothesisedSpeech);

      this.WordsPerMinute =
        (decimal)(words / this.stopWatch.Elapsed.TotalMilliseconds) * 60000;

      this.scrollHypothesis.ChangeView(
        0, this.scrollHypothesis.ScrollableHeight, 1.0f);

      this.scrollFullText.ChangeView(
        0, this.scrollFullText.ScrollableHeight, 1.0f);

      if (((++this.tickCount % GET_ON_WITH_IT_TICK_COUNT) == 0) &&
          (this.WordsPerMinute < GET_ON_WITH_IT_WPM))
      {
        // We could also use SSML here to give us much more control
        // around emphasis, timing, etc.
        await this.SayAsync("please, do get on with it");
      }
    }
    public decimal WordsPerMinute
    {
      get
      {
        return (wordsPerMinute);
      }
      set
      {
        this.SetProperty(ref this.wordsPerMinute, value);
      }
    }
    public string FullSpeech
    {
      get
      {
        return (fullSpeech);
      }
      set
      {
        this.SetProperty(ref this.fullSpeech, value);
      }
    }
    public string HypothesisedSpeech
    {
      get
      {
        return (this.hypothesisedSpeech);
      }
      set
      {
        this.SetProperty(ref this.hypothesisedSpeech, value);
      }
    }
    static int WordCount(string text)
    {
      return (text?.Split(' ').Count() ?? 0);
    }
    bool SetProperty<T>(ref T storage, T value,
      [CallerMemberName] String propertyName = null)
    {
      if (object.Equals(storage, value)) return false;

      storage = value;
      this?.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      return true;
    }
    SpeechRecognizer speechRecognizer;
    SpeechSynthesizer speechSynthesizer;
    SpeechSynthesisStream currentStream;
    decimal wordsPerMinute;
    string fullSpeech;
    string hypothesisedSpeech;
    Stopwatch stopWatch;
    int totalWordCount;
    DispatcherTimer quarterSecondTimer;
    MediaElement mediaElement;
    int tickCount;
    const int WPM_TIMER_INTERVAL_MSECS = 250;
    const int GET_ON_WITH_IT_INTERVAL_SECS = 10;
    const int GET_ON_WITH_IT_WPM = 100;
    const int GET_ON_WITH_IT_TICK_COUNT =
      (1000 / WPM_TIMER_INTERVAL_MSECS) * GET_ON_WITH_IT_INTERVAL_SECS;
  }
}
