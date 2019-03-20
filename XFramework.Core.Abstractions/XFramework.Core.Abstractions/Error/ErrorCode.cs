using System.ComponentModel;

namespace XFramework.Core.Abstractions.Error
{
    /// <summary>
    /// 框架的错误代号枚举
    /// </summary>
    public enum ErrorCode
    {

        /// <summary>
        /// 成功使用的业务代码
        /// </summary>
        [Description("成功")]
        Success = 100,

        [Description("无效的请求")]
        InvalidRequest = -100,

        [Description("无效的URL")]
        InvalidRequestUrl = -110,

        [Description("操作超时")]
        Timeout = -120,

        [Description("无效的HTTP响应")]
        InvalidHttpResponse = -130,

        [Description("无效的URI")]
        InvalidUri = -140,

        [Description("无效的参数")]
        InvalidParameter = -150,

        [Description("X服务初始化失败")]
        XServiceInitFailure = -200,

        [Description("X服务无效")]
        InvalidXServiceHandler = -210,

        [Description("无效的SOA服务名称")]
        InvalidSoaServiceId = -300,

        [Description("无效的SOA操作")]
        InvalidSoaOperation = -310,

        [Description("无效的SOA监听器")]
        InvalidSoaListener = -320,

        [Description("Soa自注册失败")]
        SoaRegistionFailed = -330,

        [Description("无效的配置文件名")]
        InvalidConfigurationFile = -400,

        [Description("错误的配置文件版本信息")]
        InvalidConfigurationVersion = -410,

        [Description("错误的配置项")]
        InvalidConfigurationItem = -420,

        [Description("配置文件没有发生改变")]
        ConfigurationNotChanged = -430,

        [Description("配置文件修改失败")]
        ConfiurationModifyFailure = -440,

        [Description("无效的AppId")]
        InvalidAppId = -500,

        [Description("App没有有效的资源信息")]
        AppResourceUnavilable = -510,

        [Description("App实例Id不存在")]
        InvalidInstanceId = -520,

        [Description("初始化Dal客户端失败")]
        InitDalClientFailed = -600,

        [Description("Dal指定的数据库名称错误")]
        InvalidDalDatabaseName = -600,
        
        [Description("不支持的数据库类型")]
        UnsupportedDatabaseType = -610,

        [Description("无效的实体类型")]
        InvalidDalEntity = -620,

        [Description("没有有效的SqlBuilder")]
        SqlBuilderNotFound = -630,

        [Description("主键不能为空")]
        PrimaryKeyIsEmpty = -640,

        [Description("主键错误")]
        InvalidPrimaryKey = -641,

        [Description("无效的Sql参数")]
        InvalidSqlParameters = -642,
        
        /// <summary>
        /// 内部错误，未知错误时采用次代码
        /// </summary>
        [Description("内部错误")]
        InternalError = -9999
    }
}
