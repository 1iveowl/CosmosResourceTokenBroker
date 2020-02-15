using Android.Runtime;
using Plugin.CurrentActivity;
using XamarinForms.Client.Authentication.Interface;


namespace MBaaSMigration.Droid
{
    [Preserve(AllMembers = true)]
    class AndroidParentWindowLocatorService : IParentWindowLocatorService
    {
        public object GetCurrentParentWindow()
        {
            return CrossCurrentActivity.Current.Activity;
        }
    }
}