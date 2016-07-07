using KF.ORM.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace KF.ORM.Infrastructure.Service
{
    /// <summary>
    /// Lambda表达式转换对象
    /// </summary>
    internal class ExpressionConvert
    {
        /// <summary>
        /// 根据Lambda表达式生成SQL条件语句(不包括"WHERE"、"AND"...)
        /// </summary>
        /// <param name="lambdaExpression">Lambda表达式</param>
        /// <param name="shorthand">是否简写</param>
        /// <returns>SQL条件语句(不包括"WHERE"、"AND"...)</returns>
        public string Convert(LambdaExpression lambdaExpression, bool shorthand = true)
        {
            if (lambdaExpression == null) return null;
            IList<string> expresser = new List<string>();
            foreach (ParameterExpression pe in lambdaExpression.Parameters)
            {
                if (!expresser.Contains(pe.Name))
                    expresser.Add(pe.Name);
            }
            return TranslateExpression(lambdaExpression.Body, lambdaExpression, expresser, shorthand);
        }

        /// <summary>
        /// 解析表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="fatherExpression">上一级表达式</param>
        /// <param name="expresser">表达者</param>
        /// <param name="shorthand">是否简写</param>
        /// <returns>SQL条件语句</returns>
        private string TranslateExpression(Expression expression, Expression fatherExpression = null, IList<string> expresser = null, bool shorthand = true)
        {
            StringBuilder sb = new StringBuilder();
            if (expression is BinaryExpression)
            {
                BinaryExpression be = expression as BinaryExpression;
                sb.Append("(");
                sb.Append(TranslateExpression(be.Left, be, expresser, shorthand));
                string expressionRight = TranslateExpression(be.Right, be, expresser, shorthand);
                if (string.IsNullOrEmpty(expressionRight))
                {
                    if (be.NodeType == ExpressionType.Equal)
                        sb.Append(" IS NULL");
                    else if (be.NodeType == ExpressionType.NotEqual)
                        sb.Append(" IS NOT NULL");
                }
                else
                {
                    sb.Append(CastType(be.NodeType));
                    sb.Append(expressionRight);
                }
                return sb.Append(")").ToString();
            }
            if (expression is MemberExpression)
            {
                MemberExpression me = expression as MemberExpression;
                if (expresser != null && expresser.Count > 0 && me.Expression is ParameterExpression && expresser.Contains((me.Expression as ParameterExpression).Name))
                {
                    if (!shorthand)
                    {
                        foreach (object customAttribute in me.Expression.Type.GetCustomAttributes(false))
                        {
                            if (customAttribute is TableAttribute)
                            {
                                sb.Append((customAttribute as TableAttribute).Name);
                                sb.Append(".");
                                break;
                            }
                        }
                    }
                    foreach (object attribute in me.Member.GetCustomAttributes(false))
                    {
                        if (attribute is ColumnAttribute)
                        {
                            ColumnAttribute columnAttr = attribute as ColumnAttribute;
                            sb.Append(columnAttr.Name);
                            return sb.ToString();
                        }
                    }
                    return me.Type.Name;
                }
                return CastValue(expression);
            }
            if (expression is NewArrayExpression)
            {
                NewArrayExpression ae = expression as NewArrayExpression;
                StringBuilder tmpstr = new StringBuilder();
                foreach (Expression ex in ae.Expressions)
                {
                    tmpstr.Append(TranslateExpression(ex, ae, expresser, shorthand));
                    tmpstr.Append(",");
                }
                return tmpstr.ToString(0, tmpstr.Length - 1);
            }
            if (expression is MethodCallExpression)
            {
                MethodCallExpression mce = expression as MethodCallExpression;
                if (mce.Method.Name == "Contains")
                {
                    if (expresser != null && expresser.Count > 0 && mce.Object is MemberExpression && (mce.Object as MemberExpression).Expression is ParameterExpression && expresser.Contains(((mce.Object as MemberExpression).Expression as ParameterExpression).Name))
                    {
                        if (fatherExpression != null && fatherExpression.NodeType == ExpressionType.Not)
                            return string.Format("({0} NOT LIKE '%{1}%')", TranslateExpression(mce.Object, mce, expresser, shorthand), CastValue(mce.Arguments[0], false));
                        return string.Format("({0} LIKE '%{1}%')", TranslateExpression(mce.Object, mce, expresser, shorthand), CastValue(mce.Arguments[0], false));
                    }
                    else
                    {
                        if (fatherExpression != null && fatherExpression.NodeType == ExpressionType.Not)
                            return string.Format("({0} NOT IN ({1}))", TranslateExpression(mce.Arguments[0], mce, expresser, shorthand), CastValue(mce.Object));
                        return string.Format("({0} IN ({1}))", TranslateExpression(mce.Arguments[0], mce, expresser, shorthand), CastValue(mce.Object));
                    }
                }
                if (mce.Method.Name == "ToString")
                    return TranslateExpression(mce.Object, mce, expresser, shorthand);
            }
            else if (expression is ConstantExpression)
            {
                ConstantExpression ce = expression as ConstantExpression;
                if (ce.Value == null)
                    return null;
                if (ce.Value is string || ce.Value is DateTime || ce.Value is char)
                    return string.Format("'{0}'", ce.Value);
                if (ce.Value is bool)
                    return bool.Parse(ce.Value.ToString()) ? "1" : "0";
                if (ce.Value is ValueType)
                    return ce.Value.ToString();
            }
            else if (expression is UnaryExpression)
            {
                UnaryExpression ue = ((UnaryExpression)expression);
                return TranslateExpression(ue.Operand, ue, expresser, shorthand);
            }
            else if (expression is NewExpression)
            {
                NewExpression ne = expression as NewExpression;
                StringBuilder tmpstr = new StringBuilder();
                foreach (var argument in ne.Arguments)
                {
                    tmpstr.AppendFormat("{0},", TranslateExpression(argument, ne, expresser, shorthand));
                }
                if (tmpstr.Length > 0)
                    return tmpstr.ToString(0, tmpstr.Length - 1);
                else return null;
            }
            return CastValue(expression);
        }

        /// <summary>
        /// 表达式目录树的节点的节点类型转换
        /// </summary>
        /// <param name="type">表达式目录树的节点的节点类型</param>
        /// <returns>SQL判断、链接语句</returns>
        private string CastType(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.NotEqual:
                    return " != ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return " + ";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return " - ";
                case ExpressionType.Divide:
                    return " / ";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return " * ";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取表达式常量、变量的值
        /// </summary>
        /// <param name="expression">表达式常量、变量</param>
        /// <param name="autoTag">是否自动添加引号'...'</param>
        /// <returns></returns>
        private string CastValue(Expression expression, bool autoTag = true)
        {
            object value = Expression.Lambda(expression).Compile().DynamicInvoke();
            if (value is ICollection)
            {
                StringBuilder tmpstr = new StringBuilder();
                ICollection collection = (ICollection)value;
                foreach (object item in collection)
                {
                    if (item is ValueType)
                        tmpstr.Append(item);
                    else
                        tmpstr.Append(string.Format("'{0}'", item));
                    tmpstr.Append(",");
                }
                if (collection.Count > 0)
                    return tmpstr.ToString(0, tmpstr.Length - 1);
                return "''";
            }
            if (!autoTag)
                return value.ToString();
            if (value is string || value is DateTime || value is char || value is bool)
                return string.Format("'{0}'", value);
            if (value is ValueType)
                return value.ToString();
            return string.Empty;
        }
    }
}