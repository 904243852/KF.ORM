using System;

namespace KF.ORM.Entity
{
    /// <summary>
    /// 数据库属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DatabaseAttribute : Attribute
    {
        private string _name = string.Empty;

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}