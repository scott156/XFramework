namespace XFramework.Dal.Orm
{
    public interface IDatabaseParameterLink
    {
        /// <summary>
        /// 上下文关系
        /// </summary>
        DatabaseParameterRelation? Releation { get; set; }

        /// <summary>
        /// 下一个参数
        /// </summary>
        IDatabaseParameterLink Next { get; set; }
    }
}
