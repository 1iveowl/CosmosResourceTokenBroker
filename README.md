# Cosmos Resource Token Broker - Guide and Library

<!-- [![NuGet Badge](https://buildstats.info/nuget/Plugin.SegmentedControl.Netstandard)](https://www.nuget.org/packages/Plugin.SegmentedControl.Netstandard/) -->

***Please star this repository if you find it useful. Thank you!***

## Why This Repository

I recently recently created a Xamarin Forms app for [iOS](https://apps.apple.com/us/app/1iveowl-expenses/id1457905006) and [Android](https://play.google.com/store/apps/details?id=com.x1iveowl.myeasyexpense). Early on in the development process I made a [blue pill discision](https://en.wikipedia.org/wiki/Red_pill_and_blue_pill), choosing to lean on the preview edition of [AppCenter Auth](https://docs.microsoft.com/en-us/appcenter/auth/#how-auth-works) and [AppCenter Data](https://docs.microsoft.com/en-us/appcenter/data/).

Faith would have it, that Microsoft recently decided to change direction and [retire AppCenter Auth and Data](https://devblogs.microsoft.com/appcenter/app-center-mbaas-retirement/). With the introduction of this change, I would sooner or later run out of blue pills. Of course, there's good to be said about red pill too, and faced with this change I decided to plunged myself deeper into the underlying technologies of [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/overview), [MSAL](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview) and [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction), embarking on the path of removing my dependencies of AppCenter Auth and Data.

Now, I've always enjoyed learning new stuff, and as I dove in here, I found that the shift was kind enough to offered even more learning opportunities, than first anticipated. Thus, I decided to document my learning and share my insigts as well as the code I've created in the process, in the hope that others would find this helpful.

I believe that what I'm sharing here is both relevant those migrating off of AppCenter Auth and AppCenter Data, as well as for those starting out creating an all new apps utilizing Azure AD B2C, MSAL and Azure Cosmos DB.

## About This Library

This repository presents:

- A guide for configuring and using the forementioned technologies in a mobile app type scenario.
- A code library for implementing a Resource Token Broker Service for Azure Cosmos DB.
- A set of samples help make sense of it all.

The samples demonstrate how to run the Resource Token Broker service as an Azure Function. There is also a sample of a Xamarin Forms app for iOS and Android, which uses the broker to access Cosmos data in a secure maner utilizing Azure AD B2C and MSAL for authentication.

The library and code is written using C# 8.0 and the examples are all based on the latest technology available at the time of writting, specifically:

- [.NET Core 3.1](https://devblogs.microsoft.com/dotnet/announcing-net-core-3-1/)/[.NET Standard 2.1](https://devblogs.microsoft.com/dotnet/announcing-net-standard-2-1/).
- [Azure Cosmos SDK v3.6](https://azure.microsoft.com/en-us/blog/new-for-developers-azure-cosmos-db-net-sdk-v3-now-available/).
- [Microsoft Identity Client v4.8.x / MSAL](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/msal-net-4). 

Where existing documentation or guides are available this guide will point to those.

The guide presented here assumes a basic understanding of Azure Functions and of how to deploy them. It also assumes a basic understanding of .NET Core, Azure and using Visual Studio or Visual Studio Code.

# Getting Started

Before going into the details, I suggest a quick read-through the official Microsoft documentation explaining the basics of what a Resource Token Broker is, and why it's needed: [Secure access to data in Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/secure-access-to-data). 

*Note: There is a link to a similar github repository in this document, however I found the code there somewhat outdated and insufficient for the scope presented here. Still, if you are limited to earlier versions of .NET rather than .NET Core, then you might find it useful.*

Now let's go through the steps one by one.

## Starting Afresh (important)

If you are ***not*** migrating from AppCenter Auth and AppCenter Data then you'll first need to do this:

- [Create an Azure Active Directory B2C tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant), or use one you've created earlier.
- [Create a new Cosmos DB](https://docs.microsoft.com/bs-latn-ba/azure/cosmos-db/create-cosmosdb-resources-portal), or use one you've created earlier.
- Prepare your Xamarin mobile app to use with [MSAL](https://github.com/Azure-Samples/active-directory-xamarin-native-v2).


## Migrating From AppCenter Auth and AppCenter Data

If you are migrating an existing mobile away from AppCenter Auth and AppCenter Data, then you already have a B2C Active Directory Tenant running with existing users. You also already have a Cosmos DB with existing data.

## The steps:

There are the steps needed no matter if migration away from AppCenter Auth and AppCenter Data or not:

1. Configure the Azure AD B2C Tenant. Specifically, three API's/scopes must be created and exposed and added with API Permissions. Details for how to do this is outlined below.
2. Implement a Resource Token Broker and configure it so that it operates seamlessly togehter with your Azure AD B2C and your Cosmos DB.
3. Program your app to use MSAL for authentication.
4. Program your app to store data with Cosmos DB.
5. *(Optional)* You might also want to consider implementing client/app side caching of the data/Cosmos documents.

## Step 1: Configuring the Azure AD B2C Tenant
The Resource Token Broker basically works by mapping [OAuth 2.0 scopes](https://docs.microsoft.com/en-us/azure/active-directory-b2c/authorization-code-flow) to a set of *Cosmos DB users* and then assigning Permissions to each of those *Cosmos DB Users*. A permissions is furthermore mapped between each *Cosmos DB User* and a specific *Cosmos DB Partition Key*. 

Please note, that the *Cosmos DB user* is a different entity from the *Azure AD B2C User*. The *Cosmos DB Users* are created dynamically by the broker, the first time an *Azure AD B2C User* requests a Resource Token. Each *Azure AD B2C User* will be mapped to these *Cosmos DB users* by the broker. Specfically, an *Azure AD B2C* user will upon request receive three Resource Tokens from the broker, with each Resource Token represeting a unique *Cosmos DB User*, which again map to a unique permission - i.e:

1. A Cosmos DB User with **read-only** permission to a specific Partition Key for the specific Azure AD B2C users. The Partition Key look like this: `user-037af674-890b-4f86-a3ef-90e69d585311`
2. A Cosmos DB User with **read and write** permission to a specific Partition Key for the specific Azure AD B2C users. The Partition Key look like this: `user-037af674-890b-4f86-a3ef-90e69d585311`
3. A Cosmos DB User with **read-only** permission to documents shared among all users: The Partition key will look like this: `shared`

The Guid part of the Partition Keys (`user-037af674-890b-4f86-a3ef-90e69d585311`) is the unique and immutable user object id for a user provided by Azure AD B2C.

To configure Azure AD B2C to accommodate this setup we will need to create three scopes to represent:
- User documents: read-only
- User documents: read and write
- Share documents: read-only

To do this:
1. navigate to the Azure AD B2C and the **App Registraion (preview)** menu. 
2. Open your Application.
3. Choose **Expose an API**.
4. (optional) You can change the App ID URI by pressing **edit**.
5. USe the **Add a scope** button to create three scopes with these names:
   1.  `user.readonly`
   3.  `user.readwrite`
   4.  `shared.readonly`
6. Now navigate to the **API Permission** in the menu.
7. Click **Add a permission**,
8. Pick **My APIs.** and stay within **Delegated Permissions**
9. Click on the name of your application
10. Check the three new Permission you just created. You might need to expand the them to click all the check boxes.

That's it. Azure AD B2C is now configured.

## Step 2: Implementing The Resource Token Broker

Used with this step:

- [The Resource Token Broker Service library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/CosmosResourceTokenBroker)
- [The Azure Function sample based on the Resource Token Broker Service library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/broker/AzureFunction).


    *Note: The Resource Token Broker can also be run as an ASP.NET Core app, running in an App Service or a container etc. The Resource Token Broker Service library here can easy accommodate such scenarios too, however the details for setting this up is outside the scope of this guide, although the essential steps should not be that much different.*

To implement the Resource Token Broker we need to:

1. Create the HTTP Triggered Azure Function
2. Configure the Azure Function for integration with Azure AD B2C.
3. Testing integration between Azure Function and Azure AD B2C

### Resource Token Broker - Azure Function

The Resource Token Broker can be implemented as a HTTP Triggered Azure Function. The broker receivces resource token requests from authenticated users to which it hands out tokens providing permissions to only those documents in the Cosmos DB that are owned by the specific user (read/write) or are shared among all users (read-only).

Specifically, the Resource Token Broker provides the user with a set of resource access tokens that give read-write permission to documents which are stored with a Partition Keys called `user-[Unique User Id]`, where the unique id of the user is provided by Azure AD B2C, as well as read-only access to documents stores with the partition key `shared`.

In the [sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/broker/AzureFunction), the settings needed to configure the Resource Token Broker are defined as Azure Function Application Settings that are read when the [function is instantiated](https://github.com/1iveowl/CosmosResourceTokenBroker/blob/6f043ceb5c436e131f32d76256ab6caa508ec4f5/src/sample/broker/AzureFunction.Broker/CosmosResourceTokenBroker.cs#L26). 

When running your Azure Function in your emulator on your local developer machine those settings are read from the file `local.settings.json` in your project. This file will look something like this: 

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    
    "CosmosUrl": "https://[My cosmos db name].documents.azure.com:443/",
    "CosmosKey": "[Secret primary or secondary key...]",
    "CosmosCollectionId": "[Collection name]",
    "CosmosDatabaseId": "[Database name]"
  }
}
```

Please note, that this file will not look like this and might even be missing when you first open the Azure Function sample. This is because 'local.settings.json' is excluded by default by git, to protect developers from inadvertedly sharing secrets. You will therefore need to fill out these details yourself, and do so according to your configuration of Azure Cosmos DB and Azure AD B2C. The file `local.settings.tutorial.json` is included to provide an example. You need to rename it to `local.settings.json` if you want to use it.

### Integrate the Azure Function With Azure AD B2C

When you publish your Azure Function to production, you must configure the same settings that you've are spedifying in `local.settings.json` by using [Azure Function Application Settings](https://medium.com/awesome-azure/azure-reading-application-settings-in-azure-functions-asp-net-core-1dea56cf67cf). As you do this, I strongly advice that you place your secrets (i.e. your Cosmos Primiary or Secondary Key) in the an [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/). There's a great step-by-step guide for how this do this here: [Create Azure Key Vault and Azure Function App](https://daniel-krzyczkowski.github.io/Integrate-Key-Vault-Secrets-With-Azure-Functions/).

You will also need to integrate Azure Functions with Azure AD B2C. This way Azure Functions will *auto-magically* take care of the authentication of the user for you. There's a great step-by-step guide for configuring an Azure Function for integration with Azure AD B2C here: [Secure Azure Functions Using Azure AD B2C](https://medium.com/@ravindraa/secure-azure-functions-using-azure-ad-b2c-986e4ad07c6c). 

### Testing Integration Between Azure Function and Azure AD B2C

You should now have a Resource Token Broker running nicely as an Azure Function. Any access to the Azure Function will now requires AD authentication - i.e. a JWT Access Token is presented in the Authentication Header when accessing the Azure Function.  

You can test that your Azure Function/Azure AD B2C integration has been successfully configure by copying the URL of your Azure Function into a browser which is running in Incognito/InPrivate Browsing mode. If the integration works as it intended, you will be asked to log-in, according to the configuration of your Azure AD B2C sign-in User Flow, before gaining access to the Azure Function. This is just a test of course. Going forward the user will not, and should not, be prompted to log-in everytime the broker is needed, rather you will be configuring your app to handle user log-in using MSAL, and then utilize the Access Token acquired with MSAL when requesting a Resource Tokens from the Resource Token Broker.

## Step 3: Program your app to use MSAL for authentication

Used with this step:

- [B2CAuthClient Library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/B2CAuthClient).
- [Xamarin Forms Sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/client/XamarinForms.Client).

To make it easier to get started, this repository offers a simple wrapper, specifically created for MSAL and Azure Ad B2C Authentication: [B2CAuthClient Library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/B2CAuthClient). You don't need to use this, and it's really only a few hundred lines of code. It does however provide functionality that you also need below in the next step, and thus migth offer a good starting point.

To use MSAL

1. Configure 
2. Create the HTTP Triggered Azure Function
3. Configure the Azure Function for integration with Azure AD B2C.
4. Testing integration between Azure Function and Azure AD B2C

## Step 4: Program your app to store data with Cosmos DB

This repository offers the [Cosmos Resource Token Client library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/CosmosResourceTokenClient) and this [sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/client/XamarinForms.Client).


## Step 5 (optional): Caching

 This will support off-line scenarios, speed things up and to make your app less chatty on the internet. This all depends on the type of app of cause. 

AppCenter Data did offers some of this caching, primarily for the off-line scenarios. However, no such caching is offered here, but it might be worth taking a look at [Akavache](https://github.com/reactiveui/Akavache).