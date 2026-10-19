using System;
using System.Runtime.InteropServices;

namespace EC_NET2
{
    /// <summary>
    /// PDO统一接口
    /// </summary>
    public interface IPDOMapping
    {
        /// <summary>
        /// 对象索引寄存器
        /// </summary>
        ushort Content { get; }
        /// <summary>对象字典索引</summary>
        int Index { get; }
        /// <summary>子索引</summary>
        int SubIndex { get; }
        /// <summary>数据类型</summary>
        Type Type { get; }
        /// <summary>字节大小</summary>
        int Size { get; }

        /// <summary>
        /// 从原始字节缓冲区读取数据并更新内部值
        /// </summary>
        /// <param name="buffer">PDO 原始字节数据</param>
        /// <param name="offset">该映射在缓冲区中的字节偏移</param>
        unsafe void ReadFrom(byte* buffer, int offset);

        /// <summary>
        /// 将内部值写入原始字节缓冲区
        /// </summary>
        unsafe void WriteTo(byte* buffer, int offset);
    }
    /// <summary>
    /// PDO映射基类
    /// </summary>
    public abstract class BasePDOMapping<T> : IPDOMapping where T : unmanaged
    {
        /// <summary>
        /// 从站PDO容器 0X1A00 或者0X1600
        /// </summary>
        public ushort Content { get; protected set; }
        /// <summary>
        /// 值
        /// </summary>
        private T _value;
        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; protected set; }
        /// <summary>
        /// 子索引
        /// </summary>
        public int SubIndex { get; protected set; }
        /// <summary>
        /// 类型
        /// </summary>
        public Type Type => typeof(T);
        /// <summary>
        /// 大小
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// PDO映射基类构造函数
        /// </summary>
        public unsafe BasePDOMapping()
        {
            Size = sizeof(T);
        }
        /// <summary>
        /// 获取值
        /// </summary>
        /// <returns></returns>
        public virtual T GetValue() => _value;
        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetValue(T value) => _value = value;
        /// <summary>
        /// 极致读取
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        public unsafe void ReadFrom(byte* buffer, int offset)
        {
            // 直接从指针偏移处读取 T
            _value = *(T*)(buffer + offset);
        }
        /// <summary>
        /// 极致写入
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        public unsafe void WriteTo(byte* buffer, int offset)
        {
            *(T*)(buffer + offset) = _value;
        }
    }
    /// <summary>
    /// RxPDO类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RxPDOMapping<T> : BasePDOMapping<T> where T : unmanaged
    {
        /// <summary>
        /// 接收值
        /// </summary>
        public T Value
        {
            get
            {
                return GetValue();
            }
            set
            {
                SetValue(value);
            }
        }
        /// <summary>
        /// PDO接收映射类构造函数
        /// </summary>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        public RxPDOMapping(ushort content, int index, int subIndex) : base()
        {
            base.Content = content;
            base.Index = index;
            base.SubIndex = subIndex;
        }
    }
    /// <summary>
    /// TxPDO类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TxPDOMapping<T> : BasePDOMapping<T> where T : unmanaged
    {
        /// <summary>
        /// 接收值
        /// </summary>
        public T Value
        {
            get
            {
                return GetValue();
            }
            set
            {
                SetValue(value);
            }
        }
        /// <summary>
        /// PDO接收映射类构造函数
        /// </summary>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        public TxPDOMapping(ushort content, int index, int subIndex) : base()
        {
            base.Content = content;
            base.Index = index;
            base.SubIndex = subIndex;
        }
    }
}
