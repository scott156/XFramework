using System;
using System.Collections.Generic;
using System.Text;

namespace XFramework.Configuration.Client.Data
{
    public class ModifiedPropertyInfo
    {
        public string Key { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// ture : 修改或者删除
        /// false : 新增
        /// </summary>
        public bool Modify { get; set; } = true;
    }
}
