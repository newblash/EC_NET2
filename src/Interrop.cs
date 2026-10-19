using EC_NET2;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
#pragma warning disable CS8600
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8622
namespace EC_NET2_Interrop
{
    /// <summary>
    /// 用于通过P/Invoke调用EtherCAT库中的函数，实现与EtherCAT设备的底层交互。
    /// </summary>
    public class Interrop
    {
        /// <summary>
        /// 动态库常量
        /// </summary>
        public const string LIBRARY = "soem";
        /// <summary>
        /// EtherCAT设备初始化。
        /// </summary>
        /// <param name="ifaceName"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int EcxInit(string ifaceName);
        /// <summary>
        /// EtherCAT设备配置初始化。
        /// </summary>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxConfigInit(int contextt);
        /// <summary>
        /// EtherCAT设备配置映射。
        /// </summary>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxConfigMap(int contextt, int iosize);
        /// <summary>
        /// EtherCAT设备状态检查。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <param name="reqstate"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort EcxStateCheck(int contextt, ushort slave, SlaveState reqstate, int timeout = 8000000);
        /// <summary>
        /// EtherCAT设备状态读取。
        /// </summary>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxReadState(int contextt);
        /// <summary>
        /// 状态写入
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxWriteState(int context, ushort slave);
        /// <summary>
        /// 发送EtherCAT进程数据。
        /// </summary>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxSendProcessData(int contextt);
        /// <summary>
        /// 接收EtherCAT进程数据。
        /// </summary>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxReceiveProcessData(int contextt, int timeout);
        /// <summary>
        /// EtherCAT设备SDO写操作。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <param name="ca"></param>
        /// <param name="psize"></param>
        /// <param name="p"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxSDOwrite(int contextt, ushort slave, ushort index, byte subIndex, bool ca, int psize, IntPtr p, int timeout = 700000);
        /// <summary>
        /// EtherCAT设备SDO读操作。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <param name="index"></param>
        /// <param name="subindex"></param>
        /// <param name="CA"></param>
        /// <param name="psize"></param>
        /// <param name="p"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxSDOread(int contextt, ushort slave, ushort index, byte subindex, bool CA, ref int psize, IntPtr p, int timeout = 700000);
        /// <summary>
        /// 获取EtherCAT从站的输入字节数。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetEcxSlaveIbytes(int contextt, ushort slave);
        /// <summary>
        /// 获取EtherCAT从站的输出字节数。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetEcxSlaveObytes(int contextt, ushort slave);
        /// <summary>
        /// 获取EtherCAT从站的输入数据。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetSlaveInputsPtr(int contextt, ushort slave);
        /// <summary>
        /// 设置EtherCAT从站的输出数据。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetSlaveOutputsPtr(int contextt, ushort slave);
        /// <summary>
        /// 获取整个从站内存地址,需要根据SOEM的从站结构体自行定义,并解析
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetSlavePtr(int contextt, ushort slave);
        /// <summary>
        /// 关闭EtherCAT设备连接。
        /// <param name="contextt"></param>
        /// </summary>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern void EcxClose(int contextt);
        /// <summary>
        /// 重新配置EtherCAT从站。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EcxReconfigSlave(int contextt, ushort slave, int timeout);
        /// <summary>
        /// 获取EtherCAT从站的状态。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern SlaveState GetEcxSlaveState(int contextt, ushort slave);
        /// <summary>
        /// 获取EtherCAT从站的数量。
        /// </summary>
        /// <param name="contextt"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetEcxSlaveCount(int contextt);
        /// <summary>
        /// 设置EtherCAT从站的状态。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <param name="state"></param>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetEcxSlaveState(int contextt, ushort slave, SlaveState state);
        /// <summary>
        /// 获取EtherCAT从站的名称。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <param name="buf"></param>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetEcxSlaveName(int contextt, ushort slave, byte[] buf);
        /// <summary>
        /// 获取EtherCAT从站的配置地址。
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort GetEcxSlaveConfigadr(int contextt, ushort slave);
        /// <summary>
        /// 获取从站AL状态码
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort GetEcxSlaveALstatuscode(int contextt, ushort slave);

        /// <summary>
        /// 配置DC模式
        /// </summary>
        /// <returns></returns>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool EcxConfigdc(int contextt);
        /// <summary>
        /// 配置DC参数
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <param name="act"></param>
        /// <param name="CyclTime"></param>
        /// <param name="CyclShift"></param>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern void EcxDcsync0(int contextt, ushort slave, bool act, uint CyclTime, int CyclShift);
        /// <summary>
        /// 配置DC参数1
        /// </summary>
        /// <param name="contextt"></param>
        /// <param name="slave"></param>
        /// <param name="act"></param>
        /// <param name="CyclTime0"></param>
        /// <param name="CyclTime1"></param>
        /// <param name="CyclShift"></param>
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern void EcxDcsync01(int contextt, ushort slave, bool act, uint CyclTime0, uint CyclTime1, int CyclShift);
    }
    /// <summary>
    /// 高精度计时器
    /// </summary>
    public class WinTime
    {
        // ========== P/Invoke: 进程/线程优先级 + 亲和性 ==========
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern UIntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetThreadPriority(IntPtr hThread, int nPriority);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetThreadPriorityBoost(IntPtr hThread,
            [MarshalAs(UnmanagedType.Bool)] bool bDisablePriorityBoost);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetPriorityClass(IntPtr hProcess, uint dwPriorityClass);

        // ========== P/Invoke: 未公开 API，用于设置 16-31 实时优先级 ==========
        [DllImport("ntdll.dll")]
        private static extern uint NtSetInformationThread(
            IntPtr hThread,
            int threadInformationClass,
            [In] in int threadInformation,
            UIntPtr threadInformationLength);

        // ========== P/Invoke: 高精度计数 ==========
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        // ========== P/Invoke: 系统时钟精度 ==========
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeEndPeriod(uint uMilliseconds);

        // ========== 常量 ==========
        private const int THREAD_PRIORITY_TIME_CRITICAL = 15;
        private const uint REALTIME_PRIORITY_CLASS = 0x00000100;
        private const int ThreadBasePriority = 3;   // NtSetInformationThread 的 ThreadInformationClass
        private const int RealTimePriorityMax = 31;

        // ========== 进程级优先级提升：一次性 ==========
        private static readonly object _processPrioLock = new object();
        private static volatile bool _processSetToRealtime;

        // ========== 内部状态 ==========
        private readonly long _freq;
        private readonly long _periodTick;
        private long _nextTick;
        private readonly long _spinThresholdTick;
        private long _missedCount;
        private bool _periodRaised;

        /// <summary>实际生效的线程优先级（15 或 16-31，0 表示设置失败）</summary>
        public int ActualPriority { get; private set; }

        /// <summary>进程是否已成功切换到 REALTIME_PRIORITY_CLASS</summary>
        public static bool IsRealtimeProcess => _processSetToRealtime;
        /// <summary>
        /// 错过次数
        /// </summary>
        public long MissedCount => Interlocked.Read(ref _missedCount);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="periodMicroseconds"></param>
        /// <param name="cpuCore"></param>
        /// <param name="useRealTimePriority"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public WinTime(long periodMicroseconds, int cpuCore = -1, bool useRealTimePriority = true)
        {
            if (periodMicroseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(periodMicroseconds));

            // 1. 提升系统时钟精度到 1ms
            try
            {
                timeBeginPeriod(1);
                _periodRaised = true;
            }
            catch { }

            // 2. 设置 CPU 亲和性
            IntPtr hThread = GetCurrentThread();
            if (cpuCore >= 0 && cpuCore < 64)
            {
                try { SetThreadAffinityMask(hThread, new UIntPtr(1UL << cpuCore)); } catch { }
            }

            // 3. 设置线程优先级：先试 31，失败回退 15
            ActualPriority = ConfigureThreadPriority(hThread, useRealTimePriority);

            // 4. 关闭优先级动态提升
            try { SetThreadPriorityBoost(hThread, true); } catch { }

            // 5. QPC 频率和周期
            if (!QueryPerformanceFrequency(out _freq) || _freq <= 0)
                throw new Exception("QueryPerformanceFrequency 失败");

            _periodTick = periodMicroseconds * _freq / 1_000_000L;
            if (_periodTick <= 0) _periodTick = 1;

            _spinThresholdTick = _freq * 2 / 1000;

            if (!QueryPerformanceCounter(out _nextTick))
                throw new Exception("QueryPerformanceCounter 失败");
        }

        // ========== 优先级配置 ==========
        private static int ConfigureThreadPriority(IntPtr hThread, bool useRealTimePriority)
        {
            if (useRealTimePriority)
            {
                EnsureRealtimeProcessPriority();

                if (_processSetToRealtime && TrySetThreadPriority31(hThread))
                {
                    Debug.WriteLine("[WinTime] 线程优先级已设置为 31 (实时)");
                    return RealTimePriorityMax;
                }
            }

            // 回退到标准 TIME_CRITICAL (15)
            try
            {
                if (SetThreadPriority(hThread, THREAD_PRIORITY_TIME_CRITICAL))
                {
                    Debug.WriteLine("[WinTime] 线程优先级已设置为 15 (TIME_CRITICAL)");
                    return THREAD_PRIORITY_TIME_CRITICAL;
                }
            }
            catch { }

            Debug.WriteLine("[WinTime] 线程优先级设置失败");
            return 0;
        }

        private static void EnsureRealtimeProcessPriority()
        {
            if (_processSetToRealtime) return;

            lock (_processPrioLock)
            {
                if (_processSetToRealtime) return;

                try
                {
                    if (SetPriorityClass(GetCurrentProcess(), REALTIME_PRIORITY_CLASS))
                    {
                        _processSetToRealtime = true;
                        Debug.WriteLine("[WinTime] 进程已切换到 REALTIME_PRIORITY_CLASS");
                    }
                    else
                    {
                        int err = Marshal.GetLastWin32Error();
                        Debug.WriteLine($"[WinTime] SetPriorityClass 失败, Win32Error={err}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WinTime] SetPriorityClass 异常: {ex.Message}");
                }
            }
        }

        private static bool TrySetThreadPriority31(IntPtr hThread)
        {
            try
            {
                int priority = RealTimePriorityMax;
                uint status = NtSetInformationThread(
                    hThread,
                    ThreadBasePriority,
                    in priority,
                    (UIntPtr)sizeof(int));

                if (status == 0) return true;

                Debug.WriteLine($"[WinTime] NtSetInformationThread(31) 失败, NTSTATUS=0x{status:X8}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WinTime] NtSetInformationThread 异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 自动对齐下一次延时
        /// </summary>
        public void WaitNext()
        {
            _nextTick += _periodTick;

            while (true)
            {
                if (!QueryPerformanceCounter(out long now))
                    break;

                long remain = _nextTick - now;
                if (remain <= 0) break;
                //完全不停止CPU自旋,精度会更高,加上隔离CPU核心,1000us抖动实测在1us左右
                //if (remain > _spinThresholdTick)
                //    Thread.Sleep(1);
                //else
                //    Thread.SpinWait(64);
            }

            if (QueryPerformanceCounter(out long after))
            {
                long behind = after - _nextTick;
                if (behind > _periodTick)
                {
                    long missedPeriods = behind / _periodTick;
                    Interlocked.Add(ref _missedCount, missedPeriods);
                    //_nextTick += missedPeriods * _periodTick;
                    _nextTick = after + _periodTick - (behind % _periodTick);
                }
            }
        }

        /// <summary>
        /// 取消高精度定时
        /// </summary>
        public void Cleanup()
        {
            if (_periodRaised)
            {
                try { timeEndPeriod(1); } catch { }
                _periodRaised = false;
            }
        }
    }
    /// <summary>
    /// linux自补偿计时器
    /// </summary>
    public class LinuxTime
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Timespec
        {
            public long tv_sec;  // 秒
            public long tv_nsec; // 纳秒
        }
        [DllImport("libc.so.6")]
        static extern int clock_gettime(int clk_id, ref Timespec tp);

        [DllImport("libc.so.6")]
        static extern int clock_nanosleep(int clk_id, int flags, ref Timespec req, IntPtr rem);

        const int CLOCK_MONOTONIC = 1;
        const int TIMER_ABSTIME = 1;

        long _nextNsec;       // 下一个周期目标时间（纳秒）
        long _periodNsec;     // 周期（纳秒）
        long _missedCount;    // 严重落后的次数（用于监控）
        /// <summary>
        /// 
        /// </summary>
        /// <param name="periodMicroseconds"></param>

        public LinuxTime(long periodMicroseconds)
        {
            _periodNsec = periodMicroseconds * 1000L;

            var now = new Timespec();
            clock_gettime(CLOCK_MONOTONIC, ref now);
            _nextNsec = now.tv_sec * 1_000_000_000L + now.tv_nsec;

        }

        /// <summary>阻塞到下一个周期点（绝对时间递增，不累积漂移）</summary>
        public void WaitNext1()
        {
            // 关键：在上一次的"目标时刻"上递增，不是从"现在"起算
            _nextNsec += _periodNsec;

            var target = new Timespec
            {
                tv_sec = _nextNsec / 1_000_000_000L,
                tv_nsec = _nextNsec % 1_000_000_000L
            };

            clock_nanosleep(CLOCK_MONOTONIC, TIMER_ABSTIME, ref target, IntPtr.Zero);

            // 检测严重落后：如果处理耗时超过 1 个周期
            var after = new Timespec();
            clock_gettime(CLOCK_MONOTONIC, ref after);
            long afterNsec = after.tv_sec * 1_000_000_000L + after.tv_nsec;

            if (afterNsec - _nextNsec > _periodNsec)
            {
                // 落后超过一个周期，说明出现了调度/处理的极端延迟，重置基线
                _missedCount++;
                long behind = afterNsec - _nextNsec;
                long missedPeriods = behind / _periodNsec;

                _nextNsec += missedPeriods * _periodNsec;  // 跳到最近的周期点
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void WaitNext()
        {
            // 1. 推进一个周期
            _nextNsec += _periodNsec;

            // 2. 读取当前时间
            var now = new Timespec();
            clock_gettime(CLOCK_MONOTONIC, ref now);
            long nowNsec = now.tv_sec * 1_000_000_000L + now.tv_nsec;

            // 3. 关键修改：如果已经超过本轮周期点（迟到），
            //    不累积补偿，而是直接跳到未来第一个绝对周期点，保持相位一致。
            if (_nextNsec < nowNsec)
            {
                long behind = nowNsec - _nextNsec;

                // 统计错过多少个周期（用于监控，不影响调度）
                long missedPeriods = behind / _periodNsec + 1;
                Interlocked.Add(ref _missedCount, missedPeriods);

                // 保留相位：用 behind % _periodNsec 计算余数，
                // 让 _nextNsec 落在绝对时间网格的未来点上
                long remainder = behind % _periodNsec;
                _nextNsec = nowNsec + (_periodNsec - remainder);
            }

            // 4. 睡眠到目标时刻（保持绝对时间对齐）
            var target = new Timespec
            {
                tv_sec = _nextNsec / 1_000_000_000L,
                tv_nsec = _nextNsec % 1_000_000_000L
            };

            clock_nanosleep(CLOCK_MONOTONIC, TIMER_ABSTIME, ref target, IntPtr.Zero);
        }
    }
}
