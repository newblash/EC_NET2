using System;
using System.Collections.Generic;
using System.Text;

namespace EC_NET2
{
    /// <summary>
    /// 轴配置
    /// </summary>
    public struct AxisConfigs
    {
        /// <summary>
        /// 运行模式
        /// </summary>
        public readonly ModesOperations mode;
        /// <summary>
        /// TX寄存器
        /// </summary>
        public readonly ushort txcontent;
        /// <summary>
        /// RX寄存器
        /// </summary>
        public readonly ushort rxcontent;
        /// <summary>
        /// 轴配置构造函数
        /// </summary>
        /// <param name="_mode"></param>
        /// <param name="_txcontent"></param>
        /// <param name="_rxcontent"></param>
        public AxisConfigs(ModesOperations _mode,ushort _txcontent, ushort _rxcontent)
        {
            mode = _mode;
            txcontent = _txcontent;
            rxcontent = _rxcontent;
        }
    }
    /// <summary>
    /// 从站状态
    /// </summary>
    public enum SlaveState : ushort
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        Unknow = 0,
        /// <summary>
        /// 初始化状态
        /// </summary>
        Init = 1,
        /// <summary>
        /// 预操作状态
        /// </summary>
        PreOperational = 2,
        /// <summary>
        /// 引导状态
        /// </summary>
        Boot = 3,
        /// <summary>
        /// 安全操作状态
        /// </summary>
        SafeOperational = 4,
        /// <summary>
        /// 操作状态
        /// </summary>
        Operational = 8,
        /// <summary>
        /// 应答状态
        /// </summary>
        Ack = 16,
        /// <summary>
        /// 初始化错误活动状态
        /// </summary>
        Init_ErrorActive = 17,
        /// <summary>
        /// 预操作错误活动状态
        /// </summary>
        PreOperational_ErrorActive = 18,
        /// <summary>
        /// 安全操作错误活动状态
        /// </summary>
        SafeOperational_ErrorActive = 20,
        /// <summary>
        /// 操作错误活动状态
        /// </summary>
        Operational_ErrorActive = 24
    }
    /// <summary>
    /// 同步模式
    /// </summary>
    public enum SyncMode : byte
    {
        /// <summary>
        /// 自由运行模式
        /// </summary>
        FreeRun = 0,
        /// <summary>
        /// DC 同步模式
        /// </summary>
        DC = 2
    }
    /// <summary>
    /// 从站操作模式
    /// </summary>
    public enum ModesOperations : byte
    {
        /// <summary>
        /// 无模式
        /// </summary>
        NO = 0,
        /// <summary>
        /// 轮廓位置模式
        /// </summary>
        PP = 0X01,
        /// <summary>
        /// 轮廓速度模式
        /// </summary>
        PV = 0X03,
        /// <summary>
        /// 轮廓转矩模式
        /// </summary>
        PT = 0X04,
        /// <summary>
        /// 回零模式
        /// </summary>
        HM = 0X06,
        /// <summary>
        /// 周期同步位置模式
        /// </summary>
        CSP = 0X08,
        /// <summary>
        /// 周期同步速度模式
        /// </summary>
        CSV = 0X09,
        /// <summary>
        /// 周期同步转矩模式
        /// </summary>
        CST = 0X10,
    }

}
