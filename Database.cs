using KF.ORM.Infrastructure.Bases;
using KF.ORM.Infrastructure.Service;
using KF.ORM.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace KF.ORM
{
    /// <summary>
    /// 数据库交互入口
    /// </summary>
    public class Database
    {
        private static ThreadLocal<Database> database = new ThreadLocal<Database>();

        private ConnectionPool connectionPool = new ConnectionPool();

        internal Mapper mapper = new Mapper();

        internal ExpressionConvert expressionConvert = new ExpressionConvert();

        private Database() { }

        /// <summary>
        /// 获取Database实例，若线程中存在，则从线程中取；否则实例化新对象并保存在线程中
        /// </summary>
        /// <returns></returns>
        public static Database Instance()
        {
            if (database.Value == null)
            {
                database.Value = new Database();
            }
            return database.Value;
        }

        #region 获取数据库命令对象IDbCommand
        /// <summary>
        /// 获取数据库命令对象
        /// e.g.
        /// IDbCommand dc = IDbCommand(script, connection, databaseType);
        /// dc.Connection.Open();
        /// ...
        /// dc.Connection.Close();
        /// </summary>
        /// <param name="script">
        /// 要执行的SQL语句(使用ExpandoObject替代dynamic，防止匿名类不能将值传入ISql的方法内)
        /// e.g. dynamic p = new ExpandoObject();
        /// p.Select = "*";
        /// ...
        /// Database.GetSqlBuilder(databaseName).SELECT(p);
        /// </param>
        /// <param name="connection">数据库连接对象，从ConnectionPool中获取</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns>数据库命令对象</returns>
        private IDbCommand DbCommand(string script, IDbConnection connection, DatabaseType databaseType)
        {
            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = script;
            dbCommand.Connection = connection;
            dbCommand.CommandType = CommandType.Text;
#if DEBUG
            if (script != null)
                Debug.LogDbCommand(dbCommand);
#endif
            return dbCommand;
        }
        #endregion

        #region 插入
        /// <summary>
        /// 插入对象数据
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="entity">需要插入的数据对象</param>
        /// <returns>对象的Id</returns>
        public int Insert<T>(T entity) where T : class
        {
            if (entity == null) return 0;
            object val;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                IList<IDbDataParameter> parms = mapper.GetParameters(entity, mapInfo, sql);
                string script = sql.Insert(new { TableName = mapInfo.table.Name, DbDataParameters = parms });
                connection = dc.dbConnection;
                IDbCommand dbCommand = DbCommand(script, connection, dc.databaseType);
                foreach (IDbDataParameter parm in parms)
                    dbCommand.Parameters.Add(parm);
                connection.Open();
                val = dbCommand.ExecuteScalar();
                if (val != null)
                    Reflection.SetPropertyValue(entity, mapInfo.column.Where(s => s.Key.AutoId == true).FirstOrDefault().Value, val);
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return Convert.ToInt32(val);
        }

        /// <summary>
        /// 批量插入对象数据
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="entities">需要插入的数据对象集合</param>
        /// <returns></returns>
        public List<int> Insert<T>(List<T> entities) where T : class
        {
            List<int> ids = new List<int>();
            if (entities == null || entities.Count == 0) return ids;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                connection = dc.dbConnection;
                connection.Open();
                foreach (T entity in entities)
                {
                    IList<IDbDataParameter> parms = mapper.GetParameters(entity, mapInfo, sql);
                    IDbCommand dbCommand = DbCommand(sql.Insert(new { TableName = mapInfo.table.Name, DbDataParameters = parms }), connection, dc.databaseType);
                    foreach (IDbDataParameter parm in parms)
                        dbCommand.Parameters.Add(parm);
                    object val = dbCommand.ExecuteScalar();
                    if (val != null)
                        Reflection.SetPropertyValue(entity, mapInfo.column.Where(s => s.Key.AutoId == true).FirstOrDefault().Value, val);
                    ids.Add(Convert.ToInt32(val));
                }
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return ids;
        }

        /// <summary>
        /// <para>插入对象数据</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().Insert(entity, new Action(() => { Method }));</para>
        /// <para>or.</para>
        /// <para>Database.Instance().Insert(entity, () => { Method });</para>
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="entity">需要插入的数据对象</param>
        /// <param name="onSuccess">插入成功后执行的方法</param>
        /// <param name="onFail">插入失败后执行的方法</param>
        public void Insert<T>(T entity, Action onSuccess, Action onFail = null) where T : class
        {
            if (entity == null) return;
            object val;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                IList<IDbDataParameter> parms = mapper.GetParameters(entity, mapInfo, sql);
                string script = sql.Insert(new { TableName = mapInfo.table.Name, DbDataParameters = parms });
                connection = dc.dbConnection;
                IDbCommand dbCommand = DbCommand(script, connection, dc.databaseType);
                foreach (IDbDataParameter parm in parms)
                    dbCommand.Parameters.Add(parm);
                connection.Open();
                val = dbCommand.ExecuteScalar();
                if (val != null)
                    Reflection.SetPropertyValue(entity, mapInfo.column.Where(s => s.Key.AutoId == true).FirstOrDefault().Value, val);
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            if (Convert.ToInt32(val) > 0 && onSuccess != null)
                onSuccess.DynamicInvoke();
            else
                if (onFail != null) onFail.DynamicInvoke();
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除对象数据
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="entity">需要删除的数据对象</param>
        /// <returns>受影响的行数，若为0，表示删除失败，若大于0，表示删除成功</returns>
        public int Delete<T>(T entity) where T : class
        {
            if (entity == null) return 0;
            int val;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                dynamic parameters = new { TableName = mapInfo.table.Name, Where = string.Join(", ", mapInfo.column.Where(s => s.Key.PrimaryKey == true).Select(s => string.Format("{0}='{1}'", s.Key.Name, Reflection.GetPropertyValue(entity, s.Value.Name)))) };
                connection = dc.dbConnection;
                connection.Open();
                val = DbCommand(sql.Delete(parameters), connection, dc.databaseType).ExecuteNonQuery();
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return val;
        }

        /// <summary>
        /// 批量删除对象数据
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="entities">需要删除的数据对象集合</param>
        /// <returns>累计受影响的行数(所有对象实例集合删除影响的行数的总和)</returns>
        public int Delete<T>(List<T> entities) where T : class
        {
            if (entities == null || entities.Count == 0) return 0;
            int val = 0;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                connection = dc.dbConnection;
                connection.Open();
                foreach (T entity in entities)
                {
                    dynamic parameters = new { TableName = mapInfo.table.Name, Where = string.Join(", ", mapInfo.column.Where(s => s.Key.PrimaryKey == true).Select(s => string.Format("{0}='{1}'", s.Key.Name, Reflection.GetPropertyValue(entity, s.Value.Name)))) };
                    string script = sql.Delete(parameters);
                    val += DbCommand(script, connection, dc.databaseType).ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return val;
        }

        /// <summary>
        /// 根据Lambda表达式删除对象数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="expression">Lambda表达式</param>
        /// <returns>受影响的行数</returns>
        public int Delete<T>(Expression<Func<T, bool>> expression) where T : class
        {
            int val;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                dynamic parameters = new { TableName = mapInfo.table.Name, Where = expressionConvert.Convert(expression) };
                connection = dc.dbConnection;
                connection.Open();
                val = DbCommand(sql.Delete(parameters), connection, dc.databaseType).ExecuteNonQuery();
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return val;
        }
        #endregion

        #region 修改
        /// <summary>
        /// 更新对象数据
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="entity">需要更新的数据对象实例</param>
        /// <returns>受影响的行数，若为0，表示更新失败，若大于0，表示更新成功</returns>
        public int Update<T>(T entity) where T : class
        {
            if (entity == null) return 0;
            int val;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                IList<IDbDataParameter> parms = mapper.GetParameters(entity, mapInfo, sql);
                string script = sql.Update(new { TableName = mapInfo.table.Name, DbDataParameters = parms, Where = string.Join(", ", mapInfo.column.Where(s => s.Key.PrimaryKey == true).Select(s => string.Format("{0}='{1}'", s.Key.Name, Reflection.GetPropertyValue(entity, s.Value.Name)))) });
                connection = dc.dbConnection;
                IDbCommand dbCommand = DbCommand(script, connection, dc.databaseType);
                foreach (IDbDataParameter parm in parms)
                    dbCommand.Parameters.Add(parm);
                connection.Open();
                val = dbCommand.ExecuteNonQuery();
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return val;
        }

        /// <summary>
        /// 批量更新对象数据
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="entities">需要更新的数据对象实例集合</param>
        /// <returns>累计受影响的行数(所有对象实例集合更新影响的行数的总和)，若为0，表示没有数据更新</returns>
        public int Update<T>(List<T> entities) where T : class
        {
            if (entities == null || entities.Count == 0) return 0;
            int val = 0;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                connection = dc.dbConnection;
                connection.Open();
                foreach (T entity in entities)
                {
                    IList<IDbDataParameter> parms = mapper.GetParameters(entity, mapInfo, sql);
                    string script = sql.Update(new { TableName = mapInfo.table.Name, DbDataParameters = parms, Where = string.Join(", ", mapInfo.column.Where(s => s.Key.PrimaryKey == true).Select(s => string.Format("{0}='{1}'", s.Key.Name, Reflection.GetPropertyValue(entity, s.Value.Name)))) });
                    IDbCommand dbCommand = DbCommand(script, connection, dc.databaseType);
                    foreach (IDbDataParameter parm in parms)
                        dbCommand.Parameters.Add(parm);
                    val += dbCommand.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return val;
        }

        /// <summary>
        /// <para>更新对象数据</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().Update(entity, new Action(() => { Method }));</para>
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <param name="entity">需要更新的数据对象集合</param>
        /// <param name="onSuccess">插入成功后执行的方法</param>
        /// <param name="onFail">插入失败后执行的方法</param>
        public void Update<T>(T entity, Action onSuccess, Action onFail = null) where T : class
        {
            if (entity == null) return;
            int val;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                IList<IDbDataParameter> parms = mapper.GetParameters(entity, mapInfo, sql);
                string script = sql.Update(new { TableName = mapInfo.table.Name, DbDataParameters = parms, Where = string.Join(", ", mapInfo.column.Where(s => s.Key.PrimaryKey == true).Select(s => string.Format("{0}='{1}'", s.Key.Name, Reflection.GetPropertyValue(entity, s.Value.Name)))) });
                connection = dc.dbConnection;
                IDbCommand dbCommand = DbCommand(script, connection, dc.databaseType);
                foreach (IDbDataParameter parm in parms)
                    dbCommand.Parameters.Add(parm);
                connection.Open();
                val = dbCommand.ExecuteNonQuery();
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            if (val > 0 && onSuccess != null)
                onSuccess.DynamicInvoke();
            else
                if (onFail != null) onFail.DynamicInvoke();
        }
        #endregion

        #region 查询
        /// <summary>
        /// <para>根据Lambda表达式查询</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().Query&lt;T&gt;(s => s.Id == 1);</para>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="expression">Lambda表达式</param>
        /// <returns></returns>
        public List<T> Query<T>(Expression<Func<T, bool>> expression = null) where T : class, new()
        {
            List<T> list;
            IDataReader sdr = null;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                dynamic parameters = new { Select = "*", TableName = mapInfo.table.Name, Where = expressionConvert.Convert(expression) };
                connection = dc.dbConnection;
                connection.Open();
                sdr = DbCommand(sql.Select(parameters), connection, dc.databaseType).ExecuteReader();
                list = mapper.ToList<T>(sdr, mapInfo);
            }
            finally
            {
                if (sdr != null) sdr.Close();
                if (connection != null) connection.Close();
            }
            return list;
        }

        /// <summary>
        /// <para>根据ICondition查询</para>
        /// <para>e.g.</para>
        /// <para>ICondition&lt;T&gt; condition = new ICondition&lt;T&gt;();</para>
        /// <para>condition.Add(s => s.Id == 1);</para>
        /// <para>Database.Instance().Query&lt;T&gt;(condition);</para>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="condition">条件对象</param>
        /// <returns></returns>
        public List<T> Query<T>(Condition<T> condition) where T : class, new()
        {
            List<T> list;
            IDataReader sdr = null;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                dynamic parameters = new { Select = "*", TableName = mapInfo.table.Name, Where = condition.ToString() };
                connection = dc.dbConnection;
                connection.Open();
                sdr = DbCommand(sql.Select(parameters), connection, dc.databaseType).ExecuteReader();
                list = mapper.ToList<T>(sdr, mapInfo);
            }
            finally
            {
                if (sdr != null) sdr.Close();
                if (connection != null) connection.Close();
            }
            return list;
        }

        /// <summary>
        /// <para>根据Lambda表达式分页查询</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().Query&lt;T&gt;(s => s.Id == 1, PageSize, CurrentPageIndex);</para>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="expression">Lambda表达式</param>
        /// <param name="pageSize"></param>
        /// <param name="currentPageIndex"></param>
        /// <returns></returns>
        public List<T> Query<T>(Expression<Func<T, bool>> expression, int pageSize, int currentPageIndex) where T : class, new()
        {
            List<T> list;
            IDataReader sdr = null;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                string primaryKeyColumn = string.Join(",", mapInfo.column.Where(s => s.Key.PrimaryKey == true).Select(s => s.Key.Name));
                if (string.IsNullOrEmpty(primaryKeyColumn))
                    primaryKeyColumn = mapInfo.column.FirstOrDefault().Key.Name;
                dynamic parameters = new { Select = "*", TableName = mapInfo.table.Name, Where = expressionConvert.Convert(expression), PrimaryKeyColumn = primaryKeyColumn, PageSize = pageSize, CurrentPageIndex = currentPageIndex };
                connection = dc.dbConnection;
                connection.Open();
                sdr = DbCommand(sql.Select(parameters), connection, dc.databaseType).ExecuteReader();
                list = mapper.ToList<T>(sdr, mapInfo);
            }
            finally
            {
                if (sdr != null) sdr.Close();
                if (connection != null) connection.Close();
            }
            return list;
        }

        /// <summary>
        /// <para>根据ICondition分页查询</para>
        /// <para>e.g.</para>
        /// <para>ICondition&lt;T&gt; condition = new ICondition&lt;T&gt;();</para>
        /// <para>condition.Add(s => s.Id == 1);</para>
        /// <para>Database.Instance().Query&lt;T&gt;(condition);</para>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="condition">条件对象</param>
        /// <param name="pageSize"></param>
        /// <param name="currentPageIndex"></param>
        /// <returns></returns>
        public List<T> Query<T>(Condition<T> condition, int pageSize, int currentPageIndex) where T : class, new()
        {
            List<T> list;
            IDataReader sdr = null;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                string primaryKeyColumn = string.Join(",", mapInfo.column.Where(s => s.Key.PrimaryKey == true).Select(s => s.Key.Name));
                if (string.IsNullOrEmpty(primaryKeyColumn))
                    primaryKeyColumn = mapInfo.column.FirstOrDefault().Key.Name;
                dynamic parameters = new { Select = "*", TableName = mapInfo.table.Name, Where = condition.ToString(), PrimaryKeyColumn = primaryKeyColumn, PageSize = pageSize, CurrentPageIndex = currentPageIndex };
                connection = dc.dbConnection;
                connection.Open();
                sdr = DbCommand(sql.Select(parameters), connection, dc.databaseType).ExecuteReader();
                list = mapper.ToList<T>(sdr, mapInfo);
            }
            finally
            {
                if (sdr != null) sdr.Close();
                if (connection != null) connection.Close();
            }
            return list;
        }

        /// <summary>
        /// 根据Lambda表达式查询记录数
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="expression">Lambda表达式</param>
        /// <returns></returns>
        public int Count<T>(Expression<Func<T, bool>> expression) where T : class
        {
            int count;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                string script = sql.Select(new { Select = "COUNT(*)", TableName = mapInfo.table.Name, Where = expressionConvert.Convert(expression) });
                connection = dc.dbConnection;
                connection.Open();
                count = Convert.ToInt32(DbCommand(script, connection, dc.databaseType).ExecuteScalar());
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return count;
        }

        /// <summary>
        /// 根据ICondition查询记录数
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="condition">条件对象</param>
        /// <returns>记录个数</returns>
        public int Count<T>(Condition<T> condition) where T : class
        {
            int count;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                string script = sql.Select(new { Select = "COUNT(*)", TableName = mapInfo.table.Name, Where = condition.ToString() });
                connection = dc.dbConnection;
                connection.Open();
                count = Convert.ToInt32(DbCommand(script, connection, dc.databaseType).ExecuteScalar());
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return count;
        }
        #endregion

        #region 清空
        /// <summary>
        /// 清空表数据
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <returns></returns>
        public int Truncate<T>() where T : class
        {
            int val;
            IDbConnection connection = null;
            try
            {
                MapInfo mapInfo = mapper.MapEntity(typeof(T));
                var dc = connectionPool.Connect(mapInfo.database.Name);
                ISql sql = connectionPool.GetSql(dc.databaseType);
                dynamic parameters = new { TableName = mapInfo.table.Name };
                string script = sql.Truncate(parameters);
                connection = dc.dbConnection;
                connection.Open();
                val = Convert.ToInt32(DbCommand(script, connection, dc.databaseType).ExecuteScalar());
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            return val;
        }
        #endregion

        #region 系统
        /// <summary>
        /// 获取所有可用数据库名称(configuration.connectionStrings节点中已配置的可用数据库名称)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> QueryDatabaseNames()
        {
            return connectionPool.databaseConnections.Select(s => s.Key);
        }

        /// <summary>
        /// 获取指定数据库的类型
        /// </summary>
        /// <param name="databaseName">指定数据库的名称</param>
        /// <returns></returns>
        public DatabaseType QueryDatabaseType(string databaseName)
        {
            return connectionPool.Connect(databaseName).databaseType;
        }

        /// <summary>
        /// 获取指定数据库中所有表名
        /// </summary>
        /// <param name="databaseName">指定数据库的名称</param>
        /// <returns></returns>
        public IEnumerable<string> QueryTableNames(string databaseName)
        {
            IDataReader sdr = null;
            IDbConnection connection = null;
            try
            {
                DatabaseConnection databaseConnection = connectionPool.Connect(databaseName);
                ISql sql = connectionPool.GetSql(databaseConnection.databaseType);
                connection = databaseConnection.dbConnection;
                connection.Open();
                sdr = DbCommand(sql.GetTables(null), connection, databaseConnection.databaseType).ExecuteReader();
                while (sdr.Read())
                {
                    yield return sdr["name"].ToString();
                }
            }
            finally
            {
                if (sdr != null) sdr.Close();
                if (connection != null) connection.Close();
            }
        }

        /// <summary>
        /// 获取指定数据库中所有表名
        /// </summary>
        /// <typeparam name="T">该数据库下映射实体的类型</typeparam>
        /// <returns></returns>
        public IEnumerable<string> QueryTableNames<T>()
        {
            MapInfo mapInfo = mapper.MapEntity(typeof(T));
            return QueryTableNames(mapInfo.database.Name);
        }

        /// <summary>
        /// 获取指定数据表中所有字段名
        /// </summary>
        /// <param name="databaseName">指定数据库的名称</param>
        /// <param name="tableName">指定数据表的名称</param>
        /// <returns></returns>
        public IEnumerable<string> QueryColumnNames(string databaseName, string tableName)
        {
            IDataReader sdr = null;
            IDbConnection connection = null;
            try
            {
                dynamic parameters = new { TableName = tableName };
                DatabaseConnection databaseConnection = connectionPool.Connect(databaseName);
                ISql sql = connectionPool.GetSql(databaseConnection.databaseType);
                connection = databaseConnection.dbConnection;
                connection.Open();
                sdr = DbCommand(sql.GetColumns(parameters), connection, databaseConnection.databaseType).ExecuteReader();
                while (sdr.Read())
                {
                    yield return sdr["name"].ToString();
                }
            }
            finally
            {
                if (sdr != null) sdr.Close();
                if (connection != null) connection.Close();
            }
        }

        /// <summary>
        /// 获取指定数据表中所有字段名
        /// </summary>
        /// <typeparam name="T">该数据表映射实体的类型</typeparam>
        /// <returns></returns>
        public IEnumerable<string> QueryColumnNames<T>()
        {
            MapInfo mapInfo = mapper.MapEntity(typeof(T));
            return QueryColumnNames(mapInfo.database.Name, mapInfo.table.Name);
        }
        #endregion

        #region 执行
        /// <summary>
        /// <para>执行SQL脚本，根据类型委托返回指定类型的集合</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().ExecuteReader([databaseName], "SELECT name, value FROM ...", s => new { name = s["name"], value = s["value"] });</para>
        /// </summary>
        /// <typeparam name="T">返回的泛型结果类型（可使用匿名类型）</typeparam>
        /// <param name="databaseName">数据库名称</param>
        /// <param name="script">SQL语句脚本</param>
        /// <param name="selector">类型委托</param>
        /// <returns>IEnumerable&lt;T&gt;</returns>
        public IEnumerable<T> ExecuteReader<T>(string databaseName, string script, Func<IDataReader, T> selector)
        {
            DatabaseConnection databaseConnection = connectionPool.Connect(databaseName);
            IDbCommand dc = DbCommand(script, databaseConnection.dbConnection, databaseConnection.databaseType);
            IDataReader dr = null;
            try
            {
                dc.Connection.Open();
                dr = dc.ExecuteReader();
                while (dr.Read())
                {
                    yield return selector(dr);
                }
            }
            finally
            {
                if (dr != null) dr.Close();
                dc.Connection.Close();
            }
        }

        /// <summary>
        /// <para>执行SQL脚本，并执行参数为IDataReader的委托</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().ExecuteReader([databaseName], "SELECT name, value FROM ...", (d) => { for (var i = 0; i &lt; d.FieldCount; i++) Console.WriteLine(dr[i]); });</para>
        /// </summary>
        /// <param name="databaseName">数据库名称</param>
        /// <param name="script">SQL语句脚本</param>
        /// <param name="action">参数为IDataReader的Action委托</param>
        public void ExecuteReader(string databaseName, string script, Action<IDataReader> action)
        {
            DatabaseConnection databaseConnection = connectionPool.Connect(databaseName);
            IDbCommand dc = DbCommand(script, databaseConnection.dbConnection, databaseConnection.databaseType);
            IDataReader dr = null;
            try
            {
                dc.Connection.Open();
                dr = dc.ExecuteReader();
                while (dr.Read())
                {
                    action(dr);
                }
            }
            finally
            {
                if (dr != null) dr.Close();
                dc.Connection.Close();
            }
        }

        /// <summary>
        /// 执行SQL脚本，返回操作影响的记录条数
        /// </summary>
        /// <param name="databaseName">数据库名称</param>
        /// <param name="script">SQL语句脚本</param>
        /// <returns>int</returns>
        public int ExecuteNonQuery(string databaseName, string script)
        {
            DatabaseConnection databaseConnection = connectionPool.Connect(databaseName);
            IDbCommand dc = DbCommand(script, databaseConnection.dbConnection, databaseConnection.databaseType);
            try
            {
                dc.Connection.Open();
                return dc.ExecuteNonQuery();
            }
            finally
            {
                dc.Connection.Close();
            }
        }

        /// <summary>
        /// 执行SQL脚本，返回结果集中的第一列第一行
        /// </summary>
        /// <param name="databaseName">数据库名称</param>
        /// <param name="script">SQL语句脚本</param>
        /// <returns>int</returns>
        public object ExecuteScalar(string databaseName, string script)
        {
            DatabaseConnection databaseConnection = connectionPool.Connect(databaseName);
            IDbCommand dc = DbCommand(script, databaseConnection.dbConnection, databaseConnection.databaseType);
            try
            {
                dc.Connection.Open();
                return dc.ExecuteScalar();
            }
            finally
            {
                dc.Connection.Close();
            }
        }

        /// <summary>
        /// <para>执行SQL脚本，根据类型委托返回指定类型的集合</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().ExecuteReader(new SqlConnection("Server=.;Database=TEST;Trusted_Connection=Yes;Connect Timeout=90"), d => new { name = d["name"] }, "SELECT * FROM T_User WHERE id=@id", new SqlParameter("id", 1);</para>
        /// </summary>
        /// <typeparam name="T">返回的泛型结果类型（可使用匿名类型）</typeparam>
        /// <param name="connection">IDbConnection连接对象, 需初始化连接语句</param>
        /// <param name="selector">类型委托</param>
        /// <param name="script">SQL语句脚本</param>
        /// <param name="parameters">参数化查询所用参数</param>
        /// <returns>IEnumerable&lt;T&gt;</returns>
        public IEnumerable<T> ExecuteReader<T>(IDbConnection connection, Func<IDataReader, T> selector, string script, params IDbDataParameter[] parameters)
        {
            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = script;
            dbCommand.CommandType = CommandType.Text;
            IDataReader dr = null;
            foreach (IDbDataParameter parameter in parameters)
                dbCommand.Parameters.Add(parameter);
            try
            {
                dbCommand.Connection.Open();
                dr = dbCommand.ExecuteReader();
                while (dr.Read())
                {
                    yield return selector(dr);
                }
            }
            finally
            {
                if (dr != null) dr.Close();
                dbCommand.Connection.Close();
            }
        }

        /// <summary>
        /// <para>执行SQL脚本，并执行参数为IDataReader的委托</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().ExecuteReader(new SqlConnection("Server=.;Database=..."), (d) => { for (var i = 0; i &lt; d.FieldCount; i++) Console.WriteLine(dr[i]); }, "SELECT name, value FROM ...");</para>
        /// </summary>
        /// <param name="connection">IDbConnection连接对象, 需初始化连接语句</param>
        /// <param name="action">参数为IDataReader的Action委托</param>
        /// <param name="script">SQL语句脚本</param>
        /// <param name="parameters">参数化查询所用参数</param>
        public void ExecuteReader(IDbConnection connection, Action<IDataReader> action, string script, params IDbDataParameter[] parameters)
        {
            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = script;
            dbCommand.CommandType = CommandType.Text;
            IDataReader dr = null;
            foreach (IDbDataParameter parameter in parameters)
                dbCommand.Parameters.Add(parameter);
            try
            {
                dbCommand.Connection.Open();
                dr = dbCommand.ExecuteReader();
                while (dr.Read())
                {
                    action(dr);
                }
            }
            finally
            {
                if (dr != null) dr.Close();
                dbCommand.Connection.Close();
            }
        }

        /// <summary>
        /// 执行SQL脚本，返回操作影响的记录条数
        /// </summary>
        /// <param name="connection">IDbConnection连接对象, 需初始化连接语句</param>
        /// <param name="script">SQL语句脚本</param>
        /// <param name="parameters">参数化查询所用参数</param>
        /// <returns>int</returns>
        public int ExecuteNonQuery(IDbConnection connection, string script, params IDbDataParameter[] parameters)
        {
            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = script;
            dbCommand.CommandType = CommandType.Text;
            foreach (IDbDataParameter parameter in parameters)
                dbCommand.Parameters.Add(parameter);
            try
            {
                dbCommand.Connection.Open();
                return dbCommand.ExecuteNonQuery();
            }
            finally
            {
                dbCommand.Connection.Close();
            }
        }

        /// <summary>
        /// 执行SQL脚本，返回结果集中的第一列第一行
        /// </summary>
        /// <param name="connection">IDbConnection连接对象, 需初始化连接语句</param>
        /// <param name="script">SQL语句脚本</param>
        /// <param name="parameters">参数化查询所用参数</param>
        /// <returns>int</returns>
        public object ExecuteScalar(IDbConnection connection, string script, params IDbDataParameter[] parameters)
        {
            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = script;
            dbCommand.CommandType = CommandType.Text;
            foreach (IDbDataParameter parameter in parameters)
                dbCommand.Parameters.Add(parameter);
            try
            {
                dbCommand.Connection.Open();
                return dbCommand.ExecuteScalar();
            }
            finally
            {
                dbCommand.Connection.Close();
            }
        }
        #endregion

        #region 复杂查询
        /// <summary>
        /// <para>复杂查询</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().From&lt;Class1&gt;().InnerJoin&lt;Class2&gt;((a, b) => a.Id == b.Id).Where((a, b) => a.Id == 1).Query((a, b) => new { class1Id = a.Id, class2 = b }).ToList()</para>
        /// </summary>
        /// <typeparam name="T">数据对象类型</typeparam>
        /// <returns></returns>
        public ComplexQuery.FromSection<T> From<T>() where T : new()
        {
            return new ComplexQuery.FromSection<T>(new ComplexQuery.Wick { database = this });
        }

        /// <summary>
        /// <para>复杂查询</para>
        /// <para>e.g.</para>
        /// <para>Database.Instance().From&lt;Class1, Class2&gt;().Where((a, b) => a.Id == b.Id).OrderByAsc((a, b) => new { a.Id, b.Id }).Query((a, b) => new { class1 = a, class2 = b }).ToList();</para>
        /// </summary>
        /// <typeparam name="T0">数据对象类型1</typeparam>
        /// <typeparam name="T1">数据对象类型2</typeparam>
        /// <returns></returns>
        public ComplexQuery.FormSection<T0, T1> From<T0, T1>() where T0 : new() where T1 : new()
        {
            return new ComplexQuery.FormSection<T0, T1>(new ComplexQuery.Wick { database = this });
        }

        /// <summary>
        /// 复杂查询
        /// </summary>
        /// <typeparam name="T0">数据对象类型1</typeparam>
        /// <typeparam name="T1">数据对象类型2</typeparam>
        /// <typeparam name="T2">数据对象类型3</typeparam>
        /// <returns></returns>
        public ComplexQuery.FormSection<T0, T1, T2> From<T0, T1, T2>() where T0 : new() where T1 : new() where T2 : new()
        {
            return new ComplexQuery.FormSection<T0, T1, T2>(new ComplexQuery.Wick { database = this });
        }

        /// <summary>
        /// 复杂查询
        /// </summary>
        /// <typeparam name="T0">数据对象类型1</typeparam>
        /// <typeparam name="T1">数据对象类型2</typeparam>
        /// <typeparam name="T2">数据对象类型3</typeparam>
        /// <typeparam name="T3">数据对象类型4</typeparam>
        /// <returns></returns>
        public ComplexQuery.FormSection<T0, T1, T2, T3> From<T0, T1, T2, T3>() where T0 : new() where T1 : new() where T2 : new() where T3 : new()
        {
            return new ComplexQuery.FormSection<T0, T1, T2, T3>(new ComplexQuery.Wick { database = this });
        }
        #endregion
    }
}