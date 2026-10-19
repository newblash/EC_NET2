using EC_NET2;
using EC_NET2_EX;
using EC_NET2_IMaster;
using EC_NET2_Interrop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable CS8600
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8622
namespace EC_NET2
{
    /// <summary>
    /// 主站
    /// </summary>
    public class Master : IMaster, IDisposable
    {
        /// <summary>
        /// 全局锁对象，用于同步所有EtherCAT主站实例的操作
        /// </summary>
        private static readonly object _lockerAll = new object();
        /// <summary>
        /// 锁对象，用于同步单个EtherCAT主站实例的操作
        /// </summary>
        private readonly object _locker = new object();
        /// <summary>
        /// 同步次数计数
        /// </summary>
        private long _inOutSyncCount;
        /// <summary>
        /// 主站索引
        /// </summary>
        private int _contextt = -1;
        private int _slaveCount = 0;
        /// <summary>
        /// 周期
        /// </summary>
        private uint _Cycl = 1000;
        /// <summary>
        /// 周期偏移
        /// </summary>
        private int _CyclShift = 0;
        /// <summary>
        /// 从站数量
        /// </summary>
        public int SlaveCount => _slaveCount;
        /// <summary>
        /// 停止主站时取消PDO循环信号
        /// </summary>
        private CancellationTokenSource cts;
        /// <summary>
        /// 启动事件
        /// </summary>
        public event EventHandler StartupEventHandler;
        /// <summary>
        /// 同步配置事件
        /// </summary>
        public event EventHandler SyncConfigEventHandler;
        /// <summary>
        /// 启动事件
        /// </summary>
        public event EventHandler PdoConfigEventHandler;
        /// <summary>
        /// EtherCAT主站活动开始事件处理程序
        /// </summary>
        public event EventHandler StartActivityEventHandler;
        /// <summary>
        /// EtherCAT主站输入事件
        /// </summary>
        public event EventHandler InputHandler;
        /// <summary>
        /// EtherCAT主站输出事件
        /// </summary>
        public event EventHandler OutputHandler;
        /// <summary>
        /// 读取从站状态事件,不开放给用户
        /// </summary>
        private event EventHandler ReadStateEventHandler;


        /// <summary>
        /// 读取从站 SDO 数据，直接转换为值类型（无额外分配）
        /// </summary>
        /// <typeparam name="T">值类型（结构体），大小不超过 32 字节</typeparam>
        /// <param name="slaveAddr">从站地址</param>
        /// <param name="index">对象字典索引</param>
        /// <param name="subIndex">子索引</param>
        /// <param name="isLittleEndian">数据字节序（默认小端）</param>
        /// <returns>转换后的值</returns>
        /// <exception cref="Exception">SDO 读取失败</exception>
        /// <summary>
        /// 读取从站 SDO 数据，直接转换为值类型（无托管分配）
        /// </summary>
        public unsafe T ReadSDO<T>(ushort slaveAddr, int index, int subIndex, bool isLittleEndian = true) where T : unmanaged
        {
            // 栈上分配 32 字节缓冲区（SDO 最大 4 字节，但保留 32 安全）
            byte* buffer = stackalloc byte[32];
            int size = 32;

            int result;
            lock (_locker)
            {
                result = Interrop.EcxSDOread(_contextt, slaveAddr, (ushort)index, (byte)subIndex, false, ref size, (IntPtr)buffer, 20000);
            }
            if (result <= 0)
                throw new Exception($"SDO Read Fail(slaveAddr:{slaveAddr} index:{index} subIndex:{subIndex} errorCode:{result})");

            // 将 buffer 数据复制到 T 结构体中
            T value;
            byte* pValue = (byte*)&value;
            int copyLen = Math.Min(size, sizeof(T));
            for (int i = 0; i < copyLen; i++)
                pValue[i] = buffer[i];
            // 剩余字节置零（避免未初始化）
            for (int i = copyLen; i < sizeof(T); i++)
                pValue[i] = 0;

            // 字节序调整（如果目标字节序与系统不同且类型大于 1 字节）
            if (BitConverter.IsLittleEndian != isLittleEndian && sizeof(T) > 1)
            {
                for (int i = 0; i < sizeof(T) / 2; i++)
                {
                    byte tmp = pValue[i];
                    pValue[i] = pValue[sizeof(T) - 1 - i];
                    pValue[sizeof(T) - 1 - i] = tmp;
                }
            }

            return value;
        }
        /// <summary>
        /// 写入从站SDO数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slaveAddr"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <param name="value"></param>
        /// <param name="isLittleEndian"></param>
        /// <exception cref="Exception"></exception>
        public unsafe void WriteSDO<T>(ushort slaveAddr, int index, int subIndex, T value, bool isLittleEndian = true) where T : unmanaged
        {
            byte* buffer = stackalloc byte[sizeof(T)];
            byte* pValue = (byte*)&value;

            // 复制数据到缓冲区
            for (int i = 0; i < sizeof(T); i++)
                buffer[i] = pValue[i];

            // 字节序调整（如果目标字节序与系统不同且类型 > 1 字节）
            if (BitConverter.IsLittleEndian != isLittleEndian && sizeof(T) > 1)
            {
                for (int i = 0; i < sizeof(T) / 2; i++)
                {
                    byte tmp = buffer[i];
                    buffer[i] = buffer[sizeof(T) - 1 - i];
                    buffer[sizeof(T) - 1 - i] = tmp;
                }
            }

            int result;
            lock (_locker)
            {
                result = Interrop.EcxSDOwrite(_contextt, slaveAddr, (ushort)index, (byte)subIndex, false, sizeof(T), (IntPtr)buffer, 20000);
            }
            if (result <= 0)
                throw new Exception($"SDO Write Fail(slaveAddr:{slaveAddr} index:{index} subIndex:{subIndex} errorCode:{result})");
        }
        /// <summary>
        /// 获取从站名称
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <returns></returns>
        public string GetSlaveName(ushort slaveAddr)
        {
            byte[] array = new byte[40];
            Interrop.GetEcxSlaveName(_contextt, (ushort)slaveAddr, array);
            return Encoding.ASCII.GetString(array).Replace("\0", "");
        }
        /// <summary>
        /// 读取从站状态
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <returns></returns>
        public SlaveState ReadState(ushort slaveAddr)
        {
            return Interrop.GetEcxSlaveState(_contextt, (ushort)slaveAddr);
        }
        /// <summary>
        /// 读取从站AL状态码
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <returns></returns>
        public ushort GetSlaveALstatuscode(ushort slaveAddr)
        {
            return Interrop.GetEcxSlaveALstatuscode(_contextt, slaveAddr);
        }
        /// <summary>
        /// 获取网卡名称及信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string GetInterfaceName(string name)
        {
            string name2 = name;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return name2;
            }

            Tuple<string, string, string> tuple = GetAvailableInterfaces().FirstOrDefault(x => x.Item1 == name || x.Item2 == name2);
            return tuple == null ? throw new Exception("未查询到网口:" + name2) : "\\Device\\NPF_" + tuple.Item3;
        }
        /// <summary>
        /// 获取可用的网络接口列表
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, string, string>> GetAvailableInterfaces()
        {
            List<Tuple<string, string, string>> list = new List<Tuple<string, string, string>>();
            NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in allNetworkInterfaces)
            {
                if (!networkInterface.IsReceiveOnly && networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.SupportsMulticast && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    list.Add(new Tuple<string, string, string>(networkInterface.Description, networkInterface.Name, networkInterface.Id));
                }
            }

            return list;
        }
        /// <summary>
        /// 更新从站状态事件,需要在其他线程中运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EtherCATMaster_ReadStateEventHandler(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Interrop.EcxReadState(_contextt);
                }
                catch
                {
                }
            });
        }
        /// <summary>
        /// 启动主站
        /// </summary>
        /// <param name="name">网卡名称</param>
        /// <param name="Cycl">运行周期</param>
        /// <param name="CyclShift">周期偏移</param>
        /// <param name="CpuCore">CPU核心</param>
        /// <returns>返回从站数量</returns>
        /// <exception cref="Exception"></exception>
        public int StartActivity(string name, uint Cycl = 1000, int CyclShift = 0, int CpuCore = 0)
        {
            lock (_lockerAll)
            {
                cts = new CancellationTokenSource();
                _Cycl = Cycl;
                _CyclShift = CyclShift;
                int num = 0;
                int iosize = 0;
                //获取网卡接口
                string interfaceName = GetInterfaceName(name);
                //初始化网卡接口
                if ((_contextt = Interrop.EcxInit(interfaceName)) < 0)
                {
                    throw new Exception($"EcInit Error(result:{_contextt})");
                }
                //配置从站
                if ((num = Interrop.EcxConfigInit(_contextt)) <= 0)
                {
                    throw new Exception($"EcConfigInit Error(result:{num})");
                }
                //检查从站状态是否为PreOperational
                Interrop.EcxStateCheck(_contextt, 0, SlaveState.PreOperational);
                //获取从站数量
                if ((_slaveCount = Interrop.GetEcxSlaveCount(_contextt)) <= 0)
                {
                    return 0;
                }
                else
                {
                    //在这里创建一个高优先级线程用来跑PDO
                    Thread HiThread = new Thread(() =>
                    {
                        //同步配置事件
                        this.SyncConfigEventHandler?.Invoke(this, EventArgs.Empty);
                        //最开始的启动事件
                        this.StartupEventHandler?.Invoke(this, EventArgs.Empty);
                        //发送PDO配置请求
                        this.PdoConfigEventHandler?.Invoke(this, EventArgs.Empty);
                        //检查配置表是否正确
                        if ((iosize = Interrop.EcxConfigMap(_contextt, 16384)) <= 0)
                        {
                            throw new Exception($"EcConfigMap Error(result:{iosize})");
                        }
                        //检查IO大小是否正确
                        if (iosize < Interrop.GetEcxSlaveObytes(_contextt, 0) + Interrop.GetEcxSlaveIbytes(_contextt, 0))
                        {
                            throw new Exception("iosize<Obytes+Ibytes");
                        }

                        //检查从站状态是否为SafeOperational
                        Interrop.EcxStateCheck(_contextt, 0, SlaveState.SafeOperational);
                        ReadStateEventHandler += EtherCATMaster_ReadStateEventHandler;
                        this.StartActivityEventHandler?.Invoke(this, EventArgs.Empty);
                        //进行一次PDO通讯
                        int wkc = Interrop.EcxSendProcessData(_contextt);
                        wkc = Interrop.EcxReceiveProcessData(_contextt, 2000);
                        if (wkc > 0)
                        {
                            //如果wkc正确,则进入op模式
                            Interrop.SetEcxSlaveState(_contextt, 0, SlaveState.Operational);
                            Interrop.EcxWriteState(_contextt, 0);
                            //等待所有从站进入OP模式,这行不能加,加了从站会一直卡在preop模式
                            //Interrop.EcxStateCheck(_contextt, 0, SlaveState.Operational, 1000 * 5000);
                        }
                        else
                        {
                            throw new Exception($"wkc error;return {wkc}");
                        }
                        //bool isop = false;
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            WinTime WTime = new WinTime(Cycl, CpuCore);

                            while (!cts.IsCancellationRequested)
                            {
                                lock (_locker)
                                {
                                    //首先输出事件,这样每个周期的数据都是完全同步的
                                    this.OutputHandler?.Invoke(this, EventArgs.Empty);
                                    Interrop.EcxSendProcessData(_contextt);
                                    wkc = Interrop.EcxReceiveProcessData(_contextt, 2000);
                                    //然后接受事件
                                    this.InputHandler?.Invoke(this, EventArgs.Empty);
                                    _inOutSyncCount++;
                                    if (_inOutSyncCount % 200 == 0)
                                    {
                                        this.ReadStateEventHandler?.Invoke(this, EventArgs.Empty);
                                    }
                                }
                                WTime.WaitNext();
                            }
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            LinuxTime _ticker = new LinuxTime(Cycl);  // Cycl = 4000μs
                            while (!cts.IsCancellationRequested)
                            {
                                lock (_locker)
                                {
                                    //首先输出事件,这样每个周期的数据都是完全同步的
                                    this.OutputHandler?.Invoke(this, EventArgs.Empty);
                                    Interrop.EcxSendProcessData(_contextt);
                                    wkc = Interrop.EcxReceiveProcessData(_contextt, 2000);
                                    //然后接受事件
                                    this.InputHandler?.Invoke(this, EventArgs.Empty);
                                    _inOutSyncCount++;
                                    if (_inOutSyncCount % 200 == 0)
                                    {
                                        this.ReadStateEventHandler?.Invoke(this, EventArgs.Empty);
                                    }
                                }
                                _ticker.WaitNext();
                            }
                        }

                    })
                    {
                        Priority = ThreadPriority.Highest
                    };
                    HiThread.Start();
                    //再次查找从站数量
                    _slaveCount = Interrop.GetEcxSlaveCount(_contextt);
                    return _slaveCount;
                }
            }
        }
        /// <summary>
        /// 停止EtherCAT活动
        /// </summary>
        public void StopActivity()
        {
            lock (_lockerAll)
            {
                cts?.Cancel();
                _inOutSyncCount = 0;
                if (_contextt >= 0)
                {
                    try
                    {
                        Interrop.SetEcxSlaveState(_contextt, 0, SlaveState.Init);
                    }
                    catch { };
                    try
                    {
                        Interrop.EcxWriteState(_contextt, 0);
                    }
                    catch { }
                    ;
                    try
                    {
                        Interrop.EcxClose(_contextt);
                    }
                    catch { }
                    ;
                }
            }
        }
        /// <summary>
        /// 释放接口
        /// </summary>
        public void Dispose()
        {
            try
            {
                StopActivity();
            }
            catch { }
        }
        /// <summary>
        /// 写入从站状态
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <param name="state"></param>
        public void WriteState(ushort slaveAddr, SlaveState state)
        {
            lock (_locker)
            {
                Interrop.SetEcxSlaveState(_contextt, slaveAddr, state);
                Interrop.EcxWriteState(_contextt, slaveAddr);
            }
        }
        /// <summary>
        /// 重新配置从站
        /// </summary>
        /// <param name="slaveAddr"></param>
        public void ReconfigSlave(ushort slaveAddr)
        {
            lock (_locker)
            {
                Interrop.EcxReconfigSlave(_contextt, slaveAddr, 2000);
            }
        }
        /// <summary>
        /// 获取从站输入缓冲区指针（仅用于初始化阶段缓存）
        /// </summary>
        public IntPtr GetInputPtr(ushort slave, out uint Length)
        {
            Length = Interrop.GetEcxSlaveIbytes(_contextt, slave);
            return Interrop.GetSlaveInputsPtr(_contextt, slave);
        }

        /// <summary>
        /// 获取从站输出缓冲区指针（仅用于初始化阶段缓存）
        /// </summary>
        public IntPtr GetOutputPtr(ushort slave, out uint Length)
        {
            Length = Interrop.GetEcxSlaveObytes(_contextt, slave);
            return Interrop.GetSlaveOutputsPtr(_contextt, slave);
        }
        /// <summary>
        /// 主站通用配置DC方法
        /// </summary>
        /// <param name="slave"></param>
        public void ConfigDC(ushort slave)
        {
            Interrop.EcxDcsync0(_contextt, slave, true, _Cycl * 1000, _CyclShift * 1000);
        }
        /// <summary>
        /// 配置RxPDO列表
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="_rxPDOs"></param>
        public void ConfigRxPDO(ushort slave, ICollection<IPDOMapping> _rxPDOs)
        {
            if (_rxPDOs == null || _rxPDOs.Count == 0) return;
            //按 Content 分组，保持原顺序
            var groups = _rxPDOs
                .Select((item, idx) => new { item, idx })
                .GroupBy(x => x.item.Content)
                .OrderBy(g => g.Min(x => x.idx))
                .ToList();
            //验证所有 Content 是否属于 RxPDO 范围（0x1600~0x16FF）
            const ushort rxPdoMin = 0x1600, rxPdoMax = 0x16FF;
            if (groups.Any(g => g.Key < rxPdoMin || g.Key > rxPdoMax))
                throw new ArgumentException("Contains TxPDO indices (0x1A00~) in RxPDO config.");
            //配置 PDO 分配（0x1C12）- 修正顺序
            ushort assignIndex = 0x1C12;
            //清空分配表
            WriteSDO(slave, assignIndex, 0, (ushort)0);
            //配置所有 PDO 映射对象（保持不变）
            foreach (var group in groups)
            {
                ushort content = group.Key;
                var mappings = group.Select(x => x.item).ToList();
                //清空映射
                WriteSDO(slave, content, 0, (byte)0);
                //写入映射条目
                int count = 0;
                foreach (var item in mappings)
                {
                    uint mappingValue = ((uint)item.Index << 16)
                                      | ((uint)item.SubIndex << 8)
                                      | ((uint)item.Size * 8); // Size 为字节数
                    WriteSDO(slave, content, (ushort)(++count), mappingValue);
                }
                //设置有效条目数
                WriteSDO(slave, content, 0, (byte)count);
            }
            //按顺序填充子索引
            int subIdx = 1;
            foreach (var group in groups)
            {
                WriteSDO(slave, assignIndex, (ushort)subIdx, group.Key);
                subIdx++;
            }
            //设置总条目数
            WriteSDO(slave, assignIndex, 0, (ushort)groups.Count);
        }
        /// <summary>
        /// 配置TxPDO列表
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="_txPDOs"></param>
        public void ConfigTxPDO(ushort slave, ICollection<IPDOMapping> _txPDOs)
        {
            if (_txPDOs == null || _txPDOs.Count == 0) return;

            //按 Content 分组，保持原顺序
            var groups = _txPDOs
                .Select((item, idx) => new { item, idx })
                .GroupBy(x => x.item.Content)
                .OrderBy(g => g.Min(x => x.idx))
                .ToList();

            //验证所有 Content 是否属于 RxPDO 范围（0x1600~0x16FF）
            const ushort rxPdoMin = 0x1A00, rxPdoMax = 0x1AFF;
            if (groups.Any(g => g.Key < rxPdoMin || g.Key > rxPdoMax))
                throw new ArgumentException("Contains TxPDO indices (0x1600~) in RxPDO config.");
            //配置 PDO 分配（0x1C13
            ushort assignIndex = 0x1C13;
            //清空分配表
            WriteSDO(slave, assignIndex, 0, (ushort)0);
            //配置所有 PDO 映射对象（保持不变）
            foreach (var group in groups)
            {
                ushort content = group.Key;
                var mappings = group.Select(x => x.item).ToList();
                // 清空映射
                WriteSDO(slave, content, 0, (byte)0);
                // 写入映射条目
                int count = 0;
                foreach (var item in mappings)
                {
                    uint mappingValue = ((uint)item.Index << 16)
                                      | ((uint)item.SubIndex << 8)
                                      | ((uint)item.Size * 8); // Size 为字节数
                    WriteSDO(slave, content, (ushort)(++count), mappingValue);
                }
                // 设置有效条目数
                WriteSDO(slave, content, 0, (byte)count);
            }
            // 再按顺序填充子索引
            int subIdx = 1;
            foreach (var group in groups)
            {
                WriteSDO(slave, assignIndex, (ushort)subIdx, group.Key);
                subIdx++;
            }
            WriteSDO(slave, assignIndex, 0, (ushort)groups.Count);
        }
    }
}
namespace EC_NET2_IMaster
{
    /// <summary>
    /// 主站接口
    /// </summary>
    public interface IMaster
    {
        /// <summary>
        /// 获取从站数量
        /// </summary>
        int SlaveCount { get; }
        /// <summary>
        /// 启动事件
        /// </summary>
        event EventHandler StartupEventHandler;
        /// <summary>
        /// 同步配置事件
        /// </summary>
        event EventHandler SyncConfigEventHandler;
        /// <summary>
        /// PDO配置事件
        /// </summary>
        event EventHandler PdoConfigEventHandler;
        /// <summary>
        /// EtherCAT开始活动事件处理器
        /// </summary>
        event EventHandler StartActivityEventHandler;
        /// <summary>
        /// EtherCAT主站输入输出同步事件处理程序
        /// </summary>
        event EventHandler InputHandler;
        /// <summary>
        /// EtherCAT主站输入输出同步事件处理程序
        /// </summary>
        event EventHandler OutputHandler;
        /// <summary>
        /// EtherCAT活动开始
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Cycl"></param>
        /// <param name="CyclShift"></param>
        /// <param name="CpuCore"></param>
        /// <returns></returns>
        int StartActivity(string name, uint Cycl = 1000, int CyclShift = 0, int CpuCore = 0);
        /// <summary>
        /// 停止EtherCAT活动
        /// </summary>
        void StopActivity();
        /// <summary>
        /// 读取从站的SDO值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slaveAddr"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        unsafe T ReadSDO<T>(ushort slaveAddr, int index, int subIndex, bool isLittleEndian = true) where T : unmanaged;
        /// <summary>
        /// 写入从站的SDO值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slaveAddr"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <param name="value"></param>
        /// <param name="isLittleEndian"></param>
        unsafe void WriteSDO<T>(ushort slaveAddr, int index, int subIndex, T value, bool isLittleEndian = true) where T : unmanaged;
        ///// <summary>
        ///// 读取从站的SDO（服务数据对象）值
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="slaveAddr"></param>
        ///// <param name="index"></param>
        ///// <param name="subIndex"></param>
        ///// <returns></returns>
        //T ReadSDO<T>(ushort slaveAddr, int index, int subIndex) where T : struct;
        /// <summary>
        /// 读取从站的状态
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <returns></returns>
        SlaveState ReadState(ushort slaveAddr);
        ///// <summary>
        ///// 写入从站的SDO（服务数据对象）值
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="slaveAddr"></param>
        ///// <param name="index"></param>
        ///// <param name="subIndex"></param>
        ///// <param name="value"></param>
        //void WriteSDO<T>(ushort slaveAddr, int index, int subIndex, T value) where T : struct;
        /// <summary>
        /// 写入从站的状态
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <param name="state"></param>
        void WriteState(ushort slaveAddr, SlaveState state);
        /// <summary>
        /// 重新配置从站
        /// </summary>
        /// <param name="slaveAddr"></param>
        void ReconfigSlave(ushort slaveAddr);
        /// <summary>
        /// 获取从站的名称
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <returns></returns>
        string GetSlaveName(ushort slaveAddr);
        /// <summary>
        /// 获取从站AL状态码
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <returns></returns>
        ushort GetSlaveALstatuscode(ushort slaveAddr);
        /// <summary>
        /// 获取输入内存地址
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        IntPtr GetInputPtr(ushort slave, out uint Length);
        /// <summary>
        /// 获取输出内存地址
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        IntPtr GetOutputPtr(ushort slave, out uint Length);
        /// <summary>
        /// 配置DC接口
        /// </summary>
        /// <param name="slave"></param>
        void ConfigDC(ushort slave);
        /// <summary>
        /// 配置PRXDO
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="_rxPDOs"></param>
        void ConfigRxPDO(ushort slave, ICollection<IPDOMapping> _rxPDOs);
        /// <summary>
        /// 配置TXPDO
        /// </summary>
        /// <param name="slave"></param>
        /// <param name="_txPDOs"></param>
        void ConfigTxPDO(ushort slave, ICollection<IPDOMapping> _txPDOs);
    }
}
