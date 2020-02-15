using System;
using System.Text;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

namespace XamarinForms.Client.Utility
{
    public static class ExtensionMethod
    {
        public static string ToBase64UrlDecoded(this string str)
        {
            str = str.Replace('-', '+').Replace('_', '/');

            str = str.PadRight(str.Length + (4 - str.Length % 4) % 4, '=');

            var byteArray = Convert.FromBase64String(str);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length );
            
            return decoded;
        }

        public static JObject ToJwtPayloadAsJObject(this AuthenticationResult authResult)
        {
            var jwtPayload = authResult.IdToken
                .Split('.')[1]
                .ToBase64UrlDecoded();

            return JObject.Parse(jwtPayload);
        }
    }
}
