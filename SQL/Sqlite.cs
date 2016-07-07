using System.Data;
using System.Data.SQLite;
using System.Text;

namespace KF.ORM.SQL
{
    /// <summary>
    /// Sqlite数据库的SQL生成对象
    /// </summary>
    internal class Sqlite : ISql
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        public IDbDataParameter CreateParameter()
        {
            return new SQLiteParameter();
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
            return string.Format("PRAGMA TABLE_INFO({0})", p.TableName);
        }

        /// <summary>
        /// 获取指定数据库中所有表名
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public string GetTables(dynamic p)
        {
            return "SELECT name FROM SQLITE_MASTER";
        }

        /// <summary>
        /// 获取Id
        /// </summary>
        /// <returns></returns>
        public string Identity()
        {
            return "SELECT last_insert_rowid()";
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

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public string Select(dynamic p)
        {
            if (p.GetType().GetProperty("PageSize") != null && p.PageSize > 0)
            {
                if (p.GetType().GetProperty("Where") != null && !string.IsNullOrEmpty(p.Where))
                    return string.Format("SELECT {0} FROM {1} WHERE {2} ORDER BY {3} LIMIT {4} OFFSET {5}", p.Select, p.TableName, p.Where, p.PrimaryKeyColumn, p.PageSize, p.PageSize * (p.CurrentPageIndex - 1));
                return string.Format("SELECT {0} FROM {1} ORDER BY {2} LIMIT {3} OFFSET {4}", p.Select, p.TableName, p.PrimaryKeyColumn, p.PageSize, p.PageSize * (p.CurrentPageIndex - 1));
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
            return string.Format("DELETE FROM {0};UPDATE sqlite_sequence SET seq = 0 WHERE name ='{0}';VACUUM;", p.TableName);
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