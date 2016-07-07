using KF.ORM.Infrastructure.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace KF.ORM.ComplexQuery
{
    /// <summary>
    /// 贯穿查询语句生成的芯
    /// </summary>
    public class Wick
    {
        /// <summary>
        /// 数据库连接接口实例
        /// </summary>
        public Database database;

        /// <summary>
        /// SQL语句From子句
        /// </summary>
        public StringBuilder script = new StringBuilder();

        internal IDictionary<Type, MapInfo> mapInfos = new Dictionary<Type, MapInfo>();

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string databaseName;
    }
}