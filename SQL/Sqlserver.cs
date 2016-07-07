using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace KF.ORM.SQL
{
    internal class Sqlserver : ISql
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public IDbDataParameter CreateParameter()
        {
            return new SqlParameter();
        }

        /// <summary>
        /// 数据库命名参数符号
        /// </summary>
        /// <returns></returns>
        public string Dbparmchar()
        {
            return "@";
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public string Delete(dynamic p)
        {
            if (p.GetType().GetProperty("Where") != null && !string.IsNullOrEmpty(p.Where))
                return string.Format("DELETE FROM {0} WHERE {1}", p.TableName, p.Where);
            return string.Format("DELETE FROM {0}", p.TableName);
        }

        /// <summary>
        /// 获取指定数据表中所有字段名
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public string GetColumns(dynamic p)
        {
            return string.Format("SELECT * FROM SYSCOLUMNS WHERE id=OBJECT_ID('{0}')", p.TableName);
        }

        /// <summary>
        /// 获取指定数据库中所有表名
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public string GetTables(dynamic p)
        {
            return "SELECT name FROM SYS.TABLES";
        }

        /// <summary>
        /// 获取Id
        /// </summary>
        /// <returns></returns>
        public string Identity()
        {
            return "SELECT SCOPE_IDENTITY() AS AutoId";
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
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
                    return string.Format("SELECT TOP {0} {1} FROM(SELECT ROW_NUMBER() OVER(ORDER BY {2}) AS ROWNUMBER,* FROM {3} WHERE {4}) A WHERE ROWNUMBER > {5} * {0}", p.PageSize, p.Select, p.PrimaryKeyColumn, p.TableName, p.Where, p.CurrentPageIndex - 1);
                return string.Format("SELECT TOP {0} {1} FROM(SELECT ROW_NUMBER() OVER(ORDER BY {2}) AS ROWNUMBER,* FROM {3}) A WHERE ROWNUMBER > {4} * {0}", p.PageSize, p.Select, p.PrimaryKeyColumn, p.TableName, p.CurrentPageIndex - 1);
            }
            else
            {
                if (p.GetType().GetProperty("Where") != null && !string.IsNullOrEmpty(p.Where))
                    return string.Format("SELECT {0} FROM {1} WHERE {2}", p.Select, p.TableName, p.Where);
                return string.Format("SELECT {0} FROM {1}", p.Select, p.TableName);
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public string Truncate(dynamic p)
        {
            return string.Format("TRUNCATE TABLE {0}", p.TableName);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
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