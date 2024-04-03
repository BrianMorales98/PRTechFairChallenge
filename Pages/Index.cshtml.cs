using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure;
using Azure.AI.OpenAI;

namespace RazorTest.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
        public string InputText { get; set; }

        public string ResponseText { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                string filePath = "prompts/SystemPrompt.txt";
                string fileContent = System.IO.File.ReadAllText(filePath);

                String deploymentName = "pr-tech-fair-gpt-35-turbo";
                Uri azureOpenAIResourceUri = new("https://pr-tech-fair-aoai.openai.azure.com/");
                AzureKeyCredential azureOpenAIApiKey = new("");
                OpenAIClient client = new(azureOpenAIResourceUri, azureOpenAIApiKey);


                // Configure search service
                string searchEndpoint = "https://pr-tech-fair-acs.search.windows.net";
                string searchKey = "";
                string searchIndex = "prtechfaircensodataindex";

                AzureSearchChatExtensionConfiguration searchConfig = new()
                {
                    SearchEndpoint = new Uri(searchEndpoint),
                    Authentication = new OnYourDataApiKeyAuthenticationOptions(searchKey),
                    IndexName = searchIndex
                };  
                    
                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    DeploymentName = deploymentName, // Use DeploymentName for "model" with non-Azure clients
                    AzureExtensionsOptions = new AzureChatExtensionsOptions
                    {
                        Extensions = { searchConfig }
                    },
                    Messages =
                    {
                        // The system message represents instructions or other guidance about how the assistant should behave
                        new ChatRequestSystemMessage(fileContent),
                        // User messages represent current or historical input from the end user    
                        new ChatRequestUserMessage(InputText),
                    }
                };

                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatResponseMessage responseMessage = response.Value.Choices[0].Message;
                ResponseText = responseMessage.Content;
            }
            catch (Exception ex)
            {
                ResponseText = ex.Message;
            }

            return Page();
        }
}
