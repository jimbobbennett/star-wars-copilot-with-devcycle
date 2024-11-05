# Star Wars Copilot powered by Pieces and DevCycle

This repo contains a Star Wars copilot that uses [Pieces for Developers](https://pieces.app) to provide an abstraction over an LLM, and [DevCycle](https://devcycle.com) to configure the system prompt to select between different Star Wars Characters.

This is an example of how you can use DevCycle to configure a LLM-powered app. Although this is a fun example using Star Wars characters, this shows the principle you can use to do a test rollout of app updates that change the underlying model, or adjust the system prompt. For example - if you want to validate with a small group of users that changing from GPT-4o to Gemini provides a better experience, or if updates to the system prompt help give better responses.

## How the app works

This is a C# app, using the [Pieces.Extensions.AI](https://www.nuget.org/packages/Pieces.Extensions.AI) nuget package. This is a wrapper around the Pieces REST API, so you will need to install [Pieces for Developers](https://pieces.app) and have this running locally.

This app connects to DevCycle using the [DevCycle .NET SDK](https://www.nuget.org/packages/DevCycle.SDK.Server.Local) nuget package to determine 2 things:

- What model to use
- What Star Wars character to base the copilot on

The model value is taken verbatim - so you can send any supported model name and the app will work. For the character, the copilot chat is configured to be either R2-D2 or Yoda - no other value is supported.

The model is used to configure which underlying LLM Pieces will use. The character is used to set the name of the copilot chat (visible if you look at the conversation in another component of Pieces, such as the VS Code extension or desktop app), and set a system prompt to guide the copilot to answer like the selected character.

## Configuring the app

To configure the app, you need to do the following:

1. Create a new DevCycle experiment
1. Add 2 variables to the experiment:
    - `model` - with variations that match models supported by Pieces, for example `Claude 3.5 sonnet` and `Llama-3 8B`
    - `character` - with variations of `r2-d2` and `yoda`
1. Get a DevCycle server SDK key
1. Copy the `appsettings.json.example` file in the `src` folder to `appsettings.json`, and set the value of `SDKKey` to your SDK key
1. Build and run the app.

## Use the app

To use the app, enter your question just like any other LLM powered chat tool, and you will get a response. Like all chat tools, you can ask follow up questions and the conversation will be based around the entire message history.

To change the model or character, update the targetting rules in DevCycle, and restart the app.
