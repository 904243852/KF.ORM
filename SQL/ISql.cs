using System.Data;

namespace KF.ORM.SQL
{
    /// <summary>
    /// SQL构造的接口
    /// </summary>
    internal interface ISql
    {
        #region 数据库操作对象
        /// <summary>
        /// 创建数据库连接对象
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IDbConnection CreateConnection(string connectionString);

        /// <summary>
        /// 创建数据库的参数对象
        /// </summary>
        /// <returns></returns>
        IDbDataParameter CreateParameter();
        #endregion

        #region SQL语句
        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string Insert(dynamic p);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string Delete(dynamic p);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string Update(dynamic p);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string Select(dynamic p);

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string Truncate(dynamic p);

        /// <summary>
        /// 获取Id，如SELECT SCOPE_IDENTITY() AS AutoId
        /// </summary>
        /// <returns></returns>
        string Identity();

        /// <summary>
        /// 获取数据库命名参数符号，比如@(SQLSERVER)、:(ORACLE)
        /// </summary>
        /// <returns></returns>
        string Dbparmchar();

        /// <summary>
        /// 获取指定数据库中所有表名
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string GetTables(dynamic p);

        /// <summary>
        /// 获取指定数据表中所有字段名
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string GetColumns(dynamic p);
        #endregion
    }
}