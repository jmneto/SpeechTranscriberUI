// MainWindow.xaml.cs
// This file is part of the SpeechTranscriberUI project, a WPF application for real-time speech transcription.
// The MainWindow class handles the main user interface and integrates with the SpeechProcessor for speech-to-text functionality.
// It includes features such as starting and stopping transcription, copying transcribed text, and summarizing the transcriptions.
// The application also supports configuration management and maintains the window's last position using the registry.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SpeechTranscriber;

namespace SpeechTranscriberUI;
public partial class MainWindow : Window
{
    // Dictionary to hold transcribing text : Key Is SpeakerId, Data is Tuple of Text and DateTime for last change
    Dictionary<string, Tuple<string, DateTime>> transcribingDict = new Dictionary<string, Tuple<string, DateTime>>();

    // Save the Last Speaker 
    private string lastTranscribedSpeakerId = string.Empty;

    // speechKey and speechRegion
    static string? speechKey = string.Empty;
    static string? speechRegion = string.Empty;

    // SpeechProcessor instance
    private SpeechProcessor? speechProcessor;

    // Constructor
    public MainWindow()
    {
        InitializeComponent();

        // Load the last window position from the registry
        var left = RegistryHelper.ReadAppInfo("WINDOWLEFT");
        var top = RegistryHelper.ReadAppInfo("WINDOWTOP");
        if (!string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(top))
        {
            this.Left = double.Parse(left);
            this.Top = double.Parse(top);
        }
        else
        {
            // Center Window on Screen
            Size windowSize = new Size(this.Width, this.Height);
            Rect screenSize = SystemParameters.WorkArea;
            this.Left = (screenSize.Width / 2) - (windowSize.Width / 2);
            this.Top = (screenSize.Height / 2) - (windowSize.Height / 2);
        }
    }

    // Add Key Binding for the Hotkey (Ctrl+Shift+C)
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        // Create a key binding for Ctrl+Shift+C to trigger the CopyAllMenuItem_Click
        var copyAllCommand = new RoutedCommand();
        copyAllCommand.InputGestures.Add(new System.Windows.Input.KeyGesture(System.Windows.Input.Key.C, System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift));

        this.CommandBindings.Add(new CommandBinding(copyAllCommand, CopyAllMenuItem_Click));

        // Assign the command to the menu item
        var copyAllMenuItem = (MenuItem)this.FindName("Copy All");
        // Alternatively, you can set the Command property in XAML
    }

    // Event for the form loaded
    private void OnFormLoaded(object sender, RoutedEventArgs e)
    {
        StartTranscription();
    }

    // Event for the form closing
    private void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Confirm before exiting (optional)
        var result = CustomMessageBoxHelper.Show(this, "Are you sure you want to exit?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            StopTranscription();

            // Save the last window position to the registry
            RegistryHelper.WriteAppInfo("WINDOWLEFT", this.Left.ToString());
            RegistryHelper.WriteAppInfo("WINDOWTOP", this.Top.ToString());
        }
        else
            e.Cancel = true;
    }

    // Exit Menu Item Click Event Handler
    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    // Updated Event handler for Toggle Topmost MenuItem
    private void ToggleTopmostMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            // Toggle the Topmost property based on the IsChecked state of the menu item
            this.Topmost = menuItem.IsChecked;
        }
    }

    // Event Handler for "Copy All" Menu Item
    private void CopyAllMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get all text from txtTranscribed
            string allText = txtTranscribed.Text;

            // Copy to clipboard
            Clipboard.SetText(allText);

            CustomMessageBoxHelper.Show(
                    this,
                    "All transcribed text has been copied to the clipboard.",
                    "Copy Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
        }
        catch (Exception ex)
        {
            // Handle potential errors
            CustomMessageBoxHelper.Show(this, $"Failed to copy text to clipboard: {ex.Message}", "Copy Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ConfigurationMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // Instantiate the ConfigurationWindow
        ConfigurationWindow configWindow = new ConfigurationWindow();

        // Set the owner to the main window
        configWindow.Owner = this;

        // Stop Transcription
        StopTranscription();

        // Show the ConfigurationWindow as a dialog
        bool? result = configWindow.ShowDialog();

        // Restart Transcription
        StartTranscription();
    }

    // Event handler for the Summarize menu item
    private void SummarizeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SummarizeMenuItem.IsEnabled = false;

        var apiendpoint = RegistryHelper.ReadAppInfo("APIENDPOINT");
        var apikey = RegistryHelper.ReadAppInfo("APIKEY");
        var deployment = RegistryHelper.ReadAppInfo("DEPLOYMENT");

        if (string.IsNullOrEmpty(apiendpoint) || string.IsNullOrEmpty(apikey) || string.IsNullOrEmpty(deployment))
        {
            CustomMessageBoxHelper.Show(this, "API Endpoint, Key, and Deployment must be set in configuration.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var textTosummarize = txtTranscribed.Text;

        Task.Run(() =>
        {
            var result = AITextSummarizer.SummarizeText(apiendpoint, apikey, deployment, textTosummarize);
            Dispatcher.Invoke(() =>
            {
                txtTranscribed.AppendText($"\n\n<SUMMARY>\n{result}\n</SUMMARY>\n");
                lastTranscribedSpeakerId = string.Empty;
                txtTranscribed.ScrollToEnd();

                SummarizeMenuItem.IsEnabled = true;
            });
        });
    }

    // Event handler for the Ask AI menu item
    private void AskAIMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // Add code here to open the AskAiWindow
        AskAiWindow askAiWindow = new AskAiWindow();
        askAiWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        askAiWindow.Owner = this; // Set the owner to the main window
        askAiWindow.Show();
    }

    // Start Transcription
    private static bool transcriptionStarted = false;
    private void StartTranscription()
    {
        if (transcriptionStarted)
            return;

        var configured = RegistryHelper.ReadAppInfo("CONFIGURED");
        if (string.IsNullOrEmpty(configured))
        {
            CustomMessageBoxHelper.Show(this, "Please configure the application before using it.", "Configuration Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Load the speechKey and speechRegion from the registry
        speechKey = RegistryHelper.ReadAppInfo("SPEECHKEY");
        speechRegion = RegistryHelper.ReadAppInfo("SPEECHREGION");

        // Check if the environment variables are set
        if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
        {
            CustomMessageBoxHelper.Show(this, "Speech Key and Region must be set in environment variables.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Initialize the SpeechProcessor
        speechProcessor = new SpeechProcessor(speechKey, speechRegion);

        // Define the delegate handlers 
        SpeechProcessor.TranscribingHandler onTranscribing = (text, speakerId) =>
        {
            // Use Dispatcher to update UI elements from a non-UI thread
            Dispatcher.Invoke(() =>
            {
                lock (transcribingDict)
                {
                    // remove from the dictionary anything that ends with unnown
                    foreach (var key in transcribingDict.Keys.ToList())
                        if (key.EndsWith("Unknown"))
                            transcribingDict.Remove(key);

                    var dataTuple = new Tuple<string, DateTime>(text, DateTime.Now);

                    // Find the speaker ID in the list, or add it if not found add
                    if (!transcribingDict.ContainsKey(speakerId))
                        transcribingDict.Add(speakerId, dataTuple);
                    else
                        transcribingDict[speakerId] = dataTuple;

                    // Update the UI with the transcribing text
                    txtTranscribing.Clear();
                    foreach (var kvp in transcribingDict)
                        txtTranscribing.AppendText($"[{kvp.Key}]: {kvp.Value.Item1}\n");

                    txtTranscribing.ScrollToEnd();
                }
            });
        };

        SpeechProcessor.TranscribedHandler onTranscribed = (text, speakerId, isMatch) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (isMatch && text != null && speakerId != null)
                {
                    // Add the transcribed text to the transcribed list
                    if (speakerId == lastTranscribedSpeakerId)
                    {
                        // Same speaker: append text without speaker ID
                        txtTranscribed.AppendText($"{text}\n");
                    }
                    else
                    {
                        // New speaker: add speaker ID and text
                        txtTranscribed.AppendText($"\n{speakerId}  {DateTime.Now:t}\n{text}\n");
                        lastTranscribedSpeakerId = speakerId;
                    }

                    // Scroll to the end of the transcribed text
                    txtTranscribed.ScrollToEnd();

                    // Remove from the dictionary where the key is speakerId or the value.item2 is older than 5 seconds
                    lock (transcribingDict)
                        foreach (var key in transcribingDict.Keys.ToList())
                            if (key == speakerId || transcribingDict[key].Item2 < DateTime.Now.AddSeconds(-5))
                                transcribingDict.Remove(key);

                    // Clear the transcribing text
                    txtTranscribing.Clear();
                }
                else
                {
                    txtTranscribing.AppendText("No match: Speech could not be transcribed.\n");
                    txtTranscribing.ScrollToEnd();
                }
            });
        };

        // Canceled Handler
        SpeechProcessor.CanceledHandler onCanceled = (message) =>
        {
            Logger.Log(message);
            Dispatcher.Invoke(async () =>
            {
                txtTranscribing.AppendText($"[Canceled]: {message}\n");
                txtTranscribing.ScrollToEnd();

                await Task.Delay(2000);
                StartTranscription();

            });
        };

        // Ignore SessionStopped
        SpeechProcessor.SessionStoppedHandler onSessionStopped = (message) =>
            {
            };

        try
        {
            // Start both FromMic and FromSpeaker awaiting on them

            Task micTask = speechProcessor.FromMic(
                onTranscribing,
                onTranscribed,
                onCanceled,
                onSessionStopped
            );

            Task speakerTask = speechProcessor.FromSpeaker(
                onTranscribing,
                onTranscribed,
                onCanceled,
                onSessionStopped
            );

            transcriptionStarted = true;

            // Start Up Message
            txtTranscribing.AppendText("Transcription started.\n");
            txtTranscribing.ScrollToEnd();

        }

        catch (Exception ex)
        {
            CustomMessageBoxHelper.Show(this, $"An error occurred during transcription: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Stop Transcription
    private void StopTranscription()
    {
        if (speechProcessor != null)
            speechProcessor.StopRecognition();

        transcriptionStarted = false;
    }

    // Method to get the transcribed text
    public string GetTranscribedText()
    {
        return txtTranscribed.Text;
    }
}