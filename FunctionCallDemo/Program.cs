﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure.Identity;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string endpoint = config["AZURE_OPENAI_ENDPOINT"]!;
string deployment = config["AZURE_OPENAI_GPT_NAME"]!;

// IChatClient chatClient =
//     new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
//         .AsChatClient(deployment);

// IChatClient client = new ChatClientBuilder(chatClient)
//         .UseFunctionInvocation()
//         .Build();

IChatClient client =
    new ChatClientBuilder()
        .UseFunctionInvocation()
        .Use(
            new AzureOpenAIClient(new Uri(endpoint),
            new DefaultAzureCredential())
                .AsChatClient(deployment));

var chatOptions = new ChatOptions
{
    Tools = [AIFunctionFactory.Create((string location, string unit) =>
    {
        // Here you would call a weather API
        // to get the weather for the location.
        return "Periods of rain or drizzle, 15 C";
    },
    "get_current_weather",
    "Get the current weather in a given location")]
};

// System prompt to provide context.
List<ChatMessage> chatHistory = [new(ChatRole.System, """
    You are a hiking enthusiast who helps people discover fun hikes in their area. You are upbeat and friendly.
    """)];

// Weather conversation relevant to the registered function.
chatHistory.Add(new ChatMessage(ChatRole.User,
    "I live in Montreal and I'm looking for a moderate intensity hike. What's the current weather like?"));
Console.WriteLine($"{chatHistory.Last().Role} >>> {chatHistory.Last()}");

ChatResponse response = await client.GetResponseAsync(chatHistory, chatOptions);
Console.WriteLine($"Assistant >>> {response.Text}");