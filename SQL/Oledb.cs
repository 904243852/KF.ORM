using System;
using System.Data;
using System.Data.OleDb;

namespace KF.ORM.SQL
{
    internal class Oledb : ISql
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new OleDbConnection(connectionString);
        }

        public IDbDataParameter CreateParameter()
        {
            return new OleDbParameter();
        }

        public string Dbparmchar()
        {
            return "@";
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

        public string Identity()
        {
            return "SELECT @@IDENTITY AS AutoId";
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
