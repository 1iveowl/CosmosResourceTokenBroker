# Cosmos Resource Token Broker - Guide, Tutorial and Library

[![NuGet Badge](https://buildstats.info/nuget/Plugin.SegmentedControl.Netstandard)](https://www.nuget.org/packages/Plugin.SegmentedControl.Netstandard/)

***Please star this repository if you find it useful. Thank you!***

## Why This Repository

I recently recently created a Xamarin Forms app for [iOS](https://apps.apple.com/us/app/1iveowl-expenses/id1457905006) and [Android](https://play.google.com/store/apps/details?id=com.x1iveowl.myeasyexpense). Early on in the development process I made a [blue pill discision](https://en.wikipedia.org/wiki/Red_pill_and_blue_pill), choosing to lean on the preview edition of [AppCenter Auth](https://docs.microsoft.com/en-us/appcenter/auth/#how-auth-works) and [AppCenter Data](https://docs.microsoft.com/en-us/appcenter/data/).

Faith would have it, that Microsoft recently decided to change direction by [retiring AppCenter Auth and Data](https://devblogs.microsoft.com/appcenter/app-center-mbaas-retirement/). With the introduction of this change, I would sooner or later out of blue pills. There's good to be said about red pill as well, and thus I plunged myself deeper into the underlying technologies of [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/overview), [MSAL](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview) and [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) to remove the dependencies of AppCenter Auth and Data.

Now, I've always enjoyed learning new stuff, and as I dove in here, I found that the shift was kind enough to offered even more learning opportunities than first anticipated. Hence, I decided to document my learning path and share my insigts as well as the code I created in the process.

I hope that this reposity will be helpful for other and maybe even save others some time too. 

I believe that what I'm sharing here is both relevant those migrating off of AppCenter Auth and AppCenter Data, as well as for those starting out creating new apps utilizing Azure AD B2C, MSAL and Azure Cosmos DB.

## About This Library

This repository presents:

- A guide and a how-to for configuring the forementioned technologies.
- A code library and (and at sometime possibly also a set of NuGet's) for implementing a Resource Token Broker Service for Azure Cosmos DB.
- A set of samples.

The samples demonstrate how to run the Resource Token Broker service as an Azure Function as well as an example of a Xamarin Forms app for iOS and Android, which uses the broker to access Cosmos data in a secure maner utilizing Azure AD B2C and MSAL for authentication.

The library and code examples are all based on the latest technology available at the time of writting, specifically:

- [.NET Core 3.1](https://devblogs.microsoft.com/dotnet/announcing-net-core-3-1/)/[.NET Standard 2.1](https://devblogs.microsoft.com/dotnet/announcing-net-standard-2-1/).
- [Azure Cosmos SDK v3.6](https://azure.microsoft.com/en-us/blog/new-for-developers-azure-cosmos-db-net-sdk-v3-now-available/).
- [Microsoft Identity Client v4.8.x / MSAL](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/msal-net-4). 

Where existing documentation or guides are available this guide will point to those.

The guide presented here assumes a basic understanding of Azure Functions and of how to deploy them. It also assumes a basic understanding of .NET Core, Azure and using Visual Studio or Visual Studio Code.

## Getting Started

Before going into the details, I suggest a quick read-through the official Microsoft documentation explaining the basics of what a Resource Token Broker is, and why you need it: [Secure access to data in Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/secure-access-to-data). 

*Note: There is a link to a similar github repository in this document, however I found the code there somewhat outdated and insufficient for the scope presented here. Still, if you are limited to earlier versions of .NET rather than .NET Core, then you might find it useful.*

## The Steps

Here are the steps you need to go through to get started.

### Starting Afresh

If you are ***not*** migrating from AppCenter Auth and AppCenter Data then you'll first need to do this:

- [Create an Azure Active Directory B2C tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant), or use one you've already created earlier.
- [Create a new Cosmos DB](https://docs.microsoft.com/bs-latn-ba/azure/cosmos-db/create-cosmosdb-resources-portal), or use one you've already created earlier.
- Prepare your Xamarin mobile app to use with [MSAL](https://github.com/Azure-Samples/active-directory-xamarin-native-v2).


### Migrating From AppCenter Auth and AppCenter Data

If you are migrating an existing mobile app off of AppCenter Auth and AppCenter Data, then you already have a B2C Active Directory Tenant running with existing users. You also already have a Cosmos DB with existing data. 

The steps you need to take to migrate are:

1. Create a Resource Token Broker and configure it so that operates seamlessly with your B2C Active Directory and your Cosmos DB. This repository offers the [Resource Token Broker Service library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/CosmosResourceTokenBroker) and this [sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/broker/AzureFunction.Broker).
2. Program your app to use MSAL for authentication. This repository offers the [B2CAuthClient Library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/B2CAuthClient) and this [sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/client/XamarinForms.Client).
3. Program your app to read and write data to Cosmos DB utilizing the Resource Token Broker to facilitate secure access. This repository offers the [Cosmos Resource Token Client library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/CosmosResourceTokenClient) and this [sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/client/XamarinForms.Client).
4. *[optional]* You might also want to consider implementing client/app side caching of the data/Cosmos documents, to support off-line scenarios, speed things up and to make your app less chatty on the internet. It all depends on the type of app of cause. AppCenter Data did offers some of this caching, primarily for the off-line scenarios. However, no such caching is offered here, but it might be worth taking a look at [Akavache](https://github.com/reactiveui/Akavache).

If your are not migrating from AppCenter Auth and Data, i.e. starting afresh with Azure AD B2C and Azure Cosmos DB, you basically need to go through the same steps.

#### Step 1: Implementing The Resource Token Broker

The first thing you should do, is to create a HTTP Triggered Azure Function that will serve as your Resource Token Broker. 

*Note: You can also run your Resource Token Broker as an ASP.NET Core app, running in an App Service. The Resource Token Broker Service library here can easy accommodate such a scenario, however setting this up and configuring this is outside the scope of this guide, although the essential steps should not be that much different.*

The ingredients for getting the Resource Token Broker ready, running as an Azure Function, are included in this repository:

1.  The code for the [Resource Token Broker Service library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/CosmosResourceTokenBroker).
2.  The [Azure Function sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/broker/AzureFunction.Broker) which shows you how to configure and use the Resource Token Broker Service.

With this you are ready to create your own Azure Function based Resource Token Broker. 

When you've published your Azure Function and configured it as outlined in the just mentioned [guide](https://github.com/1iveowl/CosmosResourceTokenBroker/blob/6f043ceb5c436e131f32d76256ab6caa508ec4f5/src/sample/broker/AzureFunction.Broker/CosmosResourceTokenBroker.cs#L23) then you'll need to configure the Resource Broker Service. 

In the sample, configuration is defined as Azure Function Application Settings that are read when the [function is instantiated](https://github.com/1iveowl/CosmosResourceTokenBroker/blob/6f043ceb5c436e131f32d76256ab6caa508ec4f5/src/sample/broker/AzureFunction.Broker/CosmosResourceTokenBroker.cs#L26). 

When running your Azure Function in your emulator on your local developer machine those settings are read from the file 'local.settings.json' in your project. This file should look something like this: 

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    
    "CosmosUrl": "https://[My cosmos db name].documents.azure.com:443/",
    "CosmosPrimaryKey": "[Secret primary key...]",
    "CosmosCollectionId": "[Collection name]",
    "CosmosDatabaseId": "[Database name]"
  }
}
```

Please note, that this file will not look like this when you first open the Azure Function sample in this repository. Specifically, all the settings that relate to Cosmos missing. This is because 'local.settings.json' is excluded per default by git, to protect developers from inadvertedly sharing secrets. You will therefore need to fill out these details yourself and do so according to your configuration of Azure Cosmos DB and Azure AD B2C. See the included file `local.settings.tutorial.json` as a starting point.

When you publish your Azure Function into production, you must configure the same settings your are spedifying in `local.settings.json`. You do so by utilizing [Azure Function Application Settings](https://medium.com/awesome-azure/azure-reading-application-settings-in-azure-functions-asp-net-core-1dea56cf67cf). As you do this, I strongly advice that you place your secrets in the [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/). There's a great step-by-step guide for how this do this here: [Create Azure Key Vault and Azure Function App](https://daniel-krzyczkowski.github.io/Integrate-Key-Vault-Secrets-With-Azure-Functions/)

Here's a great step-by-step guide for configuring an Azure Function for integration with Azure AD B2C: [Secure Azure Functions Using Azure AD B2C](https://medium.com/@ravindraa/secure-azure-functions-using-azure-ad-b2c-986e4ad07c6c). 

With these steps behind us, you should have a Resource Token Broker running nicely in Azure Functions, and which requires AD authentication of any user accessing it. You can test that your Azure AD B2C configuration is successful by copying the URL of your Azure Function into a browser running in Incognito/InPrivate Browsing mode, and see that you will now be asked to log-in using your favorite social account (according to the configuration of your Azure AD B2C sign-in User Flow flow). 

This is just a test, of course. Going forward the user will not, and should not, be presented with such log-in requirements everytime the broker is needed in the app, rather you will be configuring the app to handle user log-in once, using MSAL, and then utilize an Access Token for the different scopes (read and read-write) to authenticate the user to the Azure Function.

#### Using MSAL With The App
