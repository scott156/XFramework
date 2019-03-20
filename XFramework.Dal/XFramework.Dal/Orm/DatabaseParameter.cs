namespace XFramework.Dal.Orm
{
    public class DatabaseParameter : IDatabaseParameter
    {
        public string Name { get; set; }
        public string Operation { get; set; } = "=";
        public object Value { get; set; }

        public DatabaseParameter()
        {

        }

        public DatabaseParameter(string name, object value, string operation = "=")
        {
            Name = name;
            Value = value;
            Operation = operation;
        }
    }
}
