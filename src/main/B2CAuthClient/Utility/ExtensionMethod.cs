using System;
using System.Text;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

namespace B2CAuthClient.Utility
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

        public static DateTime UnixTimeStampToDateTimeUtc(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
    }
}
