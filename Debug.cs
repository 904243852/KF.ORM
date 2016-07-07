#if DEBUG
using System;
using System.Configuration;
using System.Data;
using System.Reflection;

namespace KF.ORM
{
    /// <summary>
    /// 调试接口
    /// </summary>
    public interface IDebug
    {
        /// <summary>
        /// 记录IDbCommand信息
        /// </summary>
        /// <param name="dbCommand"></param>
        void LogDbCommand(IDbCommand dbCommand);
    }

    internal class Debug
    {
        static IDebug debug = null;

        static Debug()
        {
            string settingString = ConfigurationManager.AppSettings["Debug"];
            if (string.IsNullOrEmpty(settingString)) return;
            try
            {
                string assemblyString = settingString.Split(',')[0].Trim();
                Assembly assembly = Assembly.Load(assemblyString);
                string typeName = settingString.Split(',')[1].Trim();
                var instance = assembly.CreateInstance(typeName);
                if (instance is IDebug)
                {
                    debug = instance as IDebug;
                }
                else
                    throw new System.Exception("未找到有效Debug实现对象:该Debug实现对象不继承“IDebug”接口");
            }
            catch (Exception ex)
            {
                throw new System.Exception(string.Format("未找到有效Debug实现对象:{0}", ex.Message));
            }
        }

        /// <summary>
        /// 记录IDbCommand信息
        /// </summary>
        /// <param name="dbCommand"></param>
        public static void LogDbCommand(IDbCommand dbCommand)
        {
            if (debug != null)
                debug.LogDbCommand(dbCommand);
        }
    }
}
#endif