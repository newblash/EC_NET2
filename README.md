# EC_Net2

## 简介 Introduction
### EC_Net2是SOEM包装器,用于win和linux控制EtherCat设备
### EC_Net2 is SOEM wrapper, used for controlling EtherCat devices on Windows and Linux
### 交流群:1138063413(加群问题请输入nuget)
### 

### 更新内容
```
版本2026.9.28.1050 优化定时器逻辑,修改为绝对时间定时,当定时器不准时直接放弃当次任务,跳到下一个周期继续运行,避免极端周期内连发多条报文,造成DC同步看门狗触发
版本2026.9.17.1818 取消linux定时器的优先级设置,否则会造成linux线程调度与系统中断冲突,导致系统假死
版本2026.9.16.1818 修复linux下定时器可能会产生漂移的bug,修改windows下定时器为高精度自旋,支持iot版本31级优先级(需要用CPU隔离工具进行CPU隔离核心和中断),修复S曲线加速度限制导致出现加速度尖角异常的bug
版本2026.8.28.1201 自动添加unsafe支持,将100次PDO通讯后切换OP模式改为10次避免长时间等待
版本2026.8.8.1807  修复windows下AOT编译,定时器崩溃的bug
版本2026.8.8.1516  改为源码包提供,不再提供dll,修复自由模式运行时1C32和1C33写入错误的bug
版本2026.8.7.1009  修改停止时,不自动恢复使能状态,否则速度模式和扭矩模式下停止没意义的bug,修复位置,速度,扭矩,下发时控制字不是15的bug
版本2026.7.30.1000 修复在windows下停止主站时报异常的bug
版本2026.7.29.1437 将自旋定时器修改为windows多媒体定时器，避免中断导致周期时间跳过
版本2026.7.27.1837 增加运动控制算法,S型路径规划和T型路径规划
版本2026.7.18.1518 将pdo数据读写事件分成了2个,首先写入数据,pdo读写,然后读取数据,每次写入和读取到的数据都是同步最新的,之前版本写入会延迟一个周期写入
版本2026.7.17.1850 精简SDO读写,使用unsafe方法直接通过内存写入和读取,避免引用分配
版本2026.7.16.1022 修复传入相同的PDO映射,导致数据错误,多模式下写入寄存器错误的bug
版本2026.7.15.1558 修改网卡查找策略(已弃用)
版本2026.7.14.1000 修复bug
版本2026.7.13.1724 修复切换模式时速度和扭矩不为0的bug(已弃用)
版本2026.7.13.1655 添加Axis类的重载,支持多模式基础映射(已弃用)
版本2026.7.11.1717 将框架改成了netstandard2.0,为了获取极致性能,解析从站数据由span修改成了直接操作内存指针,如果要自己做通用从站,请参考底部的Slave代码
版本2026.7.11.1150 移除setoutputs和getinputs,新增SlaveALstatuscode,新增AL状态中文扩展,新增GetSlavePtr方法,需要自行根据SOEM的从站结构体解析
版本2026.7.9.1031 将DC同步改为由从站设置,不再由主站统一管理,避免有些IO模块与伺服不兼容导致IO模块不能进OP模式的bug
版本2026.7.7.1405 彻底解耦从站与主站,从站自己管理自己的PDO数据或者收发数据,主站不再持有任何从站数据,从站只需要调用主站方法即可
```
## 初始化主站
```
Master master = new Master(); 
//初始化1个地址为1的驱动器,如果有多个驱动器直接创建对应地址的多个对象即可
Axis axis = new Axis(master,SyncMode.DC, 1,0,EC_Net.Slave.ModesOperations.PP);
//通过多寄存器来映射运行模式,注意这个只提供了最小的通用PDO(请参考轴控最后面的基础PDO映射内容),如果驱动器需要其他PDO参数请自行添加
Axis axis = new Axis(null, SyncMode.FreeRun, 1, 0, new AxisConfigs[] { new AxisConfigs(ModesOperations.PP,0x1A00,0x1600), new AxisConfigs(ModesOperations.PV, 0x1A01, 0x1601), new AxisConfigs(ModesOperations.PT, 0x1A02, 0x1602) });
//初始化IO从站,注意这个从站只能自己控制byte[]里面的数据
Slave slave = new Slave(master,SyncMode.FreeRun, 2);
//添加PDO方法,清务必在启动主站之前添加,否则会报错,添加请参照伺服说明书添加
//自行添加清使用1A01~4 1601~4地址,或参照xml文件添加,部分伺服只支持1个通道的情况下全部使用1A00和1600即可,避免映射失败
TxPDOMapping<ushort> _errorCode = axis.AddTxPDOMapping<ushort>(txcontent, 0x603F + offset, 0);
RxPDOMapping<ushort> _controlword = axis.AddRxPDOMapping<ushort>(rxcontent, 0x6040 + offset, 0);
//启动主站,查找从站数量,参数为网卡名称,同步模式,同步周期us,shift时间us,windows下绑定CPU核心
int Count = master.StartActivity("EtherCatNet",1000,0,0);
//停止主站
master.StopActivity();
```
## 轴控方法
```
//伺服使能
axis.PowerOn();
//伺服去使能
axis.PowerOff();
//周期同步位置运动
axis.CSP_Position(postion)
//周期同步扭矩运动
axis.CST_Torque(100)
//周期同步速度
axis.CSV_Velocity(100)
//绝对位置运动参数为: 位置 速度 加速度 减速度(单位:Puls)
axis.MoveAbsolute(postion, speed, speed * 10, speed * 10);
//相对位置运动,参数同上
axis.MoveRelative(postion, speed, speed * 10, speed * 10);
//扭矩控制,目标扭矩,扭矩斜率
axis.TorqueControl(1000,100);//扭矩控制,单位为百分比
//速度控制,目标速度,加速度,减速度
axis.VelocityControl(1000, 100, 100);
//伺服复位
axis.Reset();
//伺服停止
axis.Stop(1000000);// 停止减速度
//伺服启动回零
axis.Home();
//写入寄存器,<写入类型>参数:驱动器地址,索引,子索引,值
axis.Master.WriteSDO<byte>(1, 0x6060, 0x00, 6);
//读寄存器,<读取类型>参数:驱动器地址,索引,子索引
axis.Master.ReadSDO<int>(1, 0x6060, 0x00);
//切换模式,在内部处理了位置,速度,扭矩等信息,直接切换即可,不用断使能
axis.ModeSwitch(ModesOperations MO);

//初始化完成后可读取参数
 从站名:_axis.SlaveName
 从站地址:_axis.SlaveAddr
 从站状态:_axis.SlaveState
 报警代码:_axis.ErrorCode
 状态字:_axis.StatusWord
 当前模式:_axis.ModesOperationDisplay
 当前位置:_axis.PositionActualValue
 当前速度:_axis.VelocityActualValue
 控制字:_axis.Controlword
 设置模式:_axis.ModesOperation
 设置位置:_axis.TargetPosition
 设置速度:_axis.ProfileVelocity
 设置加速度:_axis.ProfileAcceleration
 设置减速度:_axis.ProfileDeceleration
 快速停机:_axis.QuickOptionCode
 停止代码:_axis.HaltOptionCode
 伺服无故障:_axis.AxisState.ReadyToSwitchOn
 等待打开伺服使能:_axis.AxisState.SwitchedOn
 伺服运行:_axis.AxisState.OperationEnabled
 故障:_axis.AxisState.Fault
 接通主回路电:_axis.AxisState.VoltageEnabled
 快速停机:_axis.AxisState.QuickStop
 伺服准备好:_axis.AxisState.SwitchOnDisabled
 警告:_axis.AxisState.Warning
 远程控制:_axis.AxisState.Remote
 目前到达:_axis.AxisState.TargetReached
 内部软限位状态:_axis.AxisState.InternalLimitActive
 回原点完成输出:_axis.AxisState.HomingAttained

 //Axis类中基础PDO映射部分,没有的需要自己添加
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
```

## IO控制
```
if (slave is not null && slave.Outputs is not null && slave.Inputs is not null)
{
        //读取
        slave.Inputs[0]
        slave.Inputs[1]
        //写入
        slave.Outputs.SetBit(0, 0, true);
        slave.Outputs.SetBit(1, 0, true);
        //或者整体写入
        slave.Outputs[0] = 0xFF;
        slave.Outputs[0] = 0x0;
}
```
## 通用从站,如果你不想用已经写好的轴或者从站,可以使用通用从站来创建你自己的从站代码
```
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
                Master.WriteSDO(SlaveAddr, 0x1C32, 1, (ushort)0);
                Master.WriteSDO(SlaveAddr, 0x1C33, 1, (ushort)0);
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

```
## 伺服扩展算法
```
//S型扩展静态类(T型使用TrapezoidalPlanner静态类,其他方法签名一模一样)
//设置电机参数
SCurvePlanner.SetMotorParams(new MotorParams() { EncoderResolution = 131072, AccelTime = 0.5, MaxRPM = 3000, JerkRatio = 5 });
//S型点到点规划
var result = SCurvePlanner.Generate(0, 10000000, 0.001);
//2轴S直线插补
result = SCurvePlanner.LinearInterpolate(0, 0,100,100, 0.001);
//3轴S直线插补(重载)
result = SCurvePlanner.LinearInterpolate(0, 0, 0, 100, 100, 100, 0.001);

//2轴圆弧插补
result = SCurvePlanner.ArcInterpolate(0, 0, 100, 100, Math.PI, 0.001);
//2轴多圈圆弧插补
result = SCurvePlanner.ArcInterpolateMultiTurn(0, 0, 100, 100, 5, 1, 0.001);

//3轴螺旋插补
result = SCurvePlanner.HelicalInterpolate(0, 0, 0, 100, 100, Math.PI, 100, 0.001);
//3轴多圈螺旋插补
result = SCurvePlanner.HelicalInterpolate(0, 0, 0, 100, 100, 5, 100, 1, 0.001);

//其他方法请自行探索,懒得写了,太多了
```