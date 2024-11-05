# Star Wars Copilot powered by Pieces and DevCycle

This repo contains a Star Wars copilot that uses [Pieces for Developers](https://pieces.app) to provide an abstraction over an LLM, and [DevCycle](https://devcycle.com) to configure the system prompt to select between different Star Wars Characters.

This is an example of how you can use DevCycle to configure a LLM-powered app. Although this is a fun example using Star Wars characters, this shows the principle you can use to do a test rollout of app updates that change the underlying model, or adjust the system prompt. For example - if you want to validate with a small group of users that changing from GPT-4o to Gemini provides a better experience, or if updates to the system prompt help give better responses.

## How the app works

This is a C# app, using the [Pieces.Extensions.AI](https://www.nuget.org/packages/Pieces.Extensions.AI) nuget package. This is a wrapper around the Pieces REST API, so you will need to install [Pieces for Developers](https://pieces.app) and have this running locally.

This app connects to DevCycle using the [DevCycle .NET SDK](https://www.nuget.org/packages/DevCycle.SDK.Server.Local) nuget package to determine 2 things:

- What model to use
- What system prompt to use to define the Star Wars character in use.

The model value is taken verbatim - so you can send any supported model name and the app will work. This is used to select an LLM in the Pieces chat. Same with the system prompt - this is passed as is to each message sent to the LLM.

## Configuring the app

To configure the app, you need to do the following:

1. Create a new DevCycle experiment
1. Add 2 variables to the experiment:
    - `model` - with variations that match models supported by Pieces, for example `Claude 3.5 sonnet` and `Llama-3 8B`
    - `system-prompt` - with variations for different system prompts depending on your Star Wars character of choice. For example, `You are a helpful copilot who will try to answer all questions. Reply in the style of Yoda, including using Yoda's odd sentence structure. Refer to anger being a path to the dark side often, and when referencing context from the user workflow refer to communing with the living force.`
1. Get a DevCycle server SDK key
1. Copy the `appsettings.json.example` file in the `src` folder to `appsettings.json`, and set the value of `SDKKey` to your SDK key
1. Build and run the app.

## Use the app

To use the app, enter your question just like any other LLM powered chat tool, and you will get a response. Like all chat tools, you can ask follow up questions and the conversation will be based around the entire message history.

To change the model or system prompt, update the targeting rules in DevCycle, and restart the app.
