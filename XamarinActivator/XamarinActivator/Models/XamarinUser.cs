using Newtonsoft.Json;

namespace XamarinActivator.Models
{
    internal class XamarinUser
    {
        [JsonProperty("Company")]
        public string Company;

        [JsonProperty("Email")]
        public string Email;

        [JsonProperty("FirstName")]
        public string FirstName;

        [JsonProperty("Guid")]
        public string Guid;

        [JsonProperty("LastName")]
        public string LastName;

        [JsonProperty("Nickname")]
        public string Nickname;

        [JsonProperty("OrganizationName")]
        public string OrganizationName;

        [JsonProperty("ParentAccountGuid")]
        public string ParentAccountGuid;

        [JsonProperty("PhoneNumber")]
        public string PhoneNumber;

        [JsonProperty("TrackingGuid")]
        public string TrackingGuid;

        [JsonProperty("TwoLetterCountry")]
        public string TwoLetterCountry;
    }
}
