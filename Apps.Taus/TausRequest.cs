using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus
{
    public class TausRequest : RestRequest
    {
        public TausRequest(string endpoint, Method method, IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(endpoint, method)
        {
            var apiKey = authenticationCredentialsProviders.First(p => p.KeyName == "apiKey").Value;
            this.AddHeader("api-key", apiKey);
        }
    }
}
