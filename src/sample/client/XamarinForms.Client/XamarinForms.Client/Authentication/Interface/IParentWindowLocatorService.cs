namespace XamarinForms.Client.Authentication.Interface
{
    /// <summary>
    /// Simple platform specific service that is responsible for locating a 
    /// </summary>
    public interface IParentWindowLocatorService
    {
        object GetCurrentParentWindow();
    }
}
