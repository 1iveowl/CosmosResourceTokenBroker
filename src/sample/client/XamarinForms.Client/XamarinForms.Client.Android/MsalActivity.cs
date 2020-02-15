using Android.App;
using Android.Content;
using Android.Runtime;
using Microsoft.Identity.Client;

namespace XamarinForms.Client.Droid
{
    [Preserve(AllMembers = true)]
    [Activity]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataHost = "auth",
        DataScheme = "msal9239cf10-0007-4159-9a8b-c6a97ecd9edc")]
    public class MsalActivity : BrowserTabActivity
    {

    }
}