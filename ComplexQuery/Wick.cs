using KF.ORM.Infrastructure.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace KF.ORM.ComplexQuery
{
    /// <summary>
    /// �ᴩ��ѯ������ɵ�о
    /// </summary>
    public class Wick
    {
        /// <summary>
        /// ���ݿ����ӽӿ�ʵ��
        /// </summary>
        public Database database;

        /// <summary>
        /// SQL���From�Ӿ�
        /// </summary>
        public StringBuilder script = new StringBuilder();

        internal IDictionary<Type, MapInfo> mapInfos = new Dictionary<Type, MapInfo>();

        /// <summary>
        /// ���ݿ�����
        /// </summary>
        public string databaseName;
    }
}