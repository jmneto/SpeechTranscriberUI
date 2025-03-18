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
            string prompt = $"Create a summary of this conversation below. If possible create a conclusion statement and next steps. \nConversation:\n\n{textToSummarize}";

            ChatCompletion completion = chatClient.CompleteChat(
                        [
                            // User messages represent user input
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

public class AIGenericPrompt
{
    public static string Prompt(
        string apiEndpoint,
        string apiKey,
        string deployment,
        string text,
        string prompt,
        string context)
    {
        try
        {

            // Initialize the .NET OpenAI client
            var client = new AzureOpenAIClient(new Uri(apiEndpoint), new ApiKeyCredential(apiKey));
            var chatClient = client.GetChatClient(deployment);

            // Prepare the prompt for summarization
            string finalPrompt = $"Answer this prompt:[{prompt}]\n\nBase your answer on this conversation:\n\n[{text}]\n\nUse this information as previous context for this AI interaction:\n\n[{context}]";

            ChatCompletion completion = chatClient.CompleteChat(
                        [
                            // User messages represent user input
                            new UserChatMessage(finalPrompt),
                        ]);

            // return the summarized text
            return completion.Content[0].Text;

        }
        catch (RequestFailedException ex)
        {
            // Handle specific Azure OpenAI errors
            return $"An error occurred while generating the response. Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            // Handle other possible errors
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return $"An unexpected error occurred. Unexpected error: {ex.Message}";
        }
    }
}