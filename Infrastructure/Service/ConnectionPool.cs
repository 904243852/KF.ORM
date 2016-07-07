using KF.ORM.Infrastructure.Bases;
using KF.ORM.SQL;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace KF.ORM.Infrastructure.Service
{
    /// <summary>
    /// 连接对象池
    /// </summary>
    internal class ConnectionPool
    {
        public IDictionary<string, DatabaseConnection> databaseConnections = new Dictionary<string, DatabaseConnection>();

        private IDictionary<DatabaseType, ISql> _sqls = new Dictionary<DatabaseType, ISql>();

        /// <summary>
        /// 初始化连接池对象，获取数据库连接对象
        /// </summary>
        public ConnectionPool()
        {
            foreach (ConnectionStringSettings connectionStringSettings in ConfigurationManager.ConnectionStrings)
            {
                DatabaseConnection databaseConnection = CreateConnection(connectionStringSettings);
                databaseConnections.Add(connectionStringSettings.Name, databaseConnection);
            }
        }

        /// <summary>
        /// 获取SQL构造对象
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        public ISql GetSql(DatabaseType databaseType)
        {
            if (_sqls.ContainsKey(databaseType))
                return _sqls[databaseType];
            else
                throw new Exception("数据库类型目前不支持！无法创建该数据库类型的构造对象！");
        }

        /// <summary>
        /// 根据配置文件中所配置的数据库类型和传入的数据库链接字符串来创建相应数据库连接对象
        /// </summary>
        /// <param name="connectionStringSettings"></param>
        /// <returns></returns>
        private DatabaseConnection CreateConnection(ConnectionStringSettings connectionStringSettings)
        {
            DatabaseConnection databaseConnection = new DatabaseConnection();
            DatabaseType databaseType;
            if (!Enum.TryParse(connectionStringSettings.ProviderName, true, out databaseType))
            {
                //throw new Exception(string.Format("数据库“{0}”的类型“{1}”目前不支持！数据库类型转换失败！", connectionStringSettings.Name, connectionStringSettings.ProviderName));
            }
            databaseConnection.databaseType = databaseType;
            ISql _sql = null;
            if (!_sqls.ContainsKey(databaseConnection.databaseType))
            {
                switch (databaseConnection.databaseType)
                {
                    case DatabaseType.Sqlserver:
                        _sql = new Sqlserver();
                        break;
                    case DatabaseType.Oracle:
                        _sql = new Oracle();
                        break;
                    case DatabaseType.Mysql:
                        _sql = new Mysql();
                        break;
                    case DatabaseType.Oledb:
                        _sql = new Oledb();
                        break;
                    case DatabaseType.Sqlite:
                        _sql = new Sqlite();
                        break;
                    default:
                        throw new Exception("数据库类型目前不支持！无法创建该数据库类型的连接对象！");
                }
                if (_sql != null)
                    _sqls.Add(databaseConnection.databaseType, _sql);
            }
            else
                _sql = _sqls[databaseConnection.databaseType];
            databaseConnection.dbConnection = _sql.CreateConnection(connectionStringSettings.ConnectionString);
            return databaseConnection;
        }

        /// <summary>
        /// 连接至指定名称的数据库
        /// </summary>
        /// <param name="name">连接的数据库名称（配置文件中connectionStrings中配置）</param>
        /// <returns></returns>
        public DatabaseConnection Connect(string name)
        {
            if (databaseConnections.ContainsKey(name))
            {
                return databaseConnections[name];
            }
            throw new Exception(string.Format(".config文件connectionStrings中未配置 {0} 数据库", name));
        }
    }
}