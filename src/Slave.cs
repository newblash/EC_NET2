
using EC_NET2;
using EC_NET2_IMaster;
using EC_NET2_ISlave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
#pragma warning disable CS8600
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8622
namespace EC_NET2
{
    /// <summary>
    /// 从站实现
    /// </summary>
    public class Slave : ISlave
    {
        /// <summary>
        /// 从站设备的主站接口
        /// </summary>
        public IMaster Master { get; protected set; }
        /// <summary>
        /// 从站设备的地址
        /// </summary>
        public ushort SlaveAddr { get; }
        /// <summary>
        /// 私有字段
        /// </summary>
        private string _slaveNameLazy;
        /// <summary>
        /// 从站设备的名称
        /// </summary>
        public string SlaveName => _slaveNameLazy ?? "";
        /// <summary>
        /// 从站设备的状态
        /// </summary>
        public SlaveState SlaveState => Master.ReadState(SlaveAddr);
        /// <summary>
        /// 获取AL状态码
        /// </summary>
        public ushort SlaveALstatuscode => Master.GetSlaveALstatuscode(SlaveAddr);
        /// <summary>
        /// 从站同步模式
        /// </summary>
        public SyncMode syncMode { get; }
        /// <summary>
        /// 从站输入数据
        /// </summary>
        public byte[] Inputs { get; private set; }
        /// <summary>
        /// 从站输出数据
        /// </summary>
        public byte[] Outputs { get; private set; }
        #region 从站私有字段
        private readonly object _slaveLock = new object();
        private readonly List<IPDOMapping> _rxPDOs = new List<IPDOMapping>();
        private readonly List<IPDOMapping> _txPDOs = new List<IPDOMapping>();

        private IntPtr InputPtr;
        private IntPtr OutputPtr;
        private uint InputLength;
        private uint OutputLength;
        #endregion
        /// <summary>
        /// 从站类
        /// </summary>
        /// <param name="master"></param>
        /// <param name="slaveAddr"></param>
        /// <param name="SMode"></param>
        public Slave(IMaster master, SyncMode SMode, ushort slaveAddr)
        {
            Master = master;
            SlaveAddr = slaveAddr;
            syncMode = SMode;
            Master.InputHandler += InputHandler;
            Master.OutputHandler += OutputHandler;
            Master.SyncConfigEventHandler += SyncConfigEventHandler;
            Master.PdoConfigEventHandler += PdoConfigEventHandler;
            Master.StartActivityEventHandler += StartActivityEventHandler;
        }
        /// <summary>
        /// 在主站启动时初始化从站内存地址和长度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartActivityEventHandler(object sender, EventArgs e)
        {
            InputPtr = Master.GetInputPtr(SlaveAddr, out uint inLen);
            InputLength = inLen;
            OutputPtr = Master.GetOutputPtr(SlaveAddr, out uint outLen);
            OutputLength = outLen;
            if (Inputs == null || Inputs.Length != InputLength)
            {
                Inputs = new byte[InputLength];
            }
            if (Outputs == null || Outputs.Length != OutputLength)
            {
                Outputs = new byte[OutputLength];
            }
        }
        /// <summary>
        /// 同步配置事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncConfigEventHandler(object sender, EventArgs e)
        {
            _slaveNameLazy = Master.GetSlaveName(SlaveAddr);
            if (syncMode == SyncMode.FreeRun)
            {
                try
                {
                    Master.WriteSDO(SlaveAddr, 0x1C32, 1, (ushort)0);
                }
                catch { }
                try
                {
                    Master.WriteSDO(SlaveAddr, 0x1C33, 1, (ushort)0);
                }
                catch { }
            }
            else if (syncMode == SyncMode.DC)
            {
                Master.ConfigDC(SlaveAddr);
            }
        }
        /// <summary>
        /// PDO配置事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PdoConfigEventHandler(object sender, EventArgs e)
        {
            //等待配置从站结束
            Master.ConfigRxPDO(SlaveAddr, _rxPDOs);
            Master.ConfigTxPDO(SlaveAddr, _txPDOs);

        }
        /// <summary>
        /// 从战接收来自主站的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <returns></returns>
        public void AddRxPDOMapping<T>(ushort content, int index, int subIndex) where T : unmanaged
        {
            var temppdo = new RxPDOMapping<T>(content, index, subIndex);
            _rxPDOs.Add(temppdo);
        }
        /// <summary>
        /// 从站发送给主站的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <returns></returns>
        public void AddTxPDOMapping<T>(ushort content, int index, int subIndex) where T : unmanaged
        {
            var temppdo = new TxPDOMapping<T>(content, index, subIndex);
            _txPDOs.Add(temppdo);
        }
        /// <summary>
        /// 处理输出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private unsafe void OutputHandler(object sender, EventArgs e)
        {
            lock (_slaveLock)
            {
                if (OutputPtr != IntPtr.Zero && OutputLength > 0)
                {
                    fixed (byte* pOutputs = Outputs)
                    {
                        Buffer.MemoryCopy(pOutputs, (void*)OutputPtr, OutputLength, OutputLength);
                    }
                }
            }
        }
        /// <summary>
        /// 处理输入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private unsafe void InputHandler(object sender, EventArgs e)
        {
            if (InputPtr != IntPtr.Zero && InputLength > 0)
            {
                fixed (byte* pInputs = Inputs)
                {
                    Buffer.MemoryCopy((void*)InputPtr, pInputs, InputLength, InputLength);
                }
            }
        }
    }
}

namespace EC_NET2_ISlave
{
    /// <summary>
    /// 从站接口
    /// </summary>
    public interface ISlave
    {
        /// <summary>
        /// 从站设备的主站接口
        /// </summary>
        IMaster Master { get; }
        /// <summary>
        /// 添加RXPDO
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="content">对象索引寄存器</param>
        /// <param name="index">索引</param>
        /// <param name="subIndex">子索引</param>
        void AddRxPDOMapping<T>(ushort content, int index, int subIndex) where T : unmanaged;
        /// <summary>
        /// 添加TXPDO
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="content">对象索引寄存器</param>
        /// <param name="index">索引</param>
        /// <param name="subIndex">子索引</param>
        void AddTxPDOMapping<T>(ushort content, int index, int subIndex) where T : unmanaged;
        /// <summary>
        /// 从站设备的地址
        /// </summary>
        ushort SlaveAddr { get; }
        /// <summary>
        /// 从站设备的名称
        /// </summary>
        string SlaveName { get; }
        /// <summary>
        /// 从站设备的状态
        /// </summary>
        SlaveState SlaveState { get; }
        /// <summary>
        /// 从站同步模式
        /// </summary>
        SyncMode syncMode { get; }
        /// <summary>
        /// 从站设备的输入数据
        /// </summary>
        byte[] Inputs { get; }
        /// <summary>
        /// 从站设备的输出数据
        /// </summary>
        byte[] Outputs { get; }
    }
}
