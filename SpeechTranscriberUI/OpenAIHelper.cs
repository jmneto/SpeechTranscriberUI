// OpenAIHelper.cs
// This class provides functionality to summarize text using Azure OpenAI services.
// It initializes the Azure OpenAI client and sends a prompt to the specified deployment model
// to generate a concise summary of the provided text. The summarized text is then returned
// to the caller. Error handling is included to manage specific Azure OpenAI errors and other
// potential exceptions.


using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace SpeechTranscriberUI;

public class AITextSummarizer
{
    /// <summary>
    /// Summarizes the provided text using Azure OpenAI.
    /// </summary>
    /// <param name="apiEndpoint">The Azure OpenAI API endpoint.</param>
    /// <param name="apiKey">The Azure OpenAI API key.</param>
    /// <param name="deployment">The OpenAI model to use (e.g., "text-davinci-003").</param>
    /// <param name="maxTokens">Maximum number of tokens in the response.</param>
    /// <param name="temperature">Sampling temperature.</param>
    /// <param name="textToSummarize">The text that needs to be summarized.</param>
    /// <returns>The summarized text.</returns>
    public static string SummarizeText(
        string apiEndpoint,
        string apiKey,
        string deployment,
        string textToSummarize)
    {
        try
        {

            // Initialize the .NET OpenAI client
            var client = new AzureOpenAIClient(new Uri(apiEndpoint), new ApiKeyCredential(apiKey));
            var chatClient = client.GetChatClient(deployment);

            // Prepare the prompt for summarization
            string prompt = $"Please provide a concise summary for the following text:\n\n{textToSummarize}";

            ChatCompletion completion = chatClient.CompleteChat(
                        [
                            // User messages represent user input, whether historical or the most recent input
                            new UserChatMessage(prompt),
                        ]);

            // return the summarized text
            return completion.Content[0].Text;

        }
        catch (RequestFailedException ex)
        {
            // Handle specific Azure OpenAI errors
            return $"An error occurred while generating the summary. Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            // Handle other possible errors
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return $"An unexpected error occurred. Unexpected error: {ex.Message}";
        }
    }
}