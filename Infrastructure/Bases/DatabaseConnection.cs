using System.Data;

namespace KF.ORM.Infrastructure.Bases
{
    /// <summary>
    /// 数据库连接对象
    /// </summary>
    internal class DatabaseConnection
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType databaseType = DatabaseType.Sqlserver;

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public IDbConnection dbConnection;
    }
}