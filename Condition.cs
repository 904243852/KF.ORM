using KF.ORM.Infrastructure.Service;
using System;
using System.Linq.Expressions;
using System.Text;

namespace KF.ORM
{
    /// <summary>
    /// 条件查询对象入口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Condition<T>
    {
        private StringBuilder _scripts;

        private ExpressionConvert expressionConvert = new ExpressionConvert();

        /// <summary>
        /// 根据Lambda表达式添加条件
        /// </summary>
        /// <param name="expression">Lambda表达式</param>
        /// <returns></returns>
        public Condition<T> Add(Expression<Func<T, bool>> expression)
        {
            if (_scripts == null)
                _scripts = new StringBuilder();
            else
                _scripts.Append(" AND ");
            _scripts.Append(expressionConvert.Convert(expression));
            return this;
        }

        /// <summary>
        /// 根据SQL语句添加条件
        /// </summary>
        /// <param name="sqlscript">SQL命令</param>
        /// <returns></returns>
        public Condition<T> Add(string sqlscript)
        {
            if (_scripts == null)
                _scripts = new StringBuilder();
            else
                _scripts.Append(" AND ");
            _scripts.Append(sqlscript);
            return this;
        }

        /// <summary>
        /// 将Condition对象转换为SQL命令
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _scripts == null ? string.Empty : _scripts.ToString();
        }
    }
}