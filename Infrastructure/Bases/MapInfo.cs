using KF.ORM.Entity;
using System.Collections.Generic;
using System.Reflection;

namespace KF.ORM.Infrastructure.Bases
{
    /// <summary>
    /// Entity与Table的映射信息
    /// </summary>
    internal class MapInfo
    {
        /// <summary>
        /// 实体类中关联的数据库
        /// </summary>
        public DatabaseAttribute database;

        /// <summary>
        /// 实体类中关联的数据库表
        /// </summary>
        public TableAttribute table;

        /// <summary>
        /// 数据库中各列与实体类中各成员属性映射集合
        /// </summary>
        public IList<KeyValuePair<ColumnAttribute, PropertyInfo>> column = new List<KeyValuePair<ColumnAttribute, PropertyInfo>>();
    }
}