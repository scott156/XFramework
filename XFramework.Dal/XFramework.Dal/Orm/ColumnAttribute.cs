using System;
using System.Data;

namespace XFramework.Dal.Orm
{
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Identity { get; set; }
        public DbType ColumnType { get; set; }
        public bool PrimaryKey { get; set; }
    }
}
