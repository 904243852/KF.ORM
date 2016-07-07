using System;

namespace KF.ORM.Entity
{
    /// <summary>
    /// 数据库表中的列属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ColumnAttribute : Attribute
    {
        private string _name = string.Empty;

        /// <summary>
        /// 列名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// 不保存到数据库中（插入、更新中忽略该字段）
        /// </summary>
        public bool NotSaved { get; set; }

        /// <summary>
        /// 是否自增，一张表只能有一个IDENTITY
        /// </summary>
        public bool AutoId { get; set; }
    }
}