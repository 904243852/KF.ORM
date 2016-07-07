using System;
using System.Linq.Expressions;

namespace KF.ORM.ComplexQuery
{
    /// <summary>
    /// Join部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    public class JoinSection<T0, T1> : ExecuteSection<T0, T1> where T0 : new() where T1 : new()
    {
        /// <summary>
        /// 初始化Join部分
        /// </summary>
        /// <param name="_wick"></param>
        public JoinSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public WhereSection<T0, T1> Where(Expression<Func<T0, T1, bool>> expression)
        {
            wick.script.AppendFormat(" WHERE {0}", wick.database.expressionConvert.Convert(expression, false));
            return new WhereSection<T0, T1>(wick);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="joiner"></param>
        /// <returns></returns>
        public JoinSection<T0, T1, T2> InnerJoin<T2>(Expression<Func<T0, T1, T2, bool>> joiner) where T2 : new()
        {
            _InnerJoin<T2>(joiner);
            return new JoinSection<T0, T1, T2>(wick);
        }
    }

    /// <summary>
    /// Join部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class JoinSection<T0, T1, T2> : ExecuteSection<T0, T1, T2> where T0 : new() where T1 : new() where T2 : new()
    {
        /// <summary>
        /// 初始化Join部分
        /// </summary>
        /// <param name="_wick"></param>
        public JoinSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public WhereSection<T0, T1, T2> Where(Expression<Func<T0, T1, T2, bool>> expression)
        {
            wick.script.AppendFormat(" WHERE {0}", wick.database.expressionConvert.Convert(expression, false));
            return new WhereSection<T0, T1, T2>(wick);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="joiner"></param>
        /// <returns></returns>
        public JoinSection<T0, T1, T2, T3> InnerJoin<T3>(Expression<Func<T0, T1, T2, T3, bool>> joiner) where T3 : new()
        {
            _InnerJoin<T3>(joiner);
            return new JoinSection<T0, T1, T2, T3>(wick);
        }
    }

    /// <summary>
    /// Join部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class JoinSection<T0, T1, T2, T3> : ExecuteSection<T0, T1, T2, T3> where T0 : new() where T1 : new() where T2 : new() where T3 : new()
    {
        /// <summary>
        /// 初始化Join部分
        /// </summary>
        /// <param name="_wick"></param>
        public JoinSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public WhereSection<T0, T1, T2, T3> Where(Expression<Func<T0, T1, T2, T3, bool>> expression)
        {
            wick.script.AppendFormat(" WHERE {0}", wick.database.expressionConvert.Convert(expression, false));
            return new WhereSection<T0, T1, T2, T3>(wick);
        }
    }
}