// SpeechProcessor.cs
// This class handles speech transcription using Microsoft Cognitive Services and NAudio.
// It provides methods to transcribe speech from a microphone or speaker output.
// The class includes event handlers for transcribing, transcribed, canceled, and session stopped events.
// It uses delegates to allow external handling of these events.

using System.IO;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.CognitiveServices.Speech;
using NAudio.Wave;

namespace SpeechTranscriber;
public class SpeechProcessor
{
    // Define delegates for different events with separate parameters
    public delegate void TranscribingHandler(string text, string speakerId);
    public delegate void TranscribedHandler(string? text, string? speakerId, bool isMatch);
    public delegate void CanceledHandler(string message);
    public delegate void SessionStoppedHandler(string message);

    // TaskCompletionSource to manage task completion
    private TaskCompletionSource<int> stopAllRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

    // SpeechConfig instance
    private SpeechConfig? speechConfig = null;

    // Constructor to initialize the speechConfig
    public SpeechProcessor(string key, string region)
    {
        speechConfig = SpeechConfig.FromSubscription(key, region);
        speechConfig.SpeechRecognitionLanguage = "en-US";
        speechConfig.SetProperty(PropertyId.SpeechServiceResponse_DiarizeIntermediateResults, "true");
    }

    // Method to stop recognition externally
    public void StopRecognition()
    {
        stopAllRecognition.TrySetResult(0);
    }

    // FromMic method
    public async Task FromMic(
        TranscribingHandler onTranscribing,
        TranscribedHandler onTranscribed,
        CanceledHandler onCanceled,
        SessionStoppedHandler onSessionStopped)
    {
        if (speechConfig == null)
            throw new InvalidOperationException("SpeechConfig is not initialized.");

        using (var audioConfig = AudioConfig.FromDefaultMicrophoneInput())
        {
            // Create a conversation transcriber using audio stream input
            using (var conversationTranscriber = new ConversationTranscriber(speechConfig, audioConfig))
            {
                // Event handler for transcribing
                conversationTranscriber.Transcribing += (s, e) =>
                {
                    string text = e.Result.Text ?? string.Empty;
                    string speakerId = $"M-{e.Result.SpeakerId}";
                    onTranscribing?.Invoke(text, speakerId);
                };

                // Event handler for transcribed
                conversationTranscriber.Transcribed += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        if (string.IsNullOrEmpty(e.Result.Text))
                            return;

                        string text = e.Result.Text;
                        string speakerId = $"M-{e.Result.SpeakerId}";
                        onTranscribed?.Invoke(text, speakerId, true);
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        onTranscribed?.Invoke(null, null, false);
                    }
                };

                // Event handler for cancellation
                conversationTranscriber.Canceled += (s, e) =>
                {
                    string message = $"CANCELED: Reason={e.Reason}";
                    Logger.Log(message);
                    onCanceled?.Invoke(message);

                    if (e.Reason == CancellationReason.Error)
                    {
                        Logger.Log($"CANCELED: ErrorCode={e.ErrorCode}");
                        Logger.Log($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        onCanceled?.Invoke($"CANCELED: ErrorCode={e.ErrorCode}");
                        onCanceled?.Invoke($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        stopAllRecognition.TrySetResult(0);
                    }

                    stopAllRecognition.TrySetResult(0);
                };


                // Event handler for session stopped
                conversationTranscriber.SessionStopped += (s, e) =>
                {
                    onSessionStopped?.Invoke("Session stopped event.");
                    stopAllRecognition.TrySetResult(0);
                };

                // Start transcribing
                await conversationTranscriber.StartTranscribingAsync();

                // Waits for completion. Use Task.WhenAny to keep the task rooted.
                await Task.WhenAny(stopAllRecognition.Task);

                // Stop transcribing
                await conversationTranscriber.StopTranscribingAsync();
            }
        }
    }

    // From Speaker method
    public async Task FromSpeaker(
        TranscribingHandler onTranscribing,
        TranscribedHandler onTranscribed,
        CanceledHandler onCanceled,
        SessionStoppedHandler onSessionStopped)
    {
        if (speechConfig == null)
            throw new InvalidOperationException("SpeechConfig is not initialized.");

        // Memory Stream for communication between NAudio and Azure
        Stream output = new MemoryStream();

        // NAudio Loopback Capture setup
        var capture = new WasapiLoopbackCapture();
        capture.WaveFormat = new WaveFormat(16000, 16, 1);

        // NAudio Writer setup
        var writer = new WaveFileWriter(output, capture.WaveFormat);

        // read setup
        long lastOutputRead = 0;
        object lockObj = new object();

        // Azure Speech setup
        using var audioConfigStream = AudioInputStream.CreatePushStream();
        using var audioConfig = AudioConfig.FromStreamInput(audioConfigStream);
        using var conversationTranscriber = new ConversationTranscriber(speechConfig, audioConfig);

        // NAudio Capture Events
        capture.DataAvailable += (s, a) =>
        {
            lock (lockObj)
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);

                Console.WriteLine($"Output stream BytesRecorded: {output.Length}");

                if (output.Length > 1024)
                {
                    long currentLength = output.Length;
                    long bytesToRead = currentLength - lastOutputRead;
                    if (bytesToRead > 0)
                    {
                        int bufferSize = 1024;
                        byte[] readBytes = new byte[bufferSize];
                        output.Position = lastOutputRead;
                        int bytesRead;
                        while (bytesToRead > 0 && (bytesRead = output.Read(readBytes, 0, (int)Math.Min(bufferSize, bytesToRead))) > 0)
                        {
                            audioConfigStream.Write(readBytes, bytesRead);
                            lastOutputRead += bytesRead;
                            bytesToRead -= bytesRead;
                        }
                    }
                }
            }
        };

        // Event handler for recording stopped
        capture.RecordingStopped += (s, a) =>
        {
            writer.Dispose();
            writer = null;
            capture.Dispose();
        };


        // Azure Speech Event handler for transcribing
        conversationTranscriber.Transcribing += (s, e) =>
        {
            string text = e.Result.Text ?? string.Empty;
            string speakerId = $"S-{e.Result.SpeakerId}";
            onTranscribing?.Invoke(text, speakerId);
        };

        // Event handler for transcribed
        conversationTranscriber.Transcribed += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                if (string.IsNullOrEmpty(e.Result.Text))
                    return;

                string text = e.Result.Text;
                string speakerId = $"S-{e.Result.SpeakerId}";

                // cleand and reset the output stream
                lock (lockObj)
                {
                    output.SetLength(0);
                    lastOutputRead = 0;
                }

                onTranscribed?.Invoke(text, speakerId, true);
            }
            else if (e.Result.Reason == ResultReason.NoMatch)
            {
                onTranscribed?.Invoke(null, null, false);
            }
        };

        // Event handler for cancellation
        conversationTranscriber.Canceled += (s, e) =>
        {
            string message = $"CANCELED: Reason={e.Reason}";
            onCanceled?.Invoke(message);

            if (e.Reason == CancellationReason.Error)
            {
                onCanceled?.Invoke($"CANCELED: ErrorCode={e.ErrorCode}");
                onCanceled?.Invoke($"CANCELED: ErrorDetails={e.ErrorDetails}");
                stopAllRecognition.TrySetResult(0);
            }

            stopAllRecognition.TrySetResult(0);
        };

        // Event handler for session stopped
        conversationTranscriber.SessionStopped += (s, e) =>
        {
            onSessionStopped?.Invoke("Session stopped event.");
            stopAllRecognition.TrySetResult(0);
        };

        // Start transcribing
        await conversationTranscriber.StartTranscribingAsync();

        capture.StartRecording();

        // Waits for completion. Use Task.WhenAny to keep the task rooted.
        await Task.WhenAny(stopAllRecognition.Task);

        await conversationTranscriber.StopTranscribingAsync();

        // Stop recording
        capture.StopRecording();
    }
}