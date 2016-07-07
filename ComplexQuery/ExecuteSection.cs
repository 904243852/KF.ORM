using KF.ORM.Infrastructure.Bases;
using KF.ORM.Infrastructure.Service;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace KF.ORM.ComplexQuery
{
    /// <summary>
    /// 执行部分基类
    /// </summary>
    public class ExecuteSection
    {
        /// <summary>
        /// 贯穿查询过程的芯
        /// </summary>
        protected Wick wick;

        /// <summary>
        /// 初始化执行部分基类
        /// </summary>
        /// <param name="_wick"></param>
        public ExecuteSection(Wick _wick)
        {
            wick = _wick;
        }

        /// <summary>
        /// _InnerJoin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="joiner"></param>
        protected void _InnerJoin<T>(LambdaExpression joiner)
        {
            Type type = typeof(T);
            MapInfo mapInfo = wick.database.mapper.MapEntity(type);
            if (wick.databaseName != mapInfo.database.Name) throw new Exception(string.Format("{0}({1})不属于数据库{2}", type.Name, mapInfo.table.Name, wick.databaseName));
            if (!wick.mapInfos.ContainsKey(type))
                wick.mapInfos.Add(type, mapInfo);
            else throw new Exception(string.Format("{0}({1})不能 Inner Join 自身", type.Name, mapInfo.table.Name));
            wick.script.AppendFormat(" INNER JOIN {0} ON {1}", mapInfo.table.Name, wick.database.expressionConvert.Convert(joiner, false));
        }

        /// <summary>
        /// _From
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal MapInfo _From<T>()
        {
            Type type = typeof(T);
            MapInfo mapInfo = wick.database.mapper.MapEntity(type);
            if (string.IsNullOrEmpty(wick.databaseName))
                wick.databaseName = mapInfo.database.Name;
            else
            {
                if (wick.databaseName != mapInfo.database.Name)
                    throw new Exception(string.Format("{0}({1})不属于数据库{2}", type.Name, mapInfo.table.Name, wick.databaseName));
            }
            if (!wick.mapInfos.ContainsKey(type))
                wick.mapInfos.Add(type, mapInfo);
            else
                throw new Exception(string.Format("{0}({1})已存在", type.Name, mapInfo.table.Name));
            return mapInfo;
        }
    }

    /// <summary>
    /// 执行部分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExecuteSection<T> : ExecuteSection where T : new()
    {
        /// <summary>
        /// 初始化执行部分
        /// </summary>
        /// <param name="_wick"></param>
        public ExecuteSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Query()
        {
            MapInfo mapInfo = wick.mapInfos[typeof(T)];
            return wick.database.ExecuteReader(mapInfo.database.Name, "SELECT *" + wick.script.ToString(),
                (dataReader) =>
                {
                    T entity = new T();
                    foreach (var c in mapInfo.column)
                    {
                        Reflection.SetPropertyValue(entity, c.Value, dataReader[c.Key.Name]);
                    }
                    return entity;
                });
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public IEnumerable<TR> Query<TR>(Func<T, TR> selector = null)
        {
            MapInfo mapInfo = wick.mapInfos[typeof(T)];
            return wick.database.ExecuteReader(mapInfo.database.Name, "SELECT *" + wick.script.ToString(),
                (dataReader) =>
                {
                    T entity = new T();
                    foreach (var c in mapInfo.column)
                    {
                        Reflection.SetPropertyValue(entity, c.Value, dataReader[c.Key.Name]);
                    }
                    return selector(entity);
                });
        }

        /// <summary>
        /// 计数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            MapInfo mapInfo = wick.mapInfos[typeof(T)];
            return (int)wick.database.ExecuteScalar(mapInfo.database.Name, "SELECT *" + wick.script.ToString());
        }
    }

    /// <summary>
    /// 执行部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    public class ExecuteSection<T0, T1> : ExecuteSection where T0 : new() where T1 : new()
    {
        /// <summary>
        /// 初始化执行部分
        /// </summary>
        /// <param name="_wick"></param>
        public ExecuteSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(Func<T0, T1, T> selector)
        {
            var mapInfo0 = wick.mapInfos[typeof(T0)];
            var mapInfo1 = wick.mapInfos[typeof(T1)];
            return wick.database.ExecuteReader(wick.mapInfos[typeof(T0)].database.Name, "SELECT *" + wick.script.ToString(),
                (dataReader) =>
                {
                    T0 entity0 = new T0();
                    foreach (var c in mapInfo0.column)
                    {
                        Reflection.SetPropertyValue(entity0, c.Value, dataReader[c.Key.Name]);
                    }
                    T1 entity1 = new T1();
                    foreach (var c in mapInfo1.column)
                    {
                        Reflection.SetPropertyValue(entity1, c.Value, dataReader[c.Key.Name]);
                    }
                    return selector(entity0, entity1);
                });
        }
    }

    /// <summary>
    /// 执行部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class ExecuteSection<T0, T1, T2> : ExecuteSection where T0 : new() where T1 : new() where T2 : new()
    {
        /// <summary>
        /// 初始化执行部分
        /// </summary>
        /// <param name="_wick"></param>
        public ExecuteSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(Func<T0, T1, T2, T> selector)
        {
            var mapInfo0 = wick.mapInfos[typeof(T0)];
            var mapInfo1 = wick.mapInfos[typeof(T1)];
            var mapInfo2 = wick.mapInfos[typeof(T2)];
            return wick.database.ExecuteReader(wick.mapInfos[typeof(T0)].database.Name, "SELECT *" + wick.script.ToString(),
                (dataReader) =>
                {
                    T0 entity0 = new T0();
                    foreach (var c in mapInfo0.column)
                    {
                        Reflection.SetPropertyValue(entity0, c.Value, dataReader[c.Key.Name]);
                    }
                    T1 entity1 = new T1();
                    foreach (var c in mapInfo1.column)
                    {
                        Reflection.SetPropertyValue(entity1, c.Value, dataReader[c.Key.Name]);
                    }
                    T2 entity2 = new T2();
                    foreach (var c in mapInfo2.column)
                    {
                        Reflection.SetPropertyValue(entity2, c.Value, dataReader[c.Key.Name]);
                    }
                    return selector(entity0, entity1, entity2);
                });
        }
    }

    /// <summary>
    /// 执行部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class ExecuteSection<T0, T1, T2, T3> : ExecuteSection where T0 : new() where T1 : new() where T2 : new() where T3 : new()
    {
        /// <summary>
        /// 初始化执行部分
        /// </summary>
        /// <param name="_wick"></param>
        public ExecuteSection(Wick _wick) : base(_wick) { }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(Func<T0, T1, T2, T3, T> selector)
        {
            var mapInfo0 = wick.mapInfos[typeof(T0)];
            var mapInfo1 = wick.mapInfos[typeof(T1)];
            var mapInfo2 = wick.mapInfos[typeof(T2)];
            var mapInfo3 = wick.mapInfos[typeof(T3)];
            return wick.database.ExecuteReader(wick.mapInfos[typeof(T0)].database.Name, "SELECT *" + wick.script.ToString(),
                (dataReader) =>
                {
                    T0 entity0 = new T0();
                    foreach (var c in mapInfo0.column)
                    {
                        Reflection.SetPropertyValue(entity0, c.Value, dataReader[c.Key.Name]);
                    }
                    T1 entity1 = new T1();
                    foreach (var c in mapInfo1.column)
                    {
                        Reflection.SetPropertyValue(entity1, c.Value, dataReader[c.Key.Name]);
                    }
                    T2 entity2 = new T2();
                    foreach (var c in mapInfo2.column)
                    {
                        Reflection.SetPropertyValue(entity2, c.Value, dataReader[c.Key.Name]);
                    }
                    T3 entity3 = new T3();
                    foreach (var c in mapInfo3.column)
                    {
                        Reflection.SetPropertyValue(entity3, c.Value, dataReader[c.Key.Name]);
                    }
                    return selector(entity0, entity1, entity2, entity3);
                });
        }
    }
}