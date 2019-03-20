using System;
using System.Collections.Generic;
using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Contract.Configuration.Query
{
    public class ConfigurationQueryResponseType : SoaResponseType
    {
        public long Version { get; set; }

        public string File { get; set; }

        public List<ConfigurationItemType> Properties { get; set; }

        public DateTime LastModifyDate { get; set; }
    }

    public class ConfigurationItemType
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
