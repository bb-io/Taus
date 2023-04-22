using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus
{
    public class TausClient : RestClient
    {
        public TausClient() : base(new RestClientOptions() { ThrowOnAnyError = true, BaseUrl = new Uri("https://api.sandbox.taus.net") }) { }
    }
}
