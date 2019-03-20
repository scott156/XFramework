namespace XFramework.Dal.Orm
{
    public interface IDatabaseParameter
    {
        string Operation { get; set; }
        string Name { get; set; }
        object Value { get; set; }
    }
}
