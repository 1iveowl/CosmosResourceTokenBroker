using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using CosmosResourceToken.Core.Client;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinForms.Client.Authentication;

namespace XamarinForms.Client
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly IB2CAuthService _authService;

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

                var dataObject = JObject.Parse(jsonStr);

                var b2cHostName = dataObject["B2CHostName"].ToString();
                var tenantId = dataObject["TenantId"].ToString();
                var clientId = dataObject["ClientId"].ToString();
                var signUpSignInFlowName = dataObject["SignUpSignInFlowName"].ToString();

                var scopes = ((JArray)dataObject["Scopes"])?.Select(scope => scope?.ToString());

                _authService = new B2CAuthService(
                    b2cHostName,
                    tenantId,
                    clientId,
                    signUpSignInFlowName,
                    scopes,
                    "com.microsoft.adalcache",
                    DeviceInfo.Platform);
            }

            InitializeComponent();
        }

        private async void Button_OnSignIn(object sender, EventArgs e)
        {
            //var userContext = await _authService.AcquireUserContextForSpecificScope(_scopes.FirstOrDefault());
            var userContect = await _authService.SignIn();
        }

        private async void Button_SignOut(object sender, EventArgs e)
        {
            await _authService.SignOut();
        }
    }
}
