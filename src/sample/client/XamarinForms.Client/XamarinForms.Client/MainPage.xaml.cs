using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using B2CAuthClient;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceTokenClient;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinForms.Client.Authentication.Interface;
using XamarinForms.Client.Cache;
using XamarinForms.Client.Model;

namespace XamarinForms.Client
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly IB2CAuthService _authService;
        private readonly ICosmosTokenClient _cosmosTokenClient;

        private IUserContext _userContext;

        public MainPage()
        {
            const string defaultNameSpace = "XamarinForms.Client";

            var assembly = typeof(App).GetTypeInfo().Assembly;
            
            // Loading the file that have been marked as an Embedded Resource 
            var fileStream = assembly.GetManifestResourceStream($"{defaultNameSpace}.msalconfig.json");

            if (!(fileStream is null))
            {
                using var reader = new StreamReader(fileStream);
                var jsonStr = reader.ReadToEnd();

                var configObject = JObject.Parse(jsonStr);

                var b2cHostName = configObject["B2CHostName"].ToString();
                var tenantId = configObject["TenantId"].ToString();
                var clientId = configObject["ClientId"].ToString();
                var signUpSignInFlowName = configObject["SignUpSignInFlowName"].ToString();
                var resourceTokenBrokerUrl = configObject["ResourceTokenBrokerUrl"].ToString();

                var scopes = ((JArray)configObject["Scopes"])?.Select(scope => scope?.ToString());

                var iOSChainGroup = "com.microsoft.adalcache";

                Func<bool> isAndroidDeviceFunc = () => DeviceInfo.Platform == DevicePlatform.Android;

                Func<bool> isiOSDeviceFunc = () => DeviceInfo.Platform == DevicePlatform.iOS
                                                   || DeviceInfo.Platform == DevicePlatform.watchOS
                                                   || DeviceInfo.Platform == DevicePlatform.tvOS;

                Func<object> getAndroidParentWindowFunc = () =>
                    DependencyService.Get<IParentWindowLocatorService>().GetCurrentParentWindow();

                _authService = new B2CAuthService(
                    b2cHostName,
                    tenantId,
                    clientId,
                    signUpSignInFlowName,
                    scopes,
                    iOSChainGroup,
                    isAndroidDeviceFunc,
                    isiOSDeviceFunc,
                    getAndroidParentWindowFunc);

                var quickAndDirtyPermissionsTokenCache = new QuickAndDirtyCache();

                _cosmosTokenClient = new CosmosTokenClient(_authService, resourceTokenBrokerUrl, quickAndDirtyPermissionsTokenCache);
            }

            InitializeComponent();
        }

        private async void Button_OnSignIn(object sender, EventArgs e)
        {
            _userContext = await _authService.SignIn();
            
            SaveButton.IsEnabled = true;
        }

        private async void Button_SignOut(object sender, EventArgs e)
        {
            await _authService.SignOut();
        }

        private async void Button_Save(object sender, EventArgs e)
        {
            try
            {
                var person = new Person(FirstName.Text, LastName.Text);
                await _cosmosTokenClient.Replace($"Person-{_userContext.UserIdentifier}", person, DefaultPartitionKind.UserDocument);
            }
            catch (Exception ex)
            {
                
            }
        }

        private async void CreateButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                var person = new Person(FirstName.Text, LastName.Text);
                await _cosmosTokenClient.Create($"Person-{Guid.NewGuid()}-{_userContext.UserIdentifier}", person, DefaultPartitionKind.UserDocument);
            }
            catch (Exception ex)
            {

            }
        }

        private async void Button_ReadShared(object sender, EventArgs e)
        {
            try
            {
                var person = new Person(FirstName.Text, LastName.Text);
                var shared = await _cosmosTokenClient.Read<Person>($"Person", DefaultPartitionKind.Shared);

                SharedFirstName.Text = shared.FirstName;
                SharedLastName.Text = shared.LastName;
            }
            catch (Exception ex)
            {

            }
        }

        private async void GetDocuments_OnClicked(object sender, EventArgs e)
        {
            try
            {
                var documents = await _cosmosTokenClient.GetPartitionDocuments(DefaultPartitionKind.UserDocument);
            }
            catch (Exception exception)
            {

            }
        }

    }
}
