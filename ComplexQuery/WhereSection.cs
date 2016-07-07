using System;
using System.Linq.Expressions;

namespace KF.ORM.ComplexQuery
{
    /// <summary>
    /// Where部分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WhereSection<T> : ExecuteSection<T> where T : new()
    {
        /// <summary>
        /// 初始化Where部分
        /// </summary>
        /// <param name="wick"></param>
        public WhereSection(Wick wick) : base(wick) { }

        /// <summary>
        /// Order By ... Asc
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public OrderSection<T> OrderByAsc<TR>(Expression<Func<T, TR>> selector)
        {
            wick.script.AppendFormat(" ORDER BY {0} ASC", wick.database.expressionConvert.Convert(selector));
            return new OrderSection<T>(wick);
        }

        /// <summary>
        /// Order By ... Desc
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public OrderSection<T> OrderByDesc<TR>(Expression<Func<T, TR>> selector)
        {
            wick.script.AppendFormat(" ORDER BY {0} DESC", wick.database.expressionConvert.Convert(selector));
            return new OrderSection<T>(wick);
        }
    }

    /// <summary>
    /// Where部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    public class WhereSection<T0, T1> : ExecuteSection<T0, T1> where T0 : new() where T1 : new()
    {
        /// <summary>
        /// 初始化Where部分
        /// </summary>
        /// <param name="_wick"></param>
        public WhereSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// Order By ... Asc
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public OrderSection<T0, T1> OrderByAsc<TR>(Expression<Func<T0, T1, TR>> selector)
        {
            wick.script.AppendFormat(" ORDER BY {0} ASC", wick.database.expressionConvert.Convert(selector, false));
            return new OrderSection<T0, T1>(wick);
        }

        /// <summary>
        /// Order By ... Desc
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public OrderSection<T0, T1> OrderByDesc<TR>(Expression<Func<T0, T1, TR>> selector)
        {
            wick.script.AppendFormat(" ORDER BY {0} DESC", wick.database.expressionConvert.Convert(selector, false));
            return new OrderSection<T0, T1>(wick);
        }
    }

    /// <summary>
    /// Where部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class WhereSection<T0, T1, T2> : ExecuteSection<T0, T1, T2> where T0 : new() where T1 : new() where T2 : new()
    {
        /// <summary>
        /// 初始化Where部分
        /// </summary>
        /// <param name="_wick"></param>
        public WhereSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// Order By ... Asc
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public OrderSection<T0, T1, T2> OrderByAsc<TR>(Expression<Func<T0, T1, T2, TR>> selector)
        {
            wick.script.AppendFormat(" ORDER BY {0} ASC", wick.database.expressionConvert.Convert(selector, false));
            return new OrderSection<T0, T1, T2>(wick);
        }

        /// <summary>
        /// Order By ... Desc
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public OrderSection<T0, T1, T2> OrderByDesc<TR>(Expression<Func<T0, T1, T2, TR>> selector)
        {
            wick.script.AppendFormat(" ORDER BY {0} DESC", wick.database.expressionConvert.Convert(selector, false));
            return new OrderSection<T0, T1, T2>(wick);
        }
    }

    /// <summary>
    /// Where部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class WhereSection<T0, T1, T2, T3> : ExecuteSection<T0, T1, T2, T3> where T0 : new() where T1 : new() where T2 : new() where T3 : new()
    {
        /// <summary>
        /// 初始化Where部分
        /// </summary>
        /// <param name="_wick"></param>
        public WhereSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// Order By ... Asc
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public OrderSection<T0, T1, T2, T3> OrderByAsc<TR>(Expression<Func<T0, T1, T2, T3, TR>> selector)
        {
            wick.script.AppendFormat(" ORDER BY {0} ASC", wick.database.expressionConvert.Convert(selector, false));
            return new OrderSection<T0, T1, T2, T3>(wick);
        }

        /// <summary>
        /// Order By ... Desc
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public OrderSection<T0, T1, T2, T3> OrderByDesc<TR>(Expression<Func<T0, T1, T2, T3, TR>> selector)
        {
            wick.script.AppendFormat(" ORDER BY {0} DESC", wick.database.expressionConvert.Convert(selector, false));
            return new OrderSection<T0, T1, T2, T3>(wick);
        }
    }
}