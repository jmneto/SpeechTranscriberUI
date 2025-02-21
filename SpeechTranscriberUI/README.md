# SpeechTranscriberUI

**SpeechTranscriberUI** is a WPF application that leverages Azure Speech Transcription and OpenAI services to convert spoken words into text and generate summaries. With a user-friendly interface, it allows easy configuration of Azure and OpenAI settings, real-time transcription display, and seamless text summarization. Ideal for developers and users looking to integrate advanced speech-to-text and text processing capabilities into their workflows.

## Features

- **Configuration Management**
  - Load and save configuration settings to the Windows registry.
  - Configure Azure Text to Speech Key and Region.
  - Configure Azure OpenAI Endpoint, API Key, and Model Deployment Name.

- **Transcription Process**
  - Start the transcription process directly from the main window.
  - Real-time display of transcribed text.
  - Summarize transcribed text using OpenAI services.

## Prerequisites

- **.NET 9.0 SDK or later**
- **Visual Studio 2022 or later**
- **Azure Subscription**
  - Access to Azure Text to Speech and OpenAI services.

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/yourusername/SpeechTranscriberUI.git
```

### Open the Project

1. Open **Visual Studio 2022**.
2. Navigate to **File > Open > Project/Solution**.
3. Locate the cloned repository folder and select `SpeechTranscriberUI.sln`.

### Build and Run

1. Build the solution by navigating to **Build > Build Solution**.
2. Run the application by selecting **Debug > Start Debugging** or pressing `F5`.

## Configuration

The configuration window allows you to input and save the following settings:

- **Azure Text to Speech Key**: Your Azure Text to Speech Key.
- **Azure Text to Speech Region**: Your Azure Text to Speech Region.
- **Azure OpenAI Endpoint**: Your Azure OpenAI Endpoint.
- **Azure OpenAI API Key**: Your Azure OpenAI API Service Key.
- **Azure OpenAI Model Deployment Name**: Your Azure OpenAI Model Deployment Name.

These settings are saved to the Windows registry and are loaded automatically when the application starts.

## Usage

1. **Open the Application**: Launch `SpeechTranscriberUI`.
2. **Configure Settings**:
   - Navigate to the configuration window.
   - Input your Azure service details.
   - Save the configuration.
3. **Start Transcription**: Initiate the transcription process from the main window.
4. **View Transcription**: The transcribed text will appear in real-time.
5. **Summarize Text** (Optional): Use the summarize feature to get a concise version of the transcribed text.

## Contributing

Contributions are welcome! Please follow these steps:

1. **Fork the Repository**: Click the **Fork** button on the repository page.
2. **Clone Your Fork**:

    ```bash
    git clone https://github.com/yourusername/SpeechTranscriberUI.git
    ```

3. **Create a New Branch**:

    ```bash
    git checkout -b feature/YourFeatureName
    ```

4. **Commit Your Changes**:

    ```bash
    git commit -m "Add your detailed description here"
    ```

5. **Push to Your Fork**:

    ```bash
    git push origin feature/YourFeatureName
    ```

6. **Submit a Pull Request**: Navigate to the original repository and create a pull request.

## License

This project is licensed under the [MIT License](LICENSE). See the [LICENSE](LICENSE) file for details.

## Contact

For any questions or suggestions, please [open an issue](https://github.com/jmneto/SpeechTranscriberUI/issues) on GitHub.