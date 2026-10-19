using EC_NET2;
using EC_NET2_EX;
using EC_NET2_IMaster;
using EC_NET2_IAxis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
#pragma warning disable CS8600
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8622
namespace EC_NET2
{
    /// <summary>
    /// 轴控制
    /// </summary>
    public class Axis : IAxis
    {
        #region 轴控PDO字段
        private readonly TxPDOMapping<ushort> _errorCode;
        /// <summary>
        /// 状态字映射
        /// </summary>
        private readonly TxPDOMapping<ushort> _statusWord;
        /// <summary>
        /// 操作模式显示映射
        /// </summary>
        private readonly TxPDOMapping<byte> _modesOperationDisplay;
        /// <summary>
        /// 实际位置值映射
        /// </summary>
        private readonly TxPDOMapping<int> _positionActualValue;
        /// <summary>
        /// 实际速度值映射
        /// </summary>
        private readonly TxPDOMapping<int> _velocityActualValue;
        /// <summary>
        /// 实际扭矩值映射
        /// </summary>
        private readonly TxPDOMapping<short> _torqueActualValue;
        /// <summary>
        /// 控制字映射
        /// </summary>
        private readonly RxPDOMapping<ushort> _controlword;
        /// <summary>
        /// 操作模式映射
        /// </summary>
        private readonly RxPDOMapping<byte> _modesOperation;
        /// <summary>
        /// 目标位置映射
        /// </summary>
        private readonly RxPDOMapping<int> _targetPosition;
        /// <summary>
        /// 配置的速度映射
        /// </summary>
        private readonly RxPDOMapping<uint> _profileVelocity;
        /// <summary>
        /// 配置的加速度映射
        /// </summary>
        private readonly RxPDOMapping<uint> _profileAcceleration;
        /// <summary>
        /// 配置的减速度映射
        /// </summary>
        private readonly RxPDOMapping<uint> _profileDeceleration;
        /// <summary>
        /// 目标速度映射
        /// </summary>
        private readonly RxPDOMapping<int> _targetVelocity;
        /// <summary>
        /// 目标扭矩映射
        /// </summary>
        private readonly RxPDOMapping<short> _targetTorque;
        /// <summary>
        /// 扭矩斜率映射
        /// </summary>
        private readonly RxPDOMapping<uint> _torqueSlope;
        private readonly List<IPDOMapping> _rxPDOs = new List<IPDOMapping>();
        private readonly List<IPDOMapping> _txPDOs = new List<IPDOMapping>();
        #endregion
        #region 轴接口字段
        /// <summary>
        /// 从站设备的主站接口
        /// </summary>
        public IMaster Master { get; protected set; }
        /// <summary>
        /// 从站设备的地址
        /// </summary>
        public ushort SlaveAddr { get; }
        /// <summary>
        /// 获取AL状态码
        /// </summary>
        public ushort SlaveALstatuscode => Master.GetSlaveALstatuscode(SlaveAddr);
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
        /// 从站同步模式
        /// </summary>
        public SyncMode syncMode { get; }
        #endregion
        #region 状态字段
        /// <summary>
        /// 操作模式显示
        /// </summary>
        public byte ModesOperationDisplay
        {
            get
            {
                if (_modesOperationDisplay != null)
                    return _modesOperationDisplay.Value;
                return 0;
            }
        }
        /// <summary>
        /// 状态字
        /// </summary>
        public ushort StatusWord
        {
            get
            {
                if (_statusWord != null)
                    return _statusWord.Value;
                return 0;
            }
        }
        /// <summary>
        /// 错误字段
        /// </summary>
        public ushort ErrorCode
        {
            get
            {
                if (_errorCode != null)
                    return _errorCode.Value;
                return 0;
            }
        }
        /// <summary>
        /// 实际位置
        /// </summary>
        public int PositionActualValue
        {
            get
            {
                if (_positionActualValue != null)
                    return _positionActualValue.Value;
                return 0;
            }
        }
        /// <summary>
        /// 实际速度
        /// </summary>
        public int VelocityActualValue
        {
            get
            {
                if (_velocityActualValue != null)
                    return _velocityActualValue.Value;
                return 0;
            }
        }
        /// <summary>
        /// 实际扭矩
        /// </summary>
        public short TorqueActualValue
        {
            get
            {
                if (_torqueActualValue != null)
                    return _torqueActualValue.Value;
                return 0;
            }
        }
        /// <summary>
        /// 伺服是否准备好。
        /// </summary>
        public bool ReadyToSwitchOn => StatusWord.GetBit(0);
        /// <summary>
        /// 伺服电源。
        /// </summary>
        public bool SwitchedOn => StatusWord.GetBit(1);
        /// <summary>
        /// 获取轴的操作使能状态。
        /// </summary>
        public bool OperationEnabled => StatusWord.GetBit(2);
        /// <summary>
        /// 获取轴的故障状态。
        /// </summary>
        public bool Fault => StatusWord.GetBit(3);
        /// <summary>
        /// 获取轴的电压使能状态。
        /// </summary>
        public bool VoltageEnabled => StatusWord.GetBit(4);
        /// <summary>
        /// 获取轴的快速停止状态。
        /// </summary>
        public bool QuickStop => StatusWord.GetBit(5);
        /// <summary>
        /// 获取轴的开关状态，当设备处于禁用状态时。
        /// </summary>
        public bool SwitchOnDisabled => StatusWord.GetBit(6);
        /// <summary>
        /// 获取轴的警告状态。
        /// </summary>
        public bool Warning => StatusWord.GetBit(7);
        /// <summary>
        /// 获取轴的操作模式状态。
        /// </summary>
        public bool Remote => StatusWord.GetBit(9);
        /// <summary>
        /// 获取轴的目标到达状态。
        /// </summary>
        public bool TargetReached => StatusWord.GetBit(10);
        /// <summary>
        /// 设置轴的内部限位状态。
        /// </summary>
        public bool InternalLimitActive => StatusWord.GetBit(11);
        /// <summary>
        /// 回零确认状态
        /// </summary>
        public bool HomingAttained
        {
            get
            {
                if (ModesOperationDisplay == 6)
                {
                    return StatusWord.GetBit(15);
                }

                return false;
            }
        }
        /// <summary>
        /// PP模式目标位置值确认
        /// </summary>
        public bool TargetValueAcknowledge_PP
        {
            get
            {
                if (ModesOperationDisplay == 1)
                {
                    return StatusWord.GetBit(12);
                }

                return false;
            }
        }
        /// <summary>
        /// 获取PP的错误状态。
        /// </summary>
        public bool FollowingError_PP
        {
            get
            {
                if (ModesOperationDisplay == 1)
                {
                    return StatusWord.GetBit(13);
                }

                return false;
            }
        }
        /// <summary>
        /// 获取轴的速度接受状态。
        /// </summary>
        public bool Velocity0_PV
        {
            get
            {
                if (ModesOperationDisplay == 3)
                {
                    return StatusWord.GetBit(12);
                }

                return false;
            }
        }
        /// <summary>
        /// 获取轴回原点的接受状态。
        /// </summary>
        public bool HomingAttained_HM
        {
            get
            {
                if (ModesOperationDisplay == 6)
                {
                    return StatusWord.GetBit(12);
                }

                return false;
            }
        }
        /// <summary>
        /// 获取轴的回原点错误状态。
        /// </summary>
        public bool HomingError_HM
        {
            get
            {
                if (ModesOperationDisplay == 6)
                {
                    return StatusWord.GetBit(13);
                }

                return false;
            }
        }
        private ModesOperations ActualMode { get; set; }
        #endregion
        #region 控制字段
        /// <summary>
        /// 获取或设置操作模式，表示从站的当前操作模式。
        /// </summary>
        public byte ModesOperation
        {
            get
            {
                if (_modesOperation != null)
                    return _modesOperation.Value;
                return 0;
            }
            set
            {
                if (_modesOperation != null)
                    _modesOperation.Value = value;
            }
        }
        /// <summary>
        /// 获取或设置控制字，表示从站的控制指令。
        /// </summary>
        public ushort Controlword
        {
            get
            {
                if (_controlword != null)
                    return _controlword.Value;
                return 0;
            }
            set
            {
                if (_controlword != null)
                    _controlword.Value = value;
            }
        }
        /// <summary>
        /// 获取或设置目标位置，表示从站的目标位置。
        /// </summary>
        public int TargetPosition
        {
            get
            {
                if (_targetPosition != null)
                    return _targetPosition.Value;
                return 0;
            }
            set
            {
                if (_targetPosition != null)
                    _targetPosition.Value = value;
            }
        }
        /// <summary>
        /// 获取或设置配置的速度，表示从站的速度配置值。
        /// </summary>
        public uint ProfileVelocity
        {
            get
            {
                if (_profileVelocity != null)
                    return _profileVelocity.Value;
                return 0;
            }
            set
            {
                if (_profileVelocity != null)
                    _profileVelocity.Value = value;
            }
        }
        /// <summary>
        /// 获取或设置配置的加速度，表示从站的加速度配置值。
        /// </summary>
        public uint ProfileAcceleration
        {
            get
            {
                if (_profileAcceleration != null)
                    return _profileAcceleration.Value;
                return 0;
            }
            set
            {
                if (_profileAcceleration != null)
                    _profileAcceleration.Value = value;
            }
        }
        /// <summary>
        /// 获取或设置配置的减速度，表示从站的减速度配置值。
        /// </summary>
        public uint ProfileDeceleration
        {
            get
            {
                if (_profileDeceleration != null)
                    return _profileDeceleration.Value;
                return 0;
            }
            set
            {
                if (_profileDeceleration != null)
                    _profileDeceleration.Value = value;
            }
        }
        /// <summary>
        /// 获取或设置目标速度，表示从站的目标速度。
        /// </summary>
        public int TargetVelocity
        {
            get
            {
                if (_targetVelocity != null)
                    return _targetVelocity.Value;
                return 0;
            }
            set
            {
                if (_targetVelocity != null)
                    _targetVelocity.Value = value;
            }
        }
        /// <summary>
        /// 获取或设置目标扭矩，表示从站的目标扭矩。
        /// </summary>
        public short TargetTorque
        {
            get
            {
                if (_targetTorque != null)
                    return _targetTorque.Value;
                return 0;
            }
            set
            {
                if (_targetTorque != null)
                    _targetTorque.Value = value;
            }
        }
        /// <summary>
        /// 获取或设置扭矩斜率，表示从站的扭矩斜率。
        /// </summary>
        public uint TorqueSlope
        {
            get
            {
                if (_torqueSlope != null)
                    return _torqueSlope.Value;
                return 0;
            }
            set
            {
                if (_torqueSlope != null)
                    _torqueSlope.Value = value;
            }
        }
        #endregion
        #region 控制方法
        /// <summary>
        /// 停止轴运行
        /// <para>deceleration:停止速度脉冲</para>
        /// </summary>
        /// <param name="deceleration"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void Stop(uint deceleration)
        {
            ProfileDeceleration = deceleration;
            Controlword = 11;
        }
        /// <summary>
        /// 暂停轴运行
        /// <para>deceleration:暂停速度脉冲</para>
        /// </summary>
        /// <param name="deceleration"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void Halt(uint deceleration)
        {
            ProfileDeceleration = deceleration;
            Controlword = 271;
        }
        /// <summary>
        /// 复位轴
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public void Reset()
        {
            if (ErrorCode != 0)
            {
                Controlword = Controlword.SetBit(7, true);
                var sw = Stopwatch.StartNew();
                const int timeoutMs = 300;
                while (ErrorCode != 0)
                {
                    if (sw.ElapsedMilliseconds > timeoutMs)
                        return;
                    Thread.Sleep(1); // 给驱动器时间处理
                }
                Controlword = Controlword.SetBit(7, false);
            }
        }
        /// <summary>
        /// 回原点
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public void Home()
        {
            //设置模式为回零模式
            ModesOperation = 6;
            var sw = Stopwatch.StartNew();
            const int timeoutMs = 300;
            while (ModesOperationDisplay != 6)
            {
                if (sw.ElapsedMilliseconds > timeoutMs)
                    return;
                Thread.Sleep(1); // 给驱动器时间处理
            }
            //必须在使能模式下才能回零
            Controlword = 31;
        }
        /// <summary>
        /// 模式切换
        /// </summary>
        /// <param name="MO"></param>
        public void ModeSwitch(ModesOperations MO)
        {
            if (ModesOperationDisplay != (byte)MO)
            {
                switch (MO)
                {
                    case ModesOperations.PP:
                        TargetPosition = PositionActualValue;
                        break;
                    case ModesOperations.PV:
                        TargetVelocity = VelocityActualValue;
                        break;
                    case ModesOperations.PT:
                        TargetTorque = TorqueActualValue;
                        break;
                    case ModesOperations.CSP:
                        TargetPosition = PositionActualValue;
                        break;
                    case ModesOperations.CSV:
                        TargetVelocity = VelocityActualValue;
                        break;

                    case ModesOperations.CST:
                        TargetTorque = TorqueActualValue;
                        break;
                }
                ModesOperation = (byte)MO;
                var sw = Stopwatch.StartNew();
                const int timeoutMs = 300;
                while (ModesOperationDisplay != (byte)MO)
                {
                    if (sw.ElapsedMilliseconds > timeoutMs)
                        return;
                    Thread.Sleep(1);
                }
                ActualMode = MO;
                if (StatusWord.GetBit(2))
                {
                    Controlword = 15;
                }
            }
        }
        /// <summary>
        /// 轴使能
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public void PowerOn()
        {
            const int timeoutMs = 300;
            Controlword = 6;
            var sw1 = Stopwatch.StartNew();
            while (!StatusWord.GetBit(0))
            {
                if (sw1.ElapsedMilliseconds > timeoutMs)
                    return;
                Thread.Sleep(1); // 给驱动器时间处理
            }
            Controlword = 7;
            var sw2 = Stopwatch.StartNew();
            while (!StatusWord.GetBit(1))
            {
                if (sw2.ElapsedMilliseconds > timeoutMs)
                    return;
                Thread.Sleep(1); // 给驱动器时间处理
            }
            Controlword = 15;
            var sw3 = Stopwatch.StartNew();
            while (!StatusWord.GetBit(2))
            {
                if (sw3.ElapsedMilliseconds > timeoutMs)
                    return;
                Thread.Sleep(1); // 给驱动器时间处理
            }
        }
        /// <summary>
        /// 轴去使能
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public void PowerOff()
        {
            Controlword = 0;
        }
        /// <summary>
        /// 周期同步位置运动
        /// </summary>
        /// <param name="position"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void CSP_Position(int position)
        {
            if (ActualMode == ModesOperations.CSP)
            {
                Controlword = 15;
                TargetPosition = position;
            }

        }
        /// <summary>
        /// 周期同步速度运动
        /// </summary>
        /// <param name="velocity"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void CSV_Velocity(int velocity)
        {
            if (ActualMode == ModesOperations.CSV)
            {
                Controlword = 15;
                TargetVelocity = velocity;
            }
        }
        /// <summary>
        /// 周期扭矩运动
        /// </summary>
        /// <param name="torque"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void CST_Torque(short torque)
        {
            if (ActualMode == ModesOperations.CST)
            {
                Controlword = 15;
                TargetTorque = torque;
            }
        }
        /// <summary>
        /// 绝对位置移动
        /// <para>position:位置</para>
        /// <para>velocity:运行速度</para>
        /// <para>acceleration:加速度</para>
        /// <para>deceleration:减速度</para>
        /// <para>immediatelyEffect:是否立即生效</para>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="deceleration"></param>
        /// <param name="immediatelyEffect"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void MoveAbsolute(int position, uint velocity, uint acceleration, uint deceleration, bool immediatelyEffect = false)
        {
            if (ActualMode == ModesOperations.PP)
            {
                Controlword = 15;
                var sw = Stopwatch.StartNew();
                const int timeoutMs = 300;
                while (TargetValueAcknowledge_PP)
                {
                    if (sw.ElapsedMilliseconds > timeoutMs)
                        return;
                    Thread.Sleep(1); // 给驱动器时间处理
                }
                TargetPosition = position;
                ProfileVelocity = velocity;
                ProfileAcceleration = acceleration;
                ProfileDeceleration = deceleration;
                Controlword = (ushort)(immediatelyEffect ? 63u : 31u);
                //确认目标位置被接受
                var sw1 = Stopwatch.StartNew();
                while (!TargetValueAcknowledge_PP)
                {
                    if (sw1.ElapsedMilliseconds > timeoutMs)
                        return;
                    Thread.Sleep(1); // 给驱动器时间处理
                }
                Controlword = 15;
            }

        }
        /// <summary>
        /// 相对位置移动
        /// <para>position:位置</para>
        /// <para>velocity:运行速度</para>
        /// <para>acceleration:加速度</para>
        /// <para>deceleration:减速度</para>
        /// <para>immediatelyEffect:是否立即生效</para>
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="velocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="deceleration"></param>
        /// <param name="immediatelyEffect"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void MoveRelative(int distance, uint velocity, uint acceleration, uint deceleration, bool immediatelyEffect = false)
        {
            if (ActualMode == ModesOperations.PP)
            {
                Controlword = 79;
                var sw = Stopwatch.StartNew();
                const int timeoutMs = 300;
                while (TargetValueAcknowledge_PP)
                {
                    if (sw.ElapsedMilliseconds > timeoutMs)
                        return;
                    Thread.Sleep(1); // 给驱动器时间处理
                }
                TargetPosition = distance;
                ProfileVelocity = velocity;
                ProfileAcceleration = acceleration;
                ProfileDeceleration = deceleration;
                Controlword = (ushort)(immediatelyEffect ? 127u : 95u);
                //确认目标位置被接受
                var sw1 = Stopwatch.StartNew();
                while (!TargetValueAcknowledge_PP)
                {
                    if (sw1.ElapsedMilliseconds > timeoutMs)
                        return;
                    Thread.Sleep(1); // 给驱动器时间处理
                }
                Controlword = 15;
            }
        }
        /// <summary>
        /// 扭矩控制
        /// <para>targetTorque:目标扭矩</para>
        /// <para>torqueSlope:扭矩斜率</para>
        /// </summary>
        /// <param name="targetTorque"></param>
        /// <param name="torqueSlope"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void TorqueControl(short targetTorque, uint torqueSlope)
        {
            if (ActualMode == ModesOperations.PT)
            {
                Controlword = 15;
                TargetTorque = targetTorque;
                TorqueSlope = torqueSlope;
            }
        }
        /// <summary>
        /// 速度控制
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="deceleration"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void VelocityControl(int velocity, uint acceleration, uint deceleration)
        {
            if (ActualMode == ModesOperations.PV)
            {
                Controlword = 15;
                TargetVelocity = velocity;
                ProfileAcceleration = acceleration;
                ProfileDeceleration = deceleration;
            }
        }
        #endregion
        #region  轴初始化代码
        private readonly object _slaveLock = new object();
        private IntPtr InputPtr;
        private IntPtr OutputPtr;
        private uint InputLength;
        private uint OutputLength;
        /// <summary>
        /// 轴控制类
        /// </summary>
        /// <param name="master"></param>
        /// <param name="SMode"></param>
        /// <param name="slaveAddr"></param>
        /// <param name="offset"></param>
        /// <param name="Mode"></param>
        /// <param name="txcontent"></param>
        /// <param name="rxcontent"></param>
        public Axis(IMaster master, SyncMode SMode, ushort slaveAddr, int offset = 0, ModesOperations Mode = 0, ushort txcontent = 0x1A00, ushort rxcontent = 0x1600)
        {
            ActualMode = Mode;
            Master = master;
            syncMode = SMode;
            SlaveAddr = slaveAddr;
            Master.InputHandler += InputHandler;
            Master.OutputHandler += OutputHandler;
            Master.SyncConfigEventHandler += SyncConfigEventHandler;
            Master.PdoConfigEventHandler += PdoConfigEventHandler;
            Master.StartActivityEventHandler += StartActivityEventHandler;
            _errorCode = AddTxPDOMapping<ushort>(txcontent, 0x603F + offset, 0);
            _controlword = AddRxPDOMapping<ushort>(rxcontent, 0x6040 + offset, 0);
            _statusWord = AddTxPDOMapping<ushort>(txcontent, 0x6041 + offset, 0);
            _modesOperation = AddRxPDOMapping<byte>(rxcontent, 0x6060 + offset, 0);
            _modesOperationDisplay = AddTxPDOMapping<byte>(txcontent, 0x6061 + offset, 0);
            switch (Mode)
            {
                case ModesOperations.PP://轮廓位置
                    _targetPosition = AddRxPDOMapping<int>(rxcontent, 0x607A + offset, 0);
                    _profileVelocity = AddRxPDOMapping<uint>(rxcontent, 0x6081 + offset, 0);
                    _profileAcceleration = AddRxPDOMapping<uint>(rxcontent, 0x6083 + offset, 0);
                    _profileDeceleration = AddRxPDOMapping<uint>(rxcontent, 0x6084 + offset, 0);
                    _positionActualValue = AddTxPDOMapping<int>(txcontent, 0x6064 + offset, 0);
                    break;
                case ModesOperations.PV://轮廓速度
                    _profileVelocity = AddRxPDOMapping<uint>(rxcontent, 0x6081 + offset, 0);
                    _profileAcceleration = AddRxPDOMapping<uint>(rxcontent, 0x6083 + offset, 0);
                    _profileDeceleration = AddRxPDOMapping<uint>(rxcontent, 0x6084 + offset, 0);
                    _targetVelocity = AddRxPDOMapping<int>(rxcontent, 0x60FF + offset, 0);
                    _velocityActualValue = AddTxPDOMapping<int>(txcontent, 0x606C + offset, 0);
                    break;
                case ModesOperations.PT://轮廓转矩
                    _targetTorque = AddRxPDOMapping<short>(rxcontent, 0x6071 + offset, 0);
                    _torqueSlope = AddRxPDOMapping<uint>(rxcontent, 0x6087 + offset, 0);
                    _torqueActualValue = AddTxPDOMapping<short>(txcontent, 0x6077 + offset, 0);
                    break;
                case ModesOperations.HM://回零模式
                    break;
                case ModesOperations.CSP://周期同步位置模式
                    _targetPosition = AddRxPDOMapping<int>(rxcontent, 0x607A + offset, 0);
                    _positionActualValue = AddTxPDOMapping<int>(txcontent, 0x6064 + offset, 0);
                    break;
                case ModesOperations.CSV://周期同步速度模式
                    _targetVelocity = AddRxPDOMapping<int>(rxcontent, 0x60FF + offset, 0);
                    _velocityActualValue = AddTxPDOMapping<int>(txcontent, 0x606C + offset, 0);
                    break;
                case ModesOperations.CST://周期同步转矩模式
                    _targetTorque = AddRxPDOMapping<short>(rxcontent, 0x6071 + offset, 0);
                    _torqueActualValue = AddTxPDOMapping<short>(txcontent, 0x6077 + offset, 0);
                    break;
            }
            ModesOperation = (byte)Mode;
        }
        
        /// <summary>
        /// 轴控制类
        /// </summary>
        /// <param name="master"></param>
        /// <param name="SMode"></param>
        /// <param name="slaveAddr"></param>
        /// <param name="offset"></param>
        /// <param name="axisConfigs"></param>
        public Axis(IMaster master, SyncMode SMode, ushort slaveAddr, int offset = 0, AxisConfigs[] axisConfigs = null)
        {
            if(axisConfigs is null)
            {
                throw new ArgumentNullException("轴配置不能为null");
            }
            ActualMode = axisConfigs.First().mode;
            Master = master;
            syncMode = SMode;
            SlaveAddr = slaveAddr;
            Master.InputHandler += InputHandler;
            Master.OutputHandler += OutputHandler;
            Master.SyncConfigEventHandler += SyncConfigEventHandler;
            Master.PdoConfigEventHandler += PdoConfigEventHandler;
            Master.StartActivityEventHandler += StartActivityEventHandler;
            ushort txcontent = 0x1A00;
            ushort rxcontent = 0x1600;
            _errorCode = AddTxPDOMapping<ushort>(txcontent, 0x603F + offset, 0);
            _controlword = AddRxPDOMapping<ushort>(rxcontent, 0x6040 + offset, 0);
            _statusWord = AddTxPDOMapping<ushort>(txcontent, 0x6041 + offset, 0);
            _modesOperation = AddRxPDOMapping<byte>(rxcontent, 0x6060 + offset, 0);
            _modesOperationDisplay = AddTxPDOMapping<byte>(txcontent, 0x6061 + offset, 0);
            foreach (var item in axisConfigs)
            {
                switch (item.mode)
                {
                    case ModesOperations.PP://轮廓位置
                        _targetPosition = AddRxPDOMapping<int>(item.rxcontent, 0x607A + offset, 0);
                        _profileVelocity = AddRxPDOMapping<uint>(item.rxcontent, 0x6081 + offset, 0);
                        _profileAcceleration = AddRxPDOMapping<uint>(item.rxcontent, 0x6083 + offset, 0);
                        _profileDeceleration = AddRxPDOMapping<uint>(item.rxcontent, 0x6084 + offset, 0);
                        _positionActualValue = AddTxPDOMapping<int>(item.txcontent, 0x6064 + offset, 0);
                        break;
                    case ModesOperations.PV://轮廓速度
                        _profileVelocity = AddRxPDOMapping<uint>(item.rxcontent, 0x6081 + offset, 0);
                        _profileAcceleration = AddRxPDOMapping<uint>(item.rxcontent, 0x6083 + offset, 0);
                        _profileDeceleration = AddRxPDOMapping<uint>(item.rxcontent, 0x6084 + offset, 0);
                        _targetVelocity = AddRxPDOMapping<int>(item.rxcontent, 0x60FF + offset, 0);
                        _velocityActualValue = AddTxPDOMapping<int>(item.txcontent, 0x606C + offset, 0);
                        break;
                    case ModesOperations.PT://轮廓转矩
                        _targetTorque = AddRxPDOMapping<short>(item.rxcontent, 0x6071 + offset, 0);
                        _torqueSlope = AddRxPDOMapping<uint>(item.rxcontent, 0x6087 + offset, 0);
                        _torqueActualValue = AddTxPDOMapping<short>(item.txcontent, 0x6077 + offset, 0);
                        break;
                    case ModesOperations.HM://回零模式
                        break;
                    case ModesOperations.CSP://周期同步位置模式
                        _targetPosition = AddRxPDOMapping<int>(item.rxcontent, 0x607A + offset, 0);
                        _positionActualValue = AddTxPDOMapping<int>(item.txcontent, 0x6064 + offset, 0);
                        break;
                    case ModesOperations.CSV://周期同步速度模式
                        _targetVelocity = AddRxPDOMapping<int>(item.rxcontent, 0x60FF + offset, 0);
                        _velocityActualValue = AddTxPDOMapping<int>(item.txcontent, 0x606C + offset, 0);
                        break;
                    case ModesOperations.CST://周期同步转矩模式
                        _targetTorque = AddRxPDOMapping<short>(item.rxcontent, 0x6071 + offset, 0);
                        _torqueActualValue = AddTxPDOMapping<short>(item.txcontent, 0x6077 + offset, 0);
                        break;
                }
            }

            ModesOperation = (byte)axisConfigs.First().mode;
        }
        /// <summary>
        /// 处理输出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private unsafe void OutputHandler(object sender, EventArgs e)
        {
            byte* output = (byte*)OutputPtr.ToPointer();
            // 遍历输出映射，直接写入 output（加锁）
            lock (_slaveLock)
            {
                int rxOffset = 0;
                foreach (var mapping in _rxPDOs)
                {
                    if (rxOffset + mapping.Size <= OutputLength)
                    {
                        mapping.WriteTo(output, rxOffset);
                    }
                    rxOffset += mapping.Size;
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
            byte* input = (byte*)InputPtr.ToPointer();
            // 遍历输入映射，直接从 input 读取
            int txOffset = 0;
            foreach (var mapping in _txPDOs)
            {
                if (txOffset + mapping.Size <= InputLength)
                {
                    mapping.ReadFrom(input, txOffset);
                }
                txOffset += mapping.Size;
            }
        }

        /// <summary>
        /// 在主站启动时,获取内存地址和长度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartActivityEventHandler(object sender, EventArgs e)
        {
            _slaveNameLazy = Master.GetSlaveName(SlaveAddr);
            InputPtr = Master.GetInputPtr(SlaveAddr, out uint inLen);
            InputLength = inLen;
            OutputPtr = Master.GetOutputPtr(SlaveAddr, out uint outLen);
            OutputLength = outLen;
        }
        /// <summary>
        /// 同步模式配置事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncConfigEventHandler(object sender, EventArgs e)
        {
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
            else if(syncMode == SyncMode.DC)
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
        public RxPDOMapping<T> AddRxPDOMapping<T>(ushort content, int index, int subIndex) where T : unmanaged
        {
            var existing = _rxPDOs.OfType<RxPDOMapping<T>>().FirstOrDefault(p => p.Content == content && p.Index == index && p.SubIndex == subIndex);
            if (existing != null)
            {
                return existing; // 直接返回已有对象，避免重复添加
            }
            var temppdo = new RxPDOMapping<T>(content, index, subIndex);
            _rxPDOs.Add(temppdo);
            return temppdo;
        }
        /// <summary>
        /// 从站发送给主站的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <returns></returns>
        public TxPDOMapping<T> AddTxPDOMapping<T>(ushort content, int index, int subIndex) where T : unmanaged
        {
            var existing = _txPDOs.OfType<TxPDOMapping<T>>().FirstOrDefault(p => p.Content == content && p.Index == index && p.SubIndex == subIndex);
            if (existing != null)
            {
                return existing; // 直接返回已有对象，避免重复添加
            }
            var temppdo = new TxPDOMapping<T>(content, index, subIndex);
            _txPDOs.Add(temppdo);
            return temppdo;
        }
        #endregion
    }
}
namespace EC_NET2_IAxis
{
    /// <summary>
    /// 轴控制接口
    /// </summary>
    public interface IAxis
    {
        /// <summary>
        /// 添加RXPDO
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="content">对象索引寄存器</param>
        /// <param name="index">索引</param>
        /// <param name="subIndex">子索引</param>
        /// <returns>返回一个RxPdo对象,如果在从站IO中处理则可以直接从对象中修改或者获取值</returns>
        RxPDOMapping<T> AddRxPDOMapping<T>(ushort content, int index, int subIndex) where T : unmanaged;
        /// <summary>
        /// 添加TXPDO
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="content">对象索引寄存器</param>
        /// <param name="index">索引</param>
        /// <param name="subIndex">子索引</param>
        /// <returns>返回一个RxPdo对象,如果在从站IO中处理则可以直接从对象中修改或者获取值</returns>
        /// <returns></returns>
        TxPDOMapping<T> AddTxPDOMapping<T>(ushort content, int index, int subIndex) where T : unmanaged;
        /// <summary>
        /// 从站设备的主站接口
        /// </summary>
        IMaster Master { get; }
        /// <summary>
        /// 从站设备的地址
        /// </summary>
        ushort SlaveAddr { get; }
        /// <summary>
        /// 从站设备的名称
        /// </summary>
        string SlaveName { get; }
        /// <summary>
        /// 从站同步模式
        /// </summary>
        SyncMode syncMode {  get; }
        /// <summary>
        /// 从站设备的状态
        /// </summary>
        SlaveState SlaveState { get; }
        /// <summary>
        /// 模式切换
        /// </summary>
        /// <param name="MO"></param>
        void ModeSwitch(ModesOperations MO);
        /// <summary>
        /// 停止轴的运动
        /// </summary>
        /// <param name="deceleration">减速速率</param>
        void Stop(uint deceleration);

        /// <summary>
        /// 暂停轴的运动
        /// </summary>
        /// <param name="deceleration">减速速率</param>
        void Halt(uint deceleration);

        /// <summary>
        /// 重置轴的状态
        /// </summary>
        void Reset();

        /// <summary>
        /// 将轴返回到起始位置
        /// </summary>
        void Home();

        /// <summary>
        /// 打开轴的电源
        /// </summary>
        void PowerOn();

        /// <summary>
        /// 关闭轴的电源
        /// </summary>
        void PowerOff();
        /// <summary>
        /// 周期位置运动
        /// </summary>
        /// <param name="position"></param>
        void CSP_Position(int position);
        /// <summary>
        /// 周期同步速度运动
        /// </summary>
        /// <param name="velocity"></param>
        void CSV_Velocity(int velocity);
        /// <summary>
        /// 周期同步扭矩运动
        /// </summary>
        /// <param name="torque"></param>
        void CST_Torque(short torque);
        /// <summary>
        /// 控制轴移动到绝对位置
        /// </summary>
        /// <param name="position">目标位置</param>
        /// <param name="velocity">速度</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="deceleration">减速度</param>
        /// <param name="immediately">是否立即执行</param>
        void MoveAbsolute(int position, uint velocity, uint acceleration, uint deceleration, bool immediately = false);

        /// <summary>
        /// 控制轴相对当前位置移动
        /// </summary>
        /// <param name="distance">相对距离</param>
        /// <param name="velocity">速度</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="deceleration">减速度</param>
        /// <param name="immediately">是否立即执行</param>
        void MoveRelative(int distance, uint velocity, uint acceleration, uint deceleration, bool immediately = false);

        /// <summary>
        /// 使用扭矩控制轴的运动
        /// </summary>
        /// <param name="targetTorque">目标扭矩</param>
        /// <param name="torqueSlope">扭矩斜坡</param>
        void TorqueControl(short targetTorque, uint torqueSlope);

        /// <summary>
        /// 使用速度控制轴的运动
        /// </summary>
        /// <param name="velocity">目标速度</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="deceleration">减速度</param>
        void VelocityControl(int velocity, uint acceleration, uint deceleration);
    }
}