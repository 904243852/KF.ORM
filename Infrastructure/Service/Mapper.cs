using KF.ORM.Entity;
using KF.ORM.Infrastructure.Bases;
using KF.ORM.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace KF.ORM.Infrastructure.Service
{
    internal class Mapper
    {
        private IDictionary<Type, MapInfo> mapInfos = new Dictionary<Type, MapInfo>();

        /// <summary>
        /// 获取实体映射对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public MapInfo MapEntity(Type type)
        {
            if (mapInfos.ContainsKey(type)) return mapInfos[type];
            MapInfo mapInfo = new MapInfo();
            foreach (object customAttribute in type.GetCustomAttributes(false))
            {
                if (customAttribute is DatabaseAttribute)
                {
                    mapInfo.database = customAttribute as DatabaseAttribute;
                }
                else if (customAttribute is TableAttribute)
                {
                    mapInfo.table = customAttribute as TableAttribute;
                }
            }
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                object[] customAttributes = property.GetCustomAttributes(false);
                foreach (object customAttribute in customAttributes)
                {
                    if (customAttribute is ColumnAttribute)
                    {
                        mapInfo.column.Add(new KeyValuePair<ColumnAttribute, PropertyInfo>(customAttribute as ColumnAttribute, property));
                        break;
                    }
                }
            }
            mapInfos.Add(type, mapInfo);
            return mapInfo;
        }

        /// <summary>
        /// 实体化查询的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="mapInfo"></param>
        /// <returns></returns>
        public List<T> ToList<T>(IDataReader dataReader, MapInfo mapInfo) where T : new()
        {
            List<T> list = new List<T>();
            while (dataReader.Read())
            {
                T entity = new T();
                foreach (var c in mapInfo.column)
                {
                    if (c.Value.SetMethod.IsPublic)
                        Reflection.SetPropertyValue(entity, c.Value, dataReader[c.Key.Name]);
                    else
                        c.Value.SetValue(entity, dataReader[c.Key.Name]);
                }
                list.Add(entity);
            }
            return list;
        }

        /// <summary>
        /// 获取实体类中不为空的成员值作为sql语句的参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="mapInfo"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<IDbDataParameter> GetParameters<T>(T entity, MapInfo mapInfo, ISql sql)
        {
            if (mapInfo.column == null || mapInfo.column.Count == 0) return new List<IDbDataParameter>();
            List<IDbDataParameter> paramList = new List<IDbDataParameter>();
            foreach (var c in mapInfo.column)
            {
                if (c.Key.NotSaved || c.Key.AutoId) continue;
                if (!string.IsNullOrEmpty(c.Key.Name))
                {
                    object value = Reflection.GetPropertyValue(entity, c.Value);
                    IDbDataParameter param = sql.CreateParameter();
                    param.ParameterName = c.Key.Name;
                    if (value == null) param.Value = DBNull.Value;
                    else param.Value = value;
                    paramList.Add(param);
                }
            }
            return paramList;
        }
    }
}