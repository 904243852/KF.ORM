namespace KF.ORM.ComplexQuery
{
    /// <summary>
    /// Order部分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrderSection<T> : ExecuteSection<T> where T : new()
    {
        /// <summary>
        /// 初始化Order部分
        /// </summary>
        /// <param name="wick"></param>
        public OrderSection(Wick wick) : base(wick) { }
    }

    /// <summary>
    /// Order部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    public class OrderSection<T0, T1> : ExecuteSection<T0, T1> where T0 : new() where T1 : new()
    {
        /// <summary>
        /// 初始化Order部分
        /// </summary>
        /// <param name="wick"></param>
        public OrderSection(Wick wick) : base(wick) { }
    }

    /// <summary>
    /// Order部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class OrderSection<T0, T1, T2> : ExecuteSection<T0, T1, T2> where T0 : new() where T1 : new() where T2 : new()
    {
        /// <summary>
        /// 初始化Order部分
        /// </summary>
        /// <param name="wick"></param>
        public OrderSection(Wick wick) : base(wick) { }
    }

    /// <summary>
    /// Order部分
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class OrderSection<T0, T1, T2, T3> : ExecuteSection<T0, T1, T2, T3> where T0 : new() where T1 : new() where T2 : new() where T3 : new()
    {
        /// <summary>
        /// 初始化Order部分
        /// </summary>
        /// <param name="wick"></param>
        public OrderSection(Wick wick) : base(wick) { }
    }
}