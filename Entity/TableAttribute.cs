using System;

namespace KF.ORM.Entity
{
    /// <summary>
    /// 数据库表属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute : Attribute
    {
        private string _name = string.Empty;

        /// <summary>
        /// 表名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}