// AskAiWindow.xaml.cs
// This file defines the AskAiWindow class, which is a WPF window that allows users to interact with an AI model.
// The window includes a text box for user input and a button to send the input to the AI model.
// The AI model processes the input and returns a response, which is displayed in a text box.

using System.Windows;

namespace SpeechTranscriberUI
{
    /// <summary>
    /// Interaction logic for AskAiWindow.xaml
    /// </summary>
    public partial class AskAiWindow : Window
    {
        public AskAiWindow()
        {
            InitializeComponent();

            // Set focus to promt field
            txtPrompt.Focus();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            var apiendpoint = RegistryHelper.ReadAppInfo("APIENDPOINT");
            var apikey = RegistryHelper.ReadAppInfo("APIKEY");
            var deployment = RegistryHelper.ReadAppInfo("DEPLOYMENT");

            if (string.IsNullOrEmpty(apiendpoint) || string.IsNullOrEmpty(apikey) || string.IsNullOrEmpty(deployment))
            {
                CustomMessageBoxHelper.Show(this, "API Endpoint, Key, and Deployment must be set in configuration.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // invoke the method GetTranscribedText in the parent form and store the result on a string variable
            var transcribedText = ((MainWindow)Owner).GetTranscribedText();

            if (string.IsNullOrEmpty(transcribedText))
            {
                CustomMessageBoxHelper.Show(this, "No transcribed text available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var prompt = txtPrompt.Text;
            var context = txtCompletion.Text;

            btnSend.IsEnabled = false;

            Task.Run(() =>
            {
                var result = AIGenericPrompt.Prompt(apiendpoint, apikey, deployment, transcribedText, prompt, context);
                Dispatcher.Invoke(() =>
                {
                    txtCompletion.AppendText($"{result}\n\n");
                    txtCompletion.ScrollToEnd();
                    btnSend.IsEnabled = true;
                });
            });
        }

        private void btnClearCtx_Click(object sender, RoutedEventArgs e)
        {
            txtCompletion.Clear();
        }
    }
}
