using System;
using System.Linq.Expressions;

namespace KF.ORM.ComplexQuery
{
    /// <summary>
    /// From部分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FromSection<T> : ExecuteSection<T> where T : new()
    {
        /// <summary>
        /// 初始化From部分
        /// </summary>
        /// <param name="_wick"></param>
        public FromSection(Wick _wick) : base(_wick)
        {
            wick.script.AppendFormat(" FROM {0}", _From<T>().table.Name);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public WhereSection<T> Where(Expression<Func<T, bool>> expression)
        {
            wick.script.AppendFormat(" WHERE {0}", wick.database.expressionConvert.Convert(expression));
            return new WhereSection<T>(wick);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="joiner"></param>
        /// <returns></returns>
        public JoinSection<T, T1> InnerJoin<T1>(Expression<Func<T, T1, bool>> joiner) where T1 : new()
        {
            _InnerJoin<T1>(joiner);
            return new JoinSection<T, T1>(wick);
        }
    }

    /// <summary>
    /// From部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    public class FormSection<T0, T1> : ExecuteSection<T0, T1> where T0 : new() where T1 : new()
    {
        /// <summary>
        /// 初始化From部分
        /// </summary>
        /// <param name="_wick"></param>
        public FormSection(Wick _wick) : base(_wick)
        {
            wick.script.AppendFormat(" FROM {0},{1}", _From<T0>().table.Name, _From<T1>().table.Name);
        }

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
    }

    /// <summary>
    /// From部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class FormSection<T0, T1, T2> : ExecuteSection<T0, T1> where T0 : new() where T1 : new() where T2 : new()
    {
        /// <summary>
        /// 初始化From部分
        /// </summary>
        /// <param name="_wick"></param>
        public FormSection(Wick _wick) : base(_wick)
        {
            wick.script.AppendFormat(" FROM {0},{1},{2}", _From<T0>().table.Name, _From<T1>().table.Name, _From<T2>().table.Name);
        }

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
    }

    /// <summary>
    /// From部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class FormSection<T0, T1, T2, T3> : ExecuteSection<T0, T1> where T0 : new() where T1 : new() where T2 : new() where T3 : new()
    {
        /// <summary>
        /// 初始化From部分
        /// </summary>
        /// <param name="_wick"></param>
        public FormSection(Wick _wick) : base(_wick)
        {
            wick.script.AppendFormat(" FROM {0},{1},{2},{3}", _From<T0>().table.Name, _From<T1>().table.Name, _From<T2>().table.Name, _From<T3>().table.Name);
        }

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