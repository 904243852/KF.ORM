using System;
using System.Reflection;

namespace KF.ORM.Infrastructure.Service
{
    /// <summary>
    /// 反射方法对象
    /// </summary>
    internal class Reflection
    {
        /// <summary>
        /// 获取指定类型的属性集
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// 获取指定类型的成员变量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        #region 快速执行方法

        /// <summary>
        /// 快速执行Method
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object FastMethodInvoke(object obj, MethodInfo method, params object[] parameters)
        {
            return DynamicCalls.GetMethodInvoker(method)(obj, parameters);
        }

        /// <summary>
        /// 快速实例化一个T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>()
        {
            return (T)Create(typeof(T))();
        }

        /// <summary>
        /// 快速实例化一个CreateInstanceHandler
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static CreateInstanceHandler Create(Type type)
        {
            return DynamicCalls.GetInstanceCreator(type);
        }

        #endregion

        #region 设置属性值

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(object obj, PropertyInfo property, object value)
        {
            if (property.CanWrite)
            {
                if (property == null) return;
                var propertySetter = DynamicCalls.GetPropertySetter(property);
                value = DataType.ChangeType(value, property.PropertyType);
                if (value != null)
                {
                    propertySetter(obj, value);
                }
            }
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            SetPropertyValue(obj.GetType(), obj, propertyName, value);
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(Type type, object obj, string propertyName, object value)
        {
            PropertyInfo property = type.GetProperty(propertyName);
            if (property != null)
            {
                SetPropertyValue(obj, property, value);
            }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(T entity, string propertyName)
        {
            PropertyInfo property = entity.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(entity, null);
            }
            return null;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(T entity, PropertyInfo property)
        {
            if (property != null)
            {
                return property.GetValue(entity, null);
            }
            return null;
        }

        /// <summary>
        /// 获取属性类型
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Type GetPropertyType(Type classType, string propertyName)
        {
            PropertyInfo property = classType.GetProperty(propertyName);
            if (property != null)
            {
                return property.PropertyType;
            }

            return null;
        }

        #endregion
    }
}