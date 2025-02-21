// ConfigurationWindow.xaml.cs
// This class represents the configuration window for the SpeechTranscriberUI application.
// It allows users to load and save configuration settings related to Azure services.
// The settings include Azure Text to Speech Key, Region, OpenAI Endpoint, API Key, and Model Deployment Name.
// The configuration values are read from and written to the Windows registry.
// The window provides functionality to save the configuration and start the transcription process in the main window.

using System.Windows;

namespace SpeechTranscriberUI;

public partial class ConfigurationWindow : Window
{
    public ConfigurationWindow()
    {
        InitializeComponent();
        LoadConfiguration();
    }

    // Load configuration values from the registry
    private void LoadConfiguration()
    {
        // Init/Load From Registry
        if (string.IsNullOrEmpty(txtSpeechKey.Text = RegistryHelper.ReadAppInfo("SPEECHKEY")))
            txtSpeechKey.Text = "Your Azure Text to Speech Key";

        if (string.IsNullOrEmpty(txtSpeechRegion.Text = RegistryHelper.ReadAppInfo("SPEECHREGION")))
            txtSpeechRegion.Text = "Your Azure Text to Speech Region";

        if (string.IsNullOrEmpty(txtAPIEndPoint.Text = RegistryHelper.ReadAppInfo("APIENDPOINT")))
            txtAPIEndPoint.Text = "Your Azure OpenAI Endpoint";

        if (string.IsNullOrEmpty(txtAPIKey.Text = RegistryHelper.ReadAppInfo("APIKEY")))
            txtAPIKey.Text = "Your Azure OpenAI API Service Key";

        if (string.IsNullOrEmpty(txtDeployment.Text = RegistryHelper.ReadAppInfo("DEPLOYMENT")))
            txtDeployment.Text = "Your Azure OpenAI Model Deployment Name";
    }

    // Save configuration values to the registry
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Save configuration values
        // Save to Registry
        RegistryHelper.WriteAppInfo("SPEECHKEY", txtSpeechKey.Text);
        RegistryHelper.WriteAppInfo("SPEECHREGION", txtSpeechRegion.Text);
        RegistryHelper.WriteAppInfo("APIENDPOINT", txtAPIEndPoint.Text);
        RegistryHelper.WriteAppInfo("APIKEY", txtAPIKey.Text);
        RegistryHelper.WriteAppInfo("DEPLOYMENT", txtDeployment.Text);
        RegistryHelper.WriteAppInfo("CONFIGURED", "YES");

        CustomMessageBoxHelper.Show(this, "Configuration saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        // Set DialogResult to true to indicate success
        this.DialogResult = true;

    }

    // Cancel button click event handler
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Set DialogResult to false to indicate cancellation
        this.DialogResult = false;
        this.Close();
    }
}
