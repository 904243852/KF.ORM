using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace KF.ORM.SQL
{
    internal class Mysql : ISql
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public IDbDataParameter CreateParameter()
        {
            return new MySqlParameter();
        }

        /// <summary>
        /// 数据库命名参数符号
        /// </summary>
        /// <returns></returns>
        public string Dbparmchar()
        {
            return "?";
        }

        public string Delete(dynamic p)
        {
            throw new NotImplementedException();
        }

        public string GetColumns(dynamic p)
        {
            throw new NotImplementedException();
        }

        public string GetTables(dynamic p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取Id
        /// </summary>
        /// <returns></returns>
        public string Identity()
        {
            return "SELECT @@IDENTITY";
        }

        public string Insert(dynamic p)
        {
            throw new NotImplementedException();
        }

        public string Select(dynamic p)
        {
            throw new NotImplementedException();
        }

        public string Truncate(dynamic p)
        {
            throw new NotImplementedException();
        }

        public string Update(dynamic p)
        {
            throw new NotImplementedException();
        }
    }
}