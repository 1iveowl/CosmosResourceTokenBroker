# Cosmos Resource Token Broker - Guide and Library

<!-- [![NuGet Badge](https://buildstats.info/nuget/Plugin.SegmentedControl.Netstandard)](https://www.nuget.org/packages/Plugin.SegmentedControl.Netstandard/) -->

***Please star this repository if you find it useful. Thank you!***

## Why this repository?

I recently created a Xamarin Forms app for [iOS](https://apps.apple.com/us/app/1iveowl-expenses/id1457905006) and [Android](https://play.google.com/store/apps/details?id=com.x1iveowl.myeasyexpense). Early on in the development process, I made a [blue pill discision](https://en.wikipedia.org/wiki/Red_pill_and_blue_pill), choosing to lean on the preview edition of [AppCenter Auth](https://docs.microsoft.com/en-us/appcenter/auth/#how-auth-works) and [AppCenter Data](https://docs.microsoft.com/en-us/appcenter/data/).

Fast forward a few months and Microsoft decided to change direction. Instead of graduating AppCenter Auth and AppCenter Data, from preview to final release, [both will instead be retired](https://devblogs.microsoft.com/appcenter/app-center-mbaas-retirement/). 

In other words, I would soon run out of blue pills. Of course, there's good to be said about red pills too, so I quickly plunged myself deeper into the underlying technologies that have been supporting AppCenter Auth and Data from the beginning - i.e. 
- [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/overview).
- [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction).
- [MSAL](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview).


I've always enjoyed learning new stuff, and as I dove in, I found that the migration efforts was kind enough to offer even more learning opportunities than first anticipated. Thus, I decided to document my learning and share my insigts, as well as the code that I've created in the process here. I share this, in the hope that others might find this helpful and useful in their own learning/migration journey too.

Also, I believe that what I'm sharing here in this repository, could also relevant for those starting out with apps using Azure AD B2C, MSAL and Azure Cosmos DB, without ever having used AppCenter Auth or AppCenter Data.

## About this library

This repository presents:

- A guide for configuring and using the aforementioned technologies in a mobile app type scenario.
- A code library for implementing a Resource Token Broker Service for Azure Cosmos DB.
- A set of samples to help make sense of it all.

The samples demonstrate how to run the Resource Token Broker service as an Azure Function. There is also a sample of a Xamarin Forms app for iOS and Android, which uses the broker to access Cosmos data in a secure maner utilizing Azure AD B2C and MSAL for authentication.

The library and code is written using C# 8.0 and the examples are all based on the latest technology available at the time of writting, specifically:

- [.NET Core 3.1](https://devblogs.microsoft.com/dotnet/announcing-net-core-3-1/)/[.NET Standard 2.1](https://devblogs.microsoft.com/dotnet/announcing-net-standard-2-1/).
- [Azure Cosmos SDK v3.6](https://azure.microsoft.com/en-us/blog/new-for-developers-azure-cosmos-db-net-sdk-v3-now-available/).
- [Microsoft Identity Client v4.8.x / MSAL](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/msal-net-4). 

Where existing documentation or guides are available this guide will point to those.

The guide presented here assumes a basic understanding of Azure Functions and of how to deploy them. It also assumes a basic understanding of .NET Core, Azure and using Visual Studio or Visual Studio Code.

# Getting started

Before diving into the details, I suggest a quick read-through of the official Microsoft documentation, explaining the basics of what a Resource Token Broker is, and why you need it: [Secure access to data in Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/secure-access-to-data). 

*Note: There is a link to a similar github repository in this document, however I found the code there somewhat outdated and insufficient for the scope presented here. Still, if you are limited to earlier versions of .NET rather than .NET Core, then you might find it useful.*

## Important if starting from afresh

If you are ***not*** migrating from AppCenter Auth and AppCenter Data then you'll first need to do this:

- [Create an Azure Active Directory B2C tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant), or use one you've created earlier.
- [Configure a Xamarin mobile application using Microsoft identity platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-access-web-apis).
- [Create a new Cosmos DB](https://docs.microsoft.com/bs-latn-ba/azure/cosmos-db/create-cosmosdb-resources-portal), or use one you've created earlier.

## Migrating from AppCenter Auth and AppCenter Data

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

## Step 2: Implementing the Resource Token Broker

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

When running your Azure Function in your emulator on your local developer machine those settings are read from the file `local.settings.json` in your project. Please note, that this file will be missing when you first open the Azure Function sample. This is because `local.settings.json` is excluded by default by git, to protect developers from inadvertedly sharing secrets. You will therefore need to fill out these details yourself, and do so according to your configuration of Azure Cosmos DB and Azure AD B2C. The file 

`local.settings.tutorial.json` is included to provide an example. You need to rename it to `local.settings.json` and then fill in the right values. When you have done that, `local.settings.json` will look something like this:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",    
    "CosmosUrl": "https://mycosmosdb.documents.azure.com:443/",
    "CosmosKey": "SDFKIEWRewrmwerKEWRKEWRmwermweRKWERMwerWERMwerMEWRMEWROweROWErKWER56KSDFWEERTMfdsfdfdA==",
    "CosmosCollectionId": "mycollection",
    "CosmosDatabaseId": "mydatabase"
  }
}
```

### Integrate the Azure Function with Azure AD B2C

When you publish your Azure Function to production, you must configure the same settings that you've are spedifying in `local.settings.json` by using [Azure Function Application Settings](https://medium.com/awesome-azure/azure-reading-application-settings-in-azure-functions-asp-net-core-1dea56cf67cf). As you do this, I strongly advice that you place your secrets (i.e. your Cosmos Primiary or Secondary Key) in the an [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/). There's a great step-by-step guide for how this do this here: [Create Azure Key Vault and Azure Function App](https://daniel-krzyczkowski.github.io/Integrate-Key-Vault-Secrets-With-Azure-Functions/).

You will also need to integrate Azure Functions with Azure AD B2C. This way Azure Functions will *auto-magically* take care of the authentication of the user for you. There's a great step-by-step guide for configuring an Azure Function for integration with Azure AD B2C here: [Secure Azure Functions Using Azure AD B2C](https://medium.com/@ravindraa/secure-azure-functions-using-azure-ad-b2c-986e4ad07c6c). 

### Testing integration between Azure Function and Azure AD B2C

You should now have a Resource Token Broker running nicely as an Azure Function. Any access to the Azure Function will now requires AD authentication - i.e. a JWT Access Token is presented in the Authentication Header when accessing the Azure Function.  

You can test that your Azure Function/Azure AD B2C integration has been successfully configure by copying the URL of your Azure Function into a browser which is running in Incognito/InPrivate Browsing mode. If the integration works as it intended, you will be asked to log-in, according to the configuration of your Azure AD B2C sign-in User Flow, before gaining access to the Azure Function. This is just a test of course. Going forward the user will not, and should not, be prompted to log-in everytime the broker is needed, rather you will be configuring your app to handle user log-in using MSAL, and then utilize the Access Token acquired with MSAL when requesting a Resource Tokens from the Resource Token Broker.

## Step 3: Program your app to use MSAL for authentication

Used with this step:

- [B2CAuthClient library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/B2CAuthClient).
- [Xamarin Forms Sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/client/XamarinForms.Client).

To make it easier to get started, this repository offers a simple wrapper, specifically created for MSAL and Azure Ad B2C Authentication: [B2CAuthClient Library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/B2CAuthClient). You don't need to use it, and it's really only a few hundred lines of code. It does however provide functionality that you also need below in the next step, and thus migth offer a good starting point.

The [Xamarin Forms sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/client/XamarinForms.Client) in this repository is based on this official sample from Microsoft: [Integrate Azure AD B2C into a Xamarin forms app using MSAL)][(https://github.com/Azure-Samples/active-directory-b2c-xamarin-native)).

To make it easier for you, to use MSAL in a Azure AD B2C scenario, the sample in this repository collects the MSAL app configuration that you need one file: `msalconfig.json`. This file will be missing when you first open the sample. Instead rename the file `msalconfig.tutorial.json` to `msalconfig.json` and insert the correct values. When you've renamed `msalconfig.tutorial.json` to `msalconfig.json` and have inserted the right values, `msalconfig.json` will end up looking something like this:

```json
{
  "ResourceTokenBrokerUrl": "https://myresourcebroker.azurewebsites.net/api/broker?code=RTDSKEROasdfjoeqrPSDASADNOwlfTMEDSKFSDFSSDrtrtrt455SFg==",
  "B2CHostName": "myaddomain.b2clogin.com",
  "TenantId": "myaddomain.onmicrosoft.com",
  "ClientId": "16408b56-7bf5-4bd7-b68b-dfe1137f60a4",
  "SignUpSignInFlowName": "B2C_1_flowname",
  "Scopes": [
    "https://myaddomain.onmicrosoft.com/myapiuriname/user.readonly",
    "https://myaddomain.onmicrosoft.com/myapiuriname/user.readwrite",
    "https://myaddomain.onmicrosoft.com/myapiuriname/shared.readonly"
  ]
}
```

*Note: `msalconfig.json` is made an Embedded Resource and is read by the Xamarin Forms app like [this](https://github.com/1iveowl/CosmosResourceTokenBroker/blob/8d2dd01316a8e99a1aef9ab26a70354087403237/src/sample/client/XamarinForms.Client/XamarinForms.Client/MainPage.xaml.cs#L37):*

Of cource, you don't have to follow this pattern of using `msalconfig.json` file in your own app to specify the configuration. Any way of providing these settings is just fine. In the official sample from Microsoft the configuration is specified as Constants like [this](https://github.com/Azure-Samples/active-directory-b2c-xamarin-native/blob/master/UserDetailsClient/UserDetailsClient.Core/Features/LogOn/B2CConstants.cs): 

Besides providing the MSAL configuration, you also need to make some changes specific to iOS and Android platforms:
- [Configure the iOS project with your app's return URI](https://github.com/Azure-Samples/active-directory-b2c-xamarin-native#optional-step-6a-configure-the-ios-project-with-your-apps-return-uri).
- [Configure the Android project with your app's return URI](https://github.com/Azure-Samples/active-directory-b2c-xamarin-native#optional-step-6b-configure-the-android-project-with-your-apps-return-uri)
- Create an implemnation of the `IParentWindowLocatorService` for Android like [this](https://github.com/1iveowl/CosmosResourceTokenBroker/blob/master/src/sample/client/XamarinForms.Client/XamarinForms.Client.Android/AndroidParentWindowLocatorService.cs)

If you want to test MSAL with the iOS iPhone Simulator, it is my experience that you must make sure that `CodesignEntitlements` is specified for your `Debug|iPhoneSimulator` platforms settings in your iOS `.csproj` file. At the time of writing, this setting is not there by default. Further more, I've seen this setting get removed by Visual Studio silently when opening the properties of your iOS project from within Visual Studio?! So looking out for this might save you a wild goose chase, if things suddenly stop working for no apparent reason.

```xml
...
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|iPhoneSimulator' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\iPhoneSimulator\Debug</OutputPath>
    <DefineConstants>DEBUG</DefineConstants>
    <ErrorReport>prompt</ErrorReport>

    <!-- If this is missing, then add it: -->
    <CodesignEntitlements>Entitlements.plist</CodesignEntitlements>

    <WarningLevel>4</WarningLevel>
    <MtouchArch>x86_64</MtouchArch>
    <MtouchLink>None</MtouchLink>
    <MtouchDebug>true</MtouchDebug>
  </PropertyGroup>
  ...

```

Also, `CodesignEntitlements` should include this setting in the `keychain-access-groups`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>keychain-access-groups</key>
  <array>
    <string>$(AppIdentifierPrefix)com.microsoft.adalcache</string>
  </array>
</dict>
</plist>
```

When this configuration is in order you are ready to create an instance of the `B2CAuthService` like this:

```csharp
var iOSChainGroup = "com.microsoft.adalcache";

Func<bool> isAndroidDeviceFunc = () => DeviceInfo.Platform == DevicePlatform.Android;

Func<bool> isiOSDeviceFunc = () => DeviceInfo.Platform == DevicePlatform.iOS
                    || DeviceInfo.Platform == DevicePlatform.watchOS
                    || DeviceInfo.Platform == DevicePlatform.tvOS;

Func<object> getAndroidParentWindowFunc = () =>
                    DependencyService.Get<IParentWindowLocatorService>().GetCurrentParentWindow();

IB2CAuthService authService = new B2CAuthService(
                    b2cHostName,
                    tenantId,
                    clientId,
                    signUpSignInFlowName,
                    scopes,
                    iOSChainGroup,
                    isAndroidDeviceFunc,
                    isiOSDeviceFunc,
                    getAndroidParentWindowFunc);
```

B2CAuthService has a simple interface and is easy to use:

```csharp

public interface IB2CAuthService
{
    IUserContext CurrentUserContext { get; }

    Task<IUserContext> SignIn(IEnumerable<string> scopes = null, bool silentlyOnly = false, CancellationToken cancellationToken = default);

    Task SignOut(CancellationToken cancellationToken = default);
}

public interface IUserContext
{
    bool IsLoggedOn { get; }
    bool IsNewUser { get; }

    string AccessToken { get;}
    DateTime AccessTokenExpires { get; }
    bool HasAccessTokenExpired { get; }

    string UserIdentifier { get; }
    string Name { get; }
    string Scope { get; }
    string GivenName { get;}
    string FamilyName { get;}
    string Province { get; }
    string PostalCode { get; }
    string Country { get; } 
    string JobTitle { get; }
    string StreetAddress { get; }
    string City { get; } 
    IEnumerable<string> EmailAddresses { get; }   
}
```

## Step 4: Program your app to store data with Cosmos DB

Used with this step:

- [B2CAuthClient library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/B2CAuthClient).
- [Cosmos Resource Token Client library](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/main/CosmosResourceTokenClient) and this [sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/client/XamarinForms.Client).
- [Xamarin Forms Sample](https://github.com/1iveowl/CosmosResourceTokenBroker/tree/master/src/sample/client/XamarinForms.Client).

By now your Resource Token Broker and your Xamarin Forms app should be all configured.

To start reading and writing to Cosmos DB all you need to do is to instantiate the Cosmos Token Client:

```csharp
ICosmosTokenClient cosmosTokenClient = new CosmosTokenClient(authService, resourceBrokerUrl);
```

`CosmosTokenClient` uses Dependency Injection. Hence, if you don't like, or for some reason can't use, the `B2CAuthService` provided in this repository then you can still use the CosmosTokenClient. All you need to do is implement the simple `IB2CAuthService`  interfaces and passed your own version of the B2CAuthService as the first parameter in the constructor.

The `CosmosTokenClient` will try to acquire the UserContext and specifically the Access Token from the `B2CAuthService` instance passed in with the contructor. If the user is not already logged in (i.e. CurrentUserContext is null), the `CosmosTokenClient` will try to acquire the User Context silently from the MSAL Client cache, - i.e. without presenting the user with the option to log in. If it's not possible for `CosmosTokenClient` to acquire the User Context this way either then it will throw: `CosmosClientAuthenticationException`. In other words, the approach used here is that it is best to leave it entirely up to the app developer, to define and handle the logic for when and how to present the user with an interactive login.

Also, note that `CosmosTokenClient` has a third optional parameter which accepts an instance of a class with the `ICacheSingleObjectByKey` interface. It is strongly advice that you use this, as it allows the CosmosTokenClient to cache permission requests on a per user basis. There's a quick and dirty, yet fully functional, implementation of this caching interface included as part of the Xamarin Forms sample, you'll find it [here](https://github.com/1iveowl/CosmosResourceTokenBroker/blob/master/src/sample/client/XamarinForms.Client/XamarinForms.Client/Cache/QuickAndDirtyCache.cs).

The `ICosmosTokenClient` interface is very similar to [AppCenter Data](https://docs.microsoft.com/en-us/appcenter/sdk/data/xamarin). This is no coincidence, as this approach leaves for an easy migration away from AppCenter Data, now that it is being retired.

For example Creating a document with AppCenter data looksed like this:

```csharp
await Data.CreateAsync(id, personObj, DefaultPartitions.UserDocuments, new WriteOptions(deviceTimeToLive));
```

Using the CosmosTokenClient the same create operation will look like this:
```csharp
await cosmosTokenClient.Create(id, personObj, DefaultPartitionKind.UserDocument);
```

The AppCenter Data enum `DefaultPartitions` have two values
- UserDocuments
- AppDocuments

Similary, the CosmosTokenClient uses the `DefaultPartitionKind` enum with these two possible values:
- UserDocument
- Shared 

There's no `WriteOption` etc. available for CosmosTokenClient, as caching is out of scope (See step 5 below). 

That's it. You've made it this far, and while there's certainly a lot of moving parts and settings that need to align before getting here, when you finally get to this point, reading and writing documents to Cosmos DB using the security provided by the Resource Token Broker, is actually pretty straight forward. 😊

## Step 5 (optional): Caching

One more thing. App are all different, but more often than not, an app will benefit from caching documents reads, and also sometimes document writes. Such caching offers off-line support, speed things up and to make your app less chatty on the internet. It all depends on the type of app of cause. 

AppCenter Data did offers some of this caching, primarily for the off-line scenarios. However, no such caching is offered here, but it might be worth taking a look at [Akavache](https://github.com/reactiveui/Akavache).
