using System;
using System.Data;
using System.Data.OracleClient;
using System.Text;

namespace KF.ORM.SQL
{
    internal class Oracle : ISql
    {
        public IDbConnection CreateConnection(string connectionString)
        {
#pragma warning disable 618
            return new OracleConnection(connectionString);
#pragma warning restore 618
        }

        public IDbDataParameter CreateParameter()
        {
            return new OracleParameter();
        }

        /// <summary>
        /// 数据库命名参数符号
        /// </summary>
        /// <returns></returns>
        public string Dbparmchar()
        {
            return ":";
        }

        public string Delete(dynamic p)
        {
            if (p.GetType().GetProperty("Where") != null && !string.IsNullOrEmpty(p.Where))
                return string.Format("DELETE FROM {0} WHERE {1}", p.TableName, p.Where);
            return string.Format("DELETE FROM {0}", p.TableName);
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
            throw new NotImplementedException();
        }

        public string Insert(dynamic p)
        {
            StringBuilder columns = new StringBuilder(), values = new StringBuilder();
            for (int i = 0; i < p.DbDataParameters.Count; i++)
            {
                columns.Append(p.DbDataParameters[i].ParameterName);
                values.Append(Dbparmchar()).Append(p.DbDataParameters[i].ParameterName);
                if (i < p.DbDataParameters.Count - 1)
                {
                    columns.Append(",");
                    values.Append(",");
                }
            }
            return string.Format("INSERT INTO {0}({1}) VALUES({2});{3}", p.TableName, columns, values, Identity());
        }

        public string Select(dynamic p)
        {
            if (p.GetType().GetProperty("PageSize") != null && p.PageSize > 0)
            {
                if (p.GetType().GetProperty("Where") != null && !string.IsNullOrEmpty(p.Where))
                    return string.Format("SELECT * FROM (SELECT A.*, ROWNUM RN FROM (SELECT * FROM {0}) A WHERE ROWNUM <= {1}) WHERE RN > {2} AND {3}", p.TableName, p.PageSize * p.CurrentPageIndex, p.PageSize * p.CurrentPageIndex - 1, p.Where);
                return string.Format("SELECT * FROM (SELECT A.*, ROWNUM RN FROM (SELECT * FROM {0}) A WHERE ROWNUM <= {1}) WHERE RN > {2}", p.TableName, p.PageSize * p.CurrentPageIndex, p.PageSize * p.CurrentPageIndex - 1);
            }
            else
            {
                if (p.GetType().GetProperty("Where") != null && !string.IsNullOrEmpty(p.Where))
                    return string.Format("SELECT {0} FROM {1} WHERE {2}", p.Select, p.TableName, p.Where);
                return string.Format("SELECT {0} FROM {1}", p.Select, p.TableName);
            }
        }

        public string Truncate(dynamic p)
        {
            return string.Format("TRUNCATE TABLE {0}", p.TableName);
        }

        public string Update(dynamic p)
        {
            StringBuilder sets = new StringBuilder();
            for (int i = 0; i < p.DbDataParameters.Count; i++)
            {
                sets.AppendFormat("{0}={1}{0}", p.DbDataParameters[i].ParameterName, Dbparmchar());
                if (i < p.DbDataParameters.Count - 1)
                {
                    sets.Append(",");
                }
            }
            return string.Format("UPDATE {0} SET {1} WHERE {2}", p.TableName, sets, p.Where);
        }
    }
}