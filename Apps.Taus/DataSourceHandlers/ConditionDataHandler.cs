using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Taus.DataSourceHandlers
{
    public class ConditionDataHandler : IStaticDataSourceHandler
    {
        protected Dictionary<string, string> EnumValues => new()
        {
            { ">", "Score is above threshold" },
            { ">=", "Score is above or equal threshold" },
            { "=", "Score is same as threshold" },
            { "<", "Score is below threshold" },
            { "<=", "Score is below or equal threshold" }
        };

        public Dictionary<string, string> GetData()
        {
            return EnumValues;
        }
    }
}
