using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.Requests
{
    public class BlackbirdRequest
    {
        public string Source { get; set; }
        public string SourceLanguage { get; set; }
        public string Target { get; set; }
        public string TargetLanguage { get; set; }
    }
}
