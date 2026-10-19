using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EC_NET2_EX
{
    /// <summary>
    /// AL状态码扩展
    /// </summary>
    public static class AlStatusCodeExtensions
    {
        /// <summary>
        /// 获取AL状态码说明
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetAlStatusDescription(this ushort code)
        {
            switch (code)
            {
                case 0x0000: return "无错误";
                case 0x0001: return "未指定错误";
                case 0x0002: return "内存不足";
                case 0x0011: return "无效的状态改变请求";
                case 0x0012: return "未知的请求状态";
                case 0x0013: return "不支持 Bootstrap 模式";
                case 0x0014: return "无效的 Init 配置";
                case 0x0015: return "无效的邮箱配置（Init→PreOp）";
                case 0x0016: return "无效的邮箱配置（另一情况）";
                case 0x0017: return "同步管理器配置无效";
                case 0x0018: return "无效的输入配置";
                case 0x0019: return "无效的输出配置";
                case 0x001A: return "同步错误";
                case 0x001B: return "同步管理器看门狗超时";
                case 0x001C: return "无效的同步管理器类型";
                case 0x001D: return "无效的输出配置（SafeOp→Op）";
                case 0x001E: return "无效的输入配置（PreOp→SafeOp 或 SafeOp→Op）";
                case 0x001F: return "无效的看门狗配置";
                case 0x0020: return "从站需要外部启动";
                case 0x0021: return "从站需要 INIT 状态";
                case 0x0022: return "从站需要 PREOP 状态";
                case 0x0023: return "从站需要 SAFEOP 状态";
                case 0x0024: return "无效的输入映射";
                case 0x0025: return "无效的输出映射";
                case 0x0026: return "不连贯的设定";
                case 0x0027: return "不支持 Free Run 模式";
                case 0x0028: return "不支持同步模式";
                case 0x0029: return "Free Run 需要 3 个缓冲模式";
                case 0x002A: return "跨层看门狗";
                case 0x002B: return "无效的 IO 配置";
                case 0x002C: return "严重的同步错误";
                case 0x002D: return "无效的 DC SYNCH 配置";
                case 0x0030: return "无效的 DC 门锁配置";
                case 0x0031: return "PLL 错误";
                case 0x0032: return "无效的 DC IO 错误";
                case 0x0033: return "无效的 DC 通信错误";
                case 0x0034: return "无效的 DC 同步周期时间";
                case 0x0035: return "DC Sync0 周期时间无效";
                case 0x0036: return "DC Sync1 周期时间无效";
                case 0x0037: return "DC 门锁配置无效";
                case 0x0041: return "AoE 邮箱错误";
                case 0x0042: return "EoE 邮箱错误";
                case 0x0043: return "CoE 邮箱错误";
                case 0x0044: return "FoE 邮箱错误";
                case 0x0045: return "SoE 邮箱错误";
                case 0x0050: return "EEPROM 无存取";
                case 0x0051: return "EEPROM 错误";
                case 0x0060: return "从站在本地重启";
                default:
                    // 处理供应商特定范围
                    if (code >= 0x8000 && code <= 0xFFFF)
                        return "供应商特定错误";
                    return $"未知状态码 (0x{code:X4})";
            }
        }
    }
    /// <summary>
    /// 通用扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 直接修改引用数组中的值
        /// </summary>
        /// <param name="outputs"></param>
        /// <param name="index"></param>
        /// <param name="bit"></param>
        /// <param name="value"></param>
        public static byte[] SetBit(this byte[] outputs, int index, int bit, bool value)
        {
            if (index < 0 || index >= outputs.Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} out of range [0, {outputs.Length})");
            if (bit < 0 || bit > 7)
                throw new ArgumentOutOfRangeException(nameof(bit), $"Bit {bit} must be 0-7");
            if (value)
                outputs[index] |= (byte)(1 << bit);
            else
                outputs[index] &= (byte)~(1 << bit);
            return outputs;
        }
        /// <summary>
        /// 获取指定数组中指定位的值时0或者1
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="bit"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool GetBit(this byte[] buffer, int index, int bit)
        {
            if ((uint)index >= (uint)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} out of range [0, {buffer.Length})");
            if ((uint)bit > 7)
                throw new ArgumentOutOfRangeException(nameof(bit), $"Bit {bit} must be 0-7");
            return (buffer[index] & (1 << bit)) != 0;
        }
        /// <summary>
        /// 获取指定类型中的位的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static unsafe bool GetBit<T>(this T value, int index, bool isLittleEndian = true) where T : unmanaged
        {
            if (index < 0 || index >= sizeof(T) * 8)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte* p = (byte*)&value;
            int byteIndex = isLittleEndian ? index / 8 : sizeof(T) - 1 - index / 8;
            int bitOffset = index % 8;
            return ((p[byteIndex] >> bitOffset) & 1) != 0;
        }
        /// <summary>
        /// 设置指定类型数据中位的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="flag"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static unsafe T SetBit<T>(this T value, int index, bool flag, bool isLittleEndian = true) where T : unmanaged
        {
            if (index < 0 || index >= sizeof(T) * 8)
                throw new ArgumentOutOfRangeException(nameof(index));

            T result = value;
            byte* p = (byte*)&result;
            int byteIndex = isLittleEndian ? index / 8 : sizeof(T) - 1 - index / 8;
            int bitOffset = index % 8;

            if (flag)
                p[byteIndex] |= (byte)(1 << bitOffset);
            else
                p[byteIndex] &= (byte)~(1 << bitOffset);

            return result;
        }
    }
    /// <summary>
    /// 规划路径点
    /// </summary>
    public struct TrajectoryPoint
    {
        /// <summary>
        /// 规划时间点
        /// </summary>
        public double Time { get; set; }
        /// <summary>
        /// 规划位置
        /// </summary>
        public double Position { get; set; }
        /// <summary>
        /// 规划当前速度
        /// </summary>
        public double Velocity { get; set; }
        /// <summary>
        /// 规划当前加速度
        /// </summary>
        public double Acceleration { get; set; }
    }
    /// <summary>
    /// 规划结果
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 规划是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; } = string.Empty;
        /// <summary>
        /// 规划总时间
        /// </summary>
        public double TotalTime { get; set; }
        /// <summary>
        /// 规划最大速度
        /// </summary>
        public double MaxVelocity { get; set; }
        /// <summary>
        /// 规划最大加速度
        /// </summary>
        public double MaxAcceleration { get; set; }
        /// <summary>
        /// 规划点位总数
        /// </summary>
        public int PointCount { get; set; }
        /// <summary>
        /// 最大加加速度
        /// </summary>
        public double MaxJerk { get; set; }
        /// <summary>
        /// 轴1规划点
        /// </summary>
        public IList<TrajectoryPoint> Axis1Points { get; set; } = new List<TrajectoryPoint>();
        /// <summary>
        /// 轴2规划点
        /// </summary>
        public IList<TrajectoryPoint> Axis2Points { get; set; } = new List<TrajectoryPoint>();
        /// <summary>
        /// 轴3规划点
        /// </summary>
        public IList<TrajectoryPoint> Axis3Points { get; set; } = new List<TrajectoryPoint>();
    }
    /// <summary>
    /// 电机参数
    /// </summary>
    public struct MotorParams
    {
        /// <summary>
        /// 每圈编码器单位
        /// </summary>
        public double EncoderResolution { get; set; }
        /// <summary>
        /// 电机最大速度(转/分钟)
        /// </summary>
        public double MaxRPM { get; set; }
        /// <summary>
        /// 加速减速时间
        /// </summary>
        public double AccelTime { get; set; }
        /// <summary>
        /// 加加速比例,值越小越平滑,越大越接近梯形
        /// </summary>
        public double JerkRatio { get; set; }
    }
    /// <summary>
    /// S型曲线路径规划器
    /// </summary>
    public static class SCurvePlanner
    {
        #region S单轴路径规划
        /// <summary>
        /// 默认电机参数
        /// </summary>
        private static MotorParams _defaultMotor = new MotorParams() { EncoderResolution = 131072, MaxRPM = 3000, AccelTime = 0.5, JerkRatio = 5 };
        /// <summary>
        /// 设置默认电机参数
        /// </summary>
        /// <param name="motor"></param>
        public static void SetMotorParams(MotorParams motor) => _defaultMotor = motor;

        // ------------------- 公开接口 -------------------
        /// <summary>
        /// 点到点S路径规划(起始点,结束点,周期)
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="dt">伺服周期</param>
        /// <returns>返回规划结果</returns>
        public static Result Generate(double startPos, double targetPos, double dt = 0.001) => Generate(startPos, targetPos, _defaultMotor, dt);
        /// <summary>
        /// 点到点S路径规划(起始点,结束点,电机参数,周期)
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="motor">电机参数</param>
        /// <param name="dt">伺服周期</param>
        /// <returns>返回规划结果</returns>
        public static Result Generate(double startPos, double targetPos, MotorParams motor, double dt = 0.001)
        {
            double distance = Math.Abs(targetPos - startPos);
            if (distance < 1) distance = 1;

            // 理论最大速度（脉冲/秒）
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double maxAccel = maxVel / motor.AccelTime;
            double maxJerk = maxAccel * motor.JerkRatio;

            // ---------- 关键修正：根据距离自动调整速度 ----------
            // 使用公式：安全速度 = sqrt(distance * maxAccel * 0.5)
            // 保证加速段和减速段总位移不超过总距离
            double safeVel = Math.Sqrt(distance * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10); // 至少10
                                                                  // 重新计算加速度和加加速度
            maxAccel = maxVel / motor.AccelTime;
            maxJerk = maxAccel * motor.JerkRatio;

            // 硬上限（防止极端值）
            maxVel = Math.Min(maxVel, maxVel * 1.2);
            maxAccel = Math.Min(maxAccel, maxVel * 100);
            maxJerk = Math.Min(maxJerk, maxAccel * 100);

            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            return GenerateInternal(startPos, targetPos, 0, 0, maxVel, maxAccel, maxJerk, dt);
        }
        /// <summary>
        /// 点到点S路径规划(起始点,结束点,最大速度,最大加速度,最大加加速度,周期)
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="maxVel">最大速度</param>
        /// <param name="maxAccel">最大加速度</param>
        /// <param name="maxJerk">最大加加速</param>
        /// <param name="dt">伺服周期</param>
        /// <returns>返回规划结果</returns>
        public static Result Generate(double startPos, double targetPos, double maxVel, double maxAccel, double maxJerk, double dt = 0.001)
        {
            double distance = Math.Abs(targetPos - startPos);
            if (distance < 1) distance = 1;
            double safeVel = Math.Sqrt(distance * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 1);
            maxAccel = Math.Min(maxAccel, maxVel * 100);
            maxJerk = Math.Min(maxJerk, maxAccel * 100);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);
            return GenerateInternal(startPos, targetPos, 0, 0, maxVel, maxAccel, maxJerk, dt);
        }
        /// <summary>
        /// 点到点S路径规划(起始点,结束点,起始速度,结束速度,最大速度,最大加速度,最大加加速度,周期)
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="startVel">起始速度</param>
        /// <param name="endVel">结束速度</param>
        /// <param name="maxVel">最大速度</param>
        /// <param name="maxAccel">最大加速度</param>
        /// <param name="maxJerk">最大加加速</param>
        /// <param name="dt">伺服周期</param>
        /// <returns>返回规划结果</returns>
        public static Result Generate(double startPos, double targetPos, double startVel, double endVel, double maxVel, double maxAccel, double maxJerk, double dt = 0.001)
        {
            double distance = Math.Abs(targetPos - startPos);
            if (distance < 1) distance = 1;
            double safeVel = Math.Sqrt(distance * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 1);
            maxAccel = Math.Min(maxAccel, maxVel * 100);
            maxJerk = Math.Min(maxJerk, maxAccel * 100);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);
            return GenerateInternal(startPos, targetPos, startVel, endVel, maxVel, maxAccel, maxJerk, dt);
        }

        // ------------------- 内部核心算法 -------------------
        /// <summary>
        /// S核心规划算法
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="startVel">起始速度</param>
        /// <param name="endVel">结束速度</param>
        /// <param name="maxVel">最大速度</param>
        /// <param name="maxAccel">最大加速度</param>
        /// <param name="maxJerk">最大加加速</param>
        /// <param name="dt">伺服周期</param>
        /// <returns>返回规划结果</returns>
        private static Result GenerateInternal(double startPos, double targetPos, double startVel, double endVel, double maxVel, double maxAccel, double maxJerk, double dt)
        {
            var result = new Result();

            double delta = targetPos - startPos;
            double absDelta = Math.Abs(delta);
            double dir = delta >= 0 ? 1.0 : -1.0;

            if (absDelta < 1e-10)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.MaxVelocity = 0;
                result.MaxAcceleration = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startPos, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // ---------- 计算各段时间 ----------
            double t1, t2, t3, t4, t5, t6, t7;
            double vMax = maxVel;

            // 尝试计算加速段
            t1 = maxAccel / maxJerk;
            t2 = (vMax - startVel) / maxAccel - t1;
            if (t2 < 0)
            {
                t1 = Math.Sqrt(Math.Max((vMax - startVel) / maxJerk, 0));
                t2 = 0;
            }

            // 减速段
            t5 = maxAccel / maxJerk;
            t6 = (vMax - endVel) / maxAccel - t5;
            if (t6 < 0)
            {
                t5 = Math.Sqrt(Math.Max((vMax - endVel) / maxJerk, 0));
                t6 = 0;
            }

            // 加速段位移
            double accelDist = (startVel + vMax) / 2.0 * (t1 + t2 + t1);
            double decelDist = (vMax + endVel) / 2.0 * (t5 + t6 + t5);
            t4 = (absDelta - accelDist - decelDist) / vMax;

            // 如果 t4 < 0，说明无法达到设定速度，逐步降低 vMax 直到 t4 >= 0
            int iter = 0;
            while (t4 < 0 && iter < 100)
            {
                vMax *= 0.95;                       // 每次降低5%
                t1 = maxAccel / maxJerk;
                t2 = (vMax - startVel) / maxAccel - t1;
                if (t2 < 0)
                {
                    t1 = Math.Sqrt(Math.Max((vMax - startVel) / maxJerk, 0));
                    t2 = 0;
                }
                t5 = maxAccel / maxJerk;
                t6 = (vMax - endVel) / maxAccel - t5;
                if (t6 < 0)
                {
                    t5 = Math.Sqrt(Math.Max((vMax - endVel) / maxJerk, 0));
                    t6 = 0;
                }
                accelDist = (startVel + vMax) / 2.0 * (t1 + t2 + t1);
                decelDist = (vMax + endVel) / 2.0 * (t5 + t6 + t5);
                t4 = (absDelta - accelDist - decelDist) / vMax;
                iter++;
            }

            // 极端情况：仍然为负，则强制设为 0，并调整 vMax
            if (t4 < 0)
            {
                t4 = 0;
                vMax = Math.Sqrt(absDelta * maxAccel);
                if (vMax > maxVel) vMax = maxVel;
                // 重新计算
                t1 = maxAccel / maxJerk;
                t2 = (vMax - startVel) / maxAccel - t1;
                if (t2 < 0) { t1 = Math.Sqrt(Math.Max((vMax - startVel) / maxJerk, 0)); t2 = 0; }
                t5 = maxAccel / maxJerk;
                t6 = (vMax - endVel) / maxAccel - t5;
                if (t6 < 0) { t5 = Math.Sqrt(Math.Max((vMax - endVel) / maxJerk, 0)); t6 = 0; }
            }

            // 确保非负
            t1 = Math.Max(t1, 0);
            t2 = Math.Max(t2, 0);
            t3 = t1;
            t4 = Math.Max(t4, 0);
            t5 = Math.Max(t5, 0);
            t6 = Math.Max(t6, 0);
            t7 = t5;

            double totalTime = t1 + t2 + t3 + t4 + t5 + t6 + t7;
            if (totalTime <= 0 || double.IsNaN(totalTime) || double.IsInfinity(totalTime))
            {
                // 异常回退
                result.Success = true;
                result.TotalTime = 0.001;
                result.MaxVelocity = 0;
                result.MaxAcceleration = 0;
                result.PointCount = 2;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startPos, Velocity = 0, Acceleration = 0 });
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0.001, Position = targetPos, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // ---------- 采样 ----------
            int minPoints = 50;
            int maxPoints = 100000;
            int sampleCount = (int)(totalTime / dt) + 2;
            sampleCount = Math.Max(sampleCount, minPoints);
            sampleCount = Math.Min(sampleCount, maxPoints);
            double actualDt = totalTime / (sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                double time = i * actualDt;
                if (time > totalTime) time = totalTime;
                result.Axis1Points.Add(GetPointAtTime(
                    time,
                    startPos, targetPos,
                    startVel, endVel,
                    vMax, maxAccel, maxJerk,
                    dir,
                    t1, t2, t3, t4, t5, t6, t7,
                    totalTime
                ));
            }

            // 确保终点
            var last = result.Axis1Points[result.Axis1Points.Count - 1];
            if (Math.Abs(last.Position - targetPos) > 0.001)
                result.Axis1Points.Add(GetPointAtTime(
                    totalTime,
                    startPos, targetPos,
                    startVel, endVel,
                    vMax, maxAccel, maxJerk,
                    dir,
                    t1, t2, t3, t4, t5, t6, t7,
                    totalTime
                ));

            result.Success = true;
            result.TotalTime = totalTime;
            result.MaxVelocity = vMax;
            result.MaxAcceleration = maxAccel;
            result.PointCount = result.Axis1Points.Count;
            return result;
        }

        // ------------------- 单点计算（分段累加，避免溢出） -------------------
        /// <summary>
        /// S单点规划算法
        /// </summary>
        /// <returns>返回单个点位置数据</returns>
        private static TrajectoryPoint GetPointAtTime(double time, double startPos, double targetPos, double startVel, double endVel, double maxVel, double maxAccel, double maxJerk, double dir, double t1, double t2, double t3, double t4, double t5, double t6, double t7, double totalTime)
        {
            var pt = new TrajectoryPoint { Time = time };

            if (time <= 0)
            {
                pt.Position = startPos;
                pt.Velocity = startVel;
                pt.Acceleration = 0;
                return pt;
            }

            if (time >= totalTime)
            {
                pt.Position = targetPos;
                pt.Velocity = endVel;
                pt.Acceleration = 0;
                return pt;
            }

            // 从起始状态开始，逐段累加
            double curPos = startPos;
            double curVel = startVel;
            double curAccel = 0;
            double remaining = time;

            // 阶段1：加加速
            double seg1 = Math.Min(remaining, t1);
            if (seg1 > 0)
            {
                double jerk = dir * maxJerk;
                double dt = seg1;
                curPos += curVel * dt + 0.5 * curAccel * dt * dt + (1.0 / 6.0) * jerk * dt * dt * dt;
                curVel += curAccel * dt + 0.5 * jerk * dt * dt;
                curAccel += jerk * dt;
                remaining -= seg1;
            }
            if (remaining <= 0) goto done;

            // 阶段2：匀加速
            double seg2 = Math.Min(remaining, t2);
            if (seg2 > 0)
            {
                double jerk = 0;
                double dt = seg2;
                curPos += curVel * dt + 0.5 * curAccel * dt * dt + (1.0 / 6.0) * jerk * dt * dt * dt;
                curVel += curAccel * dt + 0.5 * jerk * dt * dt;
                curAccel += jerk * dt;
                remaining -= seg2;
            }
            if (remaining <= 0) goto done;

            // 阶段3：减加速
            double seg3 = Math.Min(remaining, t3);
            if (seg3 > 0)
            {
                double jerk = -dir * maxJerk;
                double dt = seg3;
                curPos += curVel * dt + 0.5 * curAccel * dt * dt + (1.0 / 6.0) * jerk * dt * dt * dt;
                curVel += curAccel * dt + 0.5 * jerk * dt * dt;
                curAccel += jerk * dt;
                remaining -= seg3;
            }
            if (remaining <= 0) goto done;

            // 阶段4：匀速
            double seg4 = Math.Min(remaining, t4);
            if (seg4 > 0)
            {
                double jerk = 0;
                double dt = seg4;
                curPos += curVel * dt + 0.5 * curAccel * dt * dt + (1.0 / 6.0) * jerk * dt * dt * dt;
                curVel += curAccel * dt + 0.5 * jerk * dt * dt;
                curAccel += jerk * dt;
                remaining -= seg4;
            }
            if (remaining <= 0) goto done;

            // 阶段5：加减速
            double seg5 = Math.Min(remaining, t5);
            if (seg5 > 0)
            {
                double jerk = -dir * maxJerk;
                double dt = seg5;
                curPos += curVel * dt + 0.5 * curAccel * dt * dt + (1.0 / 6.0) * jerk * dt * dt * dt;
                curVel += curAccel * dt + 0.5 * jerk * dt * dt;
                curAccel += jerk * dt;
                remaining -= seg5;
            }
            if (remaining <= 0) goto done;

            // 阶段6：匀减速
            double seg6 = Math.Min(remaining, t6);
            if (seg6 > 0)
            {
                double jerk = 0;
                double dt = seg6;
                curPos += curVel * dt + 0.5 * curAccel * dt * dt + (1.0 / 6.0) * jerk * dt * dt * dt;
                curVel += curAccel * dt + 0.5 * jerk * dt * dt;
                curAccel += jerk * dt;
                remaining -= seg6;
            }
            if (remaining <= 0) goto done;

            // 阶段7：减减速
            double seg7 = Math.Min(remaining, t7);
            if (seg7 > 0)
            {
                double jerk = dir * maxJerk;
                double dt = seg7;
                curPos += curVel * dt + 0.5 * curAccel * dt * dt + (1.0 / 6.0) * jerk * dt * dt * dt;
                curVel += curAccel * dt + 0.5 * jerk * dt * dt;
                curAccel += jerk * dt;
                remaining -= seg7;
            }

        done:
            pt.Position = curPos;
            pt.Velocity = curVel;
            pt.Acceleration = curAccel;
            return pt;
        }

        #endregion

        #region 2轴直线插补
        /// <summary>
        /// 2轴直线插补（起始X,起始Y,结束X,结束Y,周期）
        /// </summary>
        /// <returns>返回规划数据</returns>
        public static Result LinearInterpolate(double startX, double startY, double targetX, double targetY, double dt = 0.001)
        {
            return LinearInterpolate(startX, startY, targetX, targetY, _defaultMotor, dt);
        }
        /// <summary>
        /// 2轴直线插补（起始X,起始Y,结束X,结束Y,电机参数,周期）
        /// </summary>
        ///  <returns>返回规划数据</returns>
        public static Result LinearInterpolate(double startX, double startY, double targetX, double targetY, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            double dx = targetX - startX;
            double dy = targetY - startY;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算电机参数
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double maxAccel = maxVel / motor.AccelTime;
            double maxJerk = maxAccel * motor.JerkRatio;

            // 短距离安全降速
            double safeVel = Math.Sqrt(distance * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            maxAccel = maxVel / motor.AccelTime;
            maxJerk = maxAccel * motor.JerkRatio;

            // 硬上限
            maxVel = Math.Min(maxVel, maxVel * 1.2);
            maxAccel = Math.Min(maxAccel, maxVel * 10);
            maxJerk = Math.Min(maxJerk, maxAccel * 10);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            // 生成沿路径方向的S型轨迹
            var singleResult = Generate(0, distance, 0, 0, maxVel, maxAccel, maxJerk, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 映射到X、Y轴
            double cosA = dx / distance;
            double sinA = dy / distance;

            foreach (var point in singleResult.Axis1Points)
            {
                var xPt = new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startX + point.Position * cosA,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                };

                var yPt = new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startY + point.Position * sinA,
                    Velocity = point.Velocity * sinA,
                    Acceleration = point.Acceleration * sinA
                };

                result.Axis1Points.Add(xPt);
                result.Axis2Points.Add(yPt);
            }

            // 确保终点精确
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            if (Math.Abs(lastX.Position - targetX) > 0.001 || Math.Abs(lastY.Position - targetY) > 0.001)
            {
                var lastPoint = singleResult.Axis1Points.Last();
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetY,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.MaxJerk = maxJerk;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }
        #endregion

        #region 2轴圆弧插补
        /// <summary>
        /// 2轴圆弧插补（起始X,起始Y,圆心X,圆心Y,弧度(π=180度,要使用什么度数自己算),周期）
        /// </summary>
        public static Result ArcInterpolate(double startX, double startY, double centerX, double centerY, double angle, double dt = 0.001)
        {
            return ArcInterpolate(startX, startY, centerX, centerY, angle, _defaultMotor, dt);
        }
        /// <summary>
        /// 2轴圆弧插补（起始X,起始Y,圆心X,圆心Y,弧度(π=180度,要使用什么度数自己算),电机参数,周期）
        /// </summary>
        public static Result ArcInterpolate(double startX, double startY, double centerX, double centerY, double angle, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            double dx0 = startX - centerX;
            double dy0 = startY - centerY;
            double radius = Math.Sqrt(dx0 * dx0 + dy0 * dy0);

            if (radius < 1e-12)
            {
                result.Success = false;
                result.Error = "半径太小或起点在圆心";
                return result;
            }

            double startAngle = Math.Atan2(dy0, dx0);
            double endAngle = startAngle + angle;
            double arcLength = Math.Abs(angle) * radius;

            if (arcLength < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算电机参数
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double maxAccel = maxVel / motor.AccelTime;
            double maxJerk = maxAccel * motor.JerkRatio;

            double safeVel = Math.Sqrt(arcLength * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            maxAccel = maxVel / motor.AccelTime;
            maxJerk = maxAccel * motor.JerkRatio;

            maxVel = Math.Min(maxVel, maxVel * 1.2);
            maxAccel = Math.Min(maxAccel, maxVel * 10);
            maxJerk = Math.Min(maxJerk, maxAccel * 10);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            // 生成沿圆弧路径的S型轨迹
            var singleResult = Generate(0, arcLength, 0, 0, maxVel, maxAccel, maxJerk, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 映射到X、Y
            int direction = angle > 0 ? 1 : -1;

            foreach (var point in singleResult.Axis1Points)
            {
                double s = point.Position;
                double currentAngle = startAngle + direction * (s / radius);

                double x = centerX + radius * Math.Cos(currentAngle);
                double y = centerY + radius * Math.Sin(currentAngle);

                // 速度：切向
                double v = point.Velocity;
                double vx = -direction * v * Math.Sin(currentAngle);
                double vy = direction * v * Math.Cos(currentAngle);

                // 加速度：切向 + 向心
                double a = point.Acceleration;
                double ax = -direction * a * Math.Sin(currentAngle)
                           - direction * v * v / radius * Math.Cos(currentAngle);
                double ay = direction * a * Math.Cos(currentAngle)
                           - direction * v * v / radius * Math.Sin(currentAngle);

                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = x,
                    Velocity = vx,
                    Acceleration = ax
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = y,
                    Velocity = vy,
                    Acceleration = ay
                });
            }

            // 确保终点精确
            double endX = centerX + radius * Math.Cos(endAngle);
            double endY = centerY + radius * Math.Sin(endAngle);

            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            if (Math.Abs(lastX.Position - endX) > 0.001 || Math.Abs(lastY.Position - endY) > 0.001)
            {
                var lastPoint = singleResult.Axis1Points.Last();
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endY,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.MaxJerk = maxJerk;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }
        #endregion

        #region 2轴圆弧连续插补

        /// <summary>
        /// 2轴多圈圆弧插补（起始X,起始Y,圆心X,圆心Y,圈数,方向(1=逆时针 -1=顺时针),周期）
        /// </summary>
        public static Result ArcInterpolateMultiTurn(double startX, double startY, double centerX, double centerY, double revolutions, int direction = 1, double dt = 0.001)
        {
            return ArcInterpolateMultiTurn(startX, startY, centerX, centerY, revolutions, direction, _defaultMotor, dt);
        }

        /// <summary>
        /// 2轴多圈圆弧插补（起始X,起始Y,圆心X,圆心Y,圈数,方向(1=逆时针 -1=顺时针),电机参数,周期）
        /// </summary>
        public static Result ArcInterpolateMultiTurn(double startX, double startY, double centerX, double centerY, double revolutions, int direction, MotorParams motor, double dt = 0.001)
        {
            // 计算总角度（支持多圈）
            double totalAngle = Math.Abs(revolutions) * 2 * Math.PI * direction;
            // 调用圆弧插补
            return ArcInterpolate(startX, startY, centerX, centerY, totalAngle, motor, dt);
        }
        #endregion

        #region 2轴多点连续插补
        /// <summary>
        /// 2轴多点连续插补（[X,Y]路径点,周期）
        /// </summary>
        public static Result MultiPointInterpolate(IList<(double x, double y)> waypoints, double dt = 0.001) => MultiPointInterpolate(waypoints, _defaultMotor, dt);
        /// <summary>
        /// 2轴多点连续插补（list[X,Y]路径点,电机参数,周期）
        /// </summary>
        public static Result MultiPointInterpolate(IList<(double x, double y)> waypoints, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();
            if (waypoints == null || waypoints.Count < 2)
            {
                result.Success = false;
                result.Error = "至少需要2个路径点";
                return result;
            }

            // 计算总路径长度
            double totalLength = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                double dx = waypoints[i + 1].x - waypoints[i].x;
                double dy = waypoints[i + 1].y - waypoints[i].y;
                totalLength += Math.Sqrt(dx * dx + dy * dy);
            }

            if (totalLength < 1e-12)
            {
                result.Success = true;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].x, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].y, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算电机参数
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double maxAccel = maxVel / motor.AccelTime;
            double maxJerk = maxAccel * motor.JerkRatio;

            double safeVel = Math.Sqrt(totalLength * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            maxAccel = maxVel / motor.AccelTime;
            maxJerk = maxAccel * motor.JerkRatio;

            maxVel = Math.Min(maxVel, maxVel * 1.2);
            maxAccel = Math.Min(maxAccel, maxVel * 10);
            maxJerk = Math.Min(maxJerk, maxAccel * 10);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            // 生成总路径的S型轨迹
            var singleResult = Generate(0, totalLength, 0, 0, maxVel, maxAccel, maxJerk, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 构建路径段信息
            var segments = new List<(double startX, double startY, double endX, double endY, double length, double cumLength)>();
            double cumLen = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                double dx = waypoints[i + 1].x - waypoints[i].x;
                double dy = waypoints[i + 1].y - waypoints[i].y;
                double len = Math.Sqrt(dx * dx + dy * dy);
                segments.Add((waypoints[i].x, waypoints[i].y, waypoints[i + 1].x, waypoints[i + 1].y, len, cumLen));
                cumLen += len;
            }

            // 映射每个点
            foreach (var point in singleResult.Axis1Points)
            {
                double s = point.Position;

                // 找到当前所在的段
                int segIndex = 0;
                for (int i = 0; i < segments.Count - 1; i++)
                {
                    if (s >= segments[i].cumLength && s < segments[i + 1].cumLength)
                    {
                        segIndex = i;
                        break;
                    }
                    if (i == segments.Count - 2 && s >= segments[i + 1].cumLength)
                    {
                        segIndex = i + 1;
                    }
                }

                var seg = segments[segIndex];
                double localS = s - seg.cumLength;
                double ratio = seg.length > 0 ? localS / seg.length : 0;
                ratio = ratio < 0 ? 0 : (ratio > 1 ? 1 : ratio);

                double x = seg.startX + (seg.endX - seg.startX) * ratio;
                double y = seg.startY + (seg.endY - seg.startY) * ratio;

                double dx = seg.endX - seg.startX;
                double dy = seg.endY - seg.startY;
                double len = seg.length;
                double cosA = len > 0 ? dx / len : 0;
                double sinA = len > 0 ? dy / len : 0;

                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = x,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = y,
                    Velocity = point.Velocity * sinA,
                    Acceleration = point.Acceleration * sinA
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.MaxJerk = maxJerk;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }
        #endregion

        #region 3轴螺旋插补
        /// <summary>
        ///  3轴螺旋线插补（起始X,起始Y,起始Z,圆心X,圆心Y,弧度(π=180度),结束Z,周期）
        /// </summary>
        public static Result HelicalInterpolate(double startX, double startY, double startZ, double centerX, double centerY, double angle, double endZ, double dt = 0.001)
        {
            return HelicalInterpolate(startX, startY, startZ, centerX, centerY, angle, endZ, _defaultMotor, dt);
        }
        /// <summary>
        /// 3轴螺旋插补（起始X,起始Y,起始Z,圆心X,圆心Y,弧度(π=180度),结束Z,电机参数,周期）
        /// </summary>
        public static Result HelicalInterpolate(double startX, double startY, double startZ, double centerX, double centerY, double angle, double endZ, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            // 计算圆弧参数
            double dx0 = startX - centerX;
            double dy0 = startY - centerY;
            double radius = Math.Sqrt(dx0 * dx0 + dy0 * dy0);

            if (radius < 1e-12)
            {
                result.Success = false;
                result.Error = "半径太小或起点在圆心";
                return result;
            }

            double startAngle = Math.Atan2(dy0, dx0);
            double endAngle = startAngle + angle;
            double arcLength = Math.Abs(angle) * radius;
            double zDelta = endZ - startZ;
            double zLength = Math.Abs(zDelta);

            // 螺旋总路径长度
            double totalLength = Math.Sqrt(arcLength * arcLength + zLength * zLength);

            if (totalLength < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = startZ, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算速度参数（基于总路径长度）
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double maxAccel = maxVel / motor.AccelTime;
            double maxJerk = maxAccel * motor.JerkRatio;

            // 安全速度限制
            double safeVel = Math.Sqrt(totalLength * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            maxAccel = maxVel / motor.AccelTime;
            maxJerk = maxAccel * motor.JerkRatio;

            // 硬上限
            maxVel = Math.Min(maxVel, maxVel * 1.2);
            maxAccel = Math.Min(maxAccel, maxVel * 10);
            maxJerk = Math.Min(maxJerk, maxAccel * 10);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            // 生成沿螺旋路径的S型轨迹
            var singleResult = Generate(0, totalLength, 0, 0, maxVel, maxAccel, maxJerk, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 计算各分量比例
            double arcRatio = arcLength / totalLength;
            double zRatio = zLength / totalLength;
            int direction = angle > 0 ? 1 : -1;
            double zDirection = zDelta >= 0 ? 1 : -1;

            // 预计算终点位置
            double endX = centerX + radius * Math.Cos(endAngle);
            double endY = centerY + radius * Math.Sin(endAngle);

            // 映射到X、Y、Z
            foreach (var point in singleResult.Axis1Points)
            {
                double s = point.Position;

                // 计算圆弧和Z轴的进度
                double arcProgress = s * arcRatio;
                double zProgress = s * zRatio;

                // 当前位置角度
                double currentAngle = startAngle + direction * (arcProgress / radius);

                // 位置
                double x = centerX + radius * Math.Cos(currentAngle);
                double y = centerY + radius * Math.Sin(currentAngle);
                double z = startZ + zDirection * zProgress;

                // 速度分解
                double v = point.Velocity;
                double vArc = v * arcRatio;
                double vZ = v * zRatio;

                // X、Y速度（切向）
                double vx = -direction * vArc * Math.Sin(currentAngle);
                double vy = direction * vArc * Math.Cos(currentAngle);
                double vz = zDirection * vZ;

                // 加速度分解（包含切向加速度和向心加速度）
                double a = point.Acceleration;
                double aArc = a * arcRatio;
                double aZ = a * zRatio;

                double ax = -direction * aArc * Math.Sin(currentAngle)
                           - direction * vArc * vArc / radius * Math.Cos(currentAngle);
                double ay = direction * aArc * Math.Cos(currentAngle)
                           - direction * vArc * vArc / radius * Math.Sin(currentAngle);
                double az = zDirection * aZ;

                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = x,
                    Velocity = vx,
                    Acceleration = ax
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = y,
                    Velocity = vy,
                    Acceleration = ay
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = z,
                    Velocity = vz,
                    Acceleration = az
                });
            }

            // 确保终点精确
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - endX) > 0.001 ||
                Math.Abs(lastY.Position - endY) > 0.001 ||
                Math.Abs(lastZ.Position - endZ) > 0.001)
            {
                var lastPoint = singleResult.Axis1Points.Last();
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endY,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endZ,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.MaxJerk = maxJerk;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }
        #endregion

        #region 3轴多圈螺旋插补
        /// <summary>
        /// 多圈螺旋插补（起始X,起始Y,起始Z,圆心X,圆心Y,圈数,结束Z,方向(1=逆时针 -1=顺时针),周期）
        /// </summary>
        public static Result HelicalInterpolate(double startX, double startY, double startZ, double centerX, double centerY, double revolutions, double endZ, int direction = 1, double dt = 0.001)
        {
            return HelicalInterpolate(startX, startY, startZ, centerX, centerY, revolutions, endZ, direction, _defaultMotor, dt);
        }

        /// <summary>
        /// 多圈螺旋插补（起始X,起始Y,起始Z,圆心X,圆心Y,圈数,结束Z,方向(1=逆时针 -1=顺时针),电机参数,周期）
        /// </summary>
        public static Result HelicalInterpolate(double startX, double startY, double startZ, double centerX, double centerY, double revolutions, double endZ, int direction, MotorParams motor, double dt = 0.001)
        {
            double angle = Math.Abs(revolutions) * 2 * Math.PI * direction;
            return HelicalInterpolate(startX, startY, startZ, centerX, centerY, angle, endZ, motor, dt);
        }

        #endregion

        #region 3轴多点连续插补
        /// <summary>
        /// 3轴多点连续插补（list[X,Y,Z]路径点,周期）
        /// </summary>
        public static Result MultiPointInterpolate(IList<(double x, double y, double z)> waypoints, double dt = 0.001)
        {
            return MultiPointInterpolate(waypoints, _defaultMotor, dt);
        }
        /// <summary>
        /// 3轴多点连续插补（list[X,Y,Z]路径点,电机参数,周期）
        /// </summary>
        public static Result MultiPointInterpolate(IList<(double x, double y, double z)> waypoints, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();
            if (waypoints == null || waypoints.Count < 2)
            {
                result.Success = false;
                result.Error = "至少需要2个路径点";
                return result;
            }

            // 计算总路径长度（3D空间）
            double totalLength = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                double dx = waypoints[i + 1].x - waypoints[i].x;
                double dy = waypoints[i + 1].y - waypoints[i].y;
                double dz = waypoints[i + 1].z - waypoints[i].z;
                totalLength += Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }

            if (totalLength < 1e-12)
            {
                result.Success = true;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].x, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].y, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].z, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算速度参数
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double maxAccel = maxVel / motor.AccelTime;
            double maxJerk = maxAccel * motor.JerkRatio;

            double safeVel = Math.Sqrt(totalLength * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            maxAccel = maxVel / motor.AccelTime;
            maxJerk = maxAccel * motor.JerkRatio;

            maxVel = Math.Min(maxVel, maxVel * 1.2);
            maxAccel = Math.Min(maxAccel, maxVel * 10);
            maxJerk = Math.Min(maxJerk, maxAccel * 10);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            // 生成总路径的S型轨迹
            var singleResult = Generate(0, totalLength, 0, 0, maxVel, maxAccel, maxJerk, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 构建路径段信息
            var segments = new List<(double startX, double startY, double startZ,
                                     double endX, double endY, double endZ,
                                     double length, double cumLength)>();
            double cumLen = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                double dx = waypoints[i + 1].x - waypoints[i].x;
                double dy = waypoints[i + 1].y - waypoints[i].y;
                double dz = waypoints[i + 1].z - waypoints[i].z;
                double len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                segments.Add((waypoints[i].x, waypoints[i].y, waypoints[i].z,
                             waypoints[i + 1].x, waypoints[i + 1].y, waypoints[i + 1].z,
                             len, cumLen));
                cumLen += len;
            }

            // 映射每个点
            foreach (var point in singleResult.Axis1Points)
            {
                double s = point.Position;

                // 找到当前所在的段
                int segIndex = 0;
                for (int i = 0; i < segments.Count - 1; i++)
                {
                    if (s >= segments[i].cumLength && s < segments[i + 1].cumLength)
                    {
                        segIndex = i;
                        break;
                    }
                    if (i == segments.Count - 2 && s >= segments[i + 1].cumLength)
                    {
                        segIndex = i + 1;
                    }
                }

                var seg = segments[segIndex];
                double localS = s - seg.cumLength;
                double ratio = seg.length > 0 ? localS / seg.length : 0;
                ratio = ratio < 0 ? 0 : (ratio > 1 ? 1 : ratio);

                // 位置插值
                double x = seg.startX + (seg.endX - seg.startX) * ratio;
                double y = seg.startY + (seg.endY - seg.startY) * ratio;
                double z = seg.startZ + (seg.endZ - seg.startZ) * ratio;

                // 方向向量
                double dx = seg.endX - seg.startX;
                double dy = seg.endY - seg.startY;
                double dz = seg.endZ - seg.startZ;
                double len = seg.length;

                double cosA = len > 0 ? dx / len : 0;
                double cosB = len > 0 ? dy / len : 0;
                double cosC = len > 0 ? dz / len : 0;

                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = x,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = y,
                    Velocity = point.Velocity * cosB,
                    Acceleration = point.Acceleration * cosB
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = z,
                    Velocity = point.Velocity * cosC,
                    Acceleration = point.Acceleration * cosC
                });
            }

            // 确保终点精确
            var lastWaypoint = waypoints.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - lastWaypoint.x) > 0.001 ||
                Math.Abs(lastY.Position - lastWaypoint.y) > 0.001 ||
                Math.Abs(lastZ.Position - lastWaypoint.z) > 0.001)
            {
                var lastPoint = singleResult.Axis1Points.Last();
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = lastWaypoint.x,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = lastWaypoint.y,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = lastWaypoint.z,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.MaxJerk = maxJerk;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }

        #endregion

        #region 3轴直线插补

        /// <summary>
        /// 3轴直线插补（起始X,起始Y,起始Z,结束X,结束Y,结束Z,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, double dt = 0.001)
        {
            return LinearInterpolate(startX, startY, startZ, targetX, targetY, targetZ, _defaultMotor, dt);
        }

        /// <summary>
        /// 3轴直线插补（起始X,起始Y,起始Z,结束X,结束Y,结束Z,电机参数,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            // 计算3D空间距离
            double dx = targetX - startX;
            double dy = targetY - startY;
            double dz = targetZ - startZ;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (distance < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = startZ, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算电机参数
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double maxAccel = maxVel / motor.AccelTime;
            double maxJerk = maxAccel * motor.JerkRatio;

            // 短距离安全降速（基于3D空间距离）
            double safeVel = Math.Sqrt(distance * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            maxAccel = maxVel / motor.AccelTime;
            maxJerk = maxAccel * motor.JerkRatio;

            // 硬上限
            maxVel = Math.Min(maxVel, 5000000);
            maxAccel = Math.Min(maxAccel, maxVel * 10);
            maxJerk = Math.Min(maxJerk, maxAccel * 10);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            // 生成沿路径方向的S型轨迹（单轴）
            var singleResult = Generate(0, distance, 0, 0, maxVel, maxAccel, maxJerk, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 方向余弦
            double cosA = dx / distance;
            double cosB = dy / distance;
            double cosC = dz / distance;

            // 映射到X、Y、Z轴
            foreach (var point in singleResult.Axis1Points)
            {
                // X轴
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startX + point.Position * cosA,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                // Y轴
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startY + point.Position * cosB,
                    Velocity = point.Velocity * cosB,
                    Acceleration = point.Acceleration * cosB
                });

                // Z轴
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startZ + point.Position * cosC,
                    Velocity = point.Velocity * cosC,
                    Acceleration = point.Acceleration * cosC
                });
            }

            // 确保终点精确
            var lastPoint = singleResult.Axis1Points.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - targetX) > 0.001 ||
                Math.Abs(lastY.Position - targetY) > 0.001 ||
                Math.Abs(lastZ.Position - targetZ) > 0.001)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetY,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetZ,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.MaxJerk = maxJerk;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }

        /// <summary>
        /// 3轴直线插补（起始X,起始Y,起始Z,结束X,结束Y,结束Z,起始速度,结束速度,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, double startVel, double endVel, double dt = 0.001)
        {
            return LinearInterpolate(startX, startY, startZ, targetX, targetY, targetZ, startVel, endVel, _defaultMotor, dt);
        }

        /// <summary>
        /// 3轴直线插补（起始X,起始Y,起始Z,结束X,结束Y,结束Z,起始速度,结束速度,电机参数,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, double startVel, double endVel, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            // 计算3D空间距离
            double dx = targetX - startX;
            double dy = targetY - startY;
            double dz = targetZ - startZ;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (distance < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = startZ, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算电机参数
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double maxAccel = maxVel / motor.AccelTime;
            double maxJerk = maxAccel * motor.JerkRatio;

            // 限制起始速度和结束速度
            startVel = Math.Min(Math.Abs(startVel), maxVel);
            endVel = Math.Min(Math.Abs(endVel), maxVel);

            // 短距离安全降速
            double safeVel = Math.Sqrt(distance * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            maxAccel = maxVel / motor.AccelTime;
            maxJerk = maxAccel * motor.JerkRatio;

            // 硬上限
            maxVel = Math.Min(maxVel, 5000000);
            maxAccel = Math.Min(maxAccel, maxVel * 10);
            maxJerk = Math.Min(maxJerk, maxAccel * 10);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            // 生成沿路径方向的S型轨迹（带起始和结束速度）
            var singleResult = Generate(0, distance, startVel, endVel, maxVel, maxAccel, maxJerk, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 方向余弦
            double cosA = dx / distance;
            double cosB = dy / distance;
            double cosC = dz / distance;

            // 映射到X、Y、Z轴
            foreach (var point in singleResult.Axis1Points)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startX + point.Position * cosA,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startY + point.Position * cosB,
                    Velocity = point.Velocity * cosB,
                    Acceleration = point.Acceleration * cosB
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startZ + point.Position * cosC,
                    Velocity = point.Velocity * cosC,
                    Acceleration = point.Acceleration * cosC
                });
            }

            // 确保终点精确
            var lastPoint = singleResult.Axis1Points.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - targetX) > 0.001 ||
                Math.Abs(lastY.Position - targetY) > 0.001 ||
                Math.Abs(lastZ.Position - targetZ) > 0.001)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetY,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetZ,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.MaxJerk = maxJerk;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }

        /// <summary>
        /// 3轴直线插补（起始X,起始Y,起始Z,结束X,结束Y,结束Z,起始速度,结束速度,最大速度,最大加速度,最大加加速度,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, double startVel, double endVel, double maxVel, double maxAccel, double maxJerk, double dt = 0.001)
        {
            var result = new Result();

            // 计算3D空间距离
            double dx = targetX - startX;
            double dy = targetY - startY;
            double dz = targetZ - startZ;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (distance < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = startZ, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 参数校验
            if (maxVel < 0 || maxAccel < 0 || maxJerk < 0 || dt <= 0)
            {
                result.Success = false;
                result.Error = "速度、加速度、加加速度、采样周期必须为非负数，且采样周期>0";
                return result;
            }

            // 限制起始速度和结束速度
            startVel = Math.Min(Math.Abs(startVel), maxVel);
            endVel = Math.Min(Math.Abs(endVel), maxVel);

            // 短距离安全降速
            double safeVel = Math.Sqrt(distance * maxAccel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            maxAccel = Math.Min(maxAccel, maxVel * 10);
            maxJerk = Math.Min(maxJerk, maxAccel * 10);
            maxVel = Math.Max(maxVel, 1);
            maxAccel = Math.Max(maxAccel, 1);
            maxJerk = Math.Max(maxJerk, 1);

            // 生成沿路径方向的S型轨迹
            var singleResult = Generate(0, distance, startVel, endVel, maxVel, maxAccel, maxJerk, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 方向余弦
            double cosA = dx / distance;
            double cosB = dy / distance;
            double cosC = dz / distance;

            // 映射到X、Y、Z轴
            foreach (var point in singleResult.Axis1Points)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startX + point.Position * cosA,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startY + point.Position * cosB,
                    Velocity = point.Velocity * cosB,
                    Acceleration = point.Acceleration * cosB
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startZ + point.Position * cosC,
                    Velocity = point.Velocity * cosC,
                    Acceleration = point.Acceleration * cosC
                });
            }

            // 确保终点精确
            var lastPoint = singleResult.Axis1Points.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - targetX) > 0.001 ||
                Math.Abs(lastY.Position - targetY) > 0.001 ||
                Math.Abs(lastZ.Position - targetZ) > 0.001)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetY,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetZ,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.MaxJerk = maxJerk;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }

        #endregion
    }
    /// <summary>
    /// T型速度规划器
    /// </summary>
    public static class TrapezoidalPlanner
    {
        #region T单轴路径规划
        /// <summary>
        /// 默认电机参数
        /// </summary>
        private static MotorParams _defaultMotor = new MotorParams() { EncoderResolution = 131072, MaxRPM = 3000, AccelTime = 0.5, JerkRatio = 5 };
        /// <summary>
        /// 设置默认电机参数
        /// </summary>
        /// <param name="motor"></param>
        public static void SetMotorParams(MotorParams motor) => _defaultMotor = motor;
        /// <summary>
        /// 生成T型轨迹(起始位置,目标位置,周期)
        /// </summary>
        public static Result Generate(double startPos, double targetPos, double dt = 0.001) => Generate(startPos, targetPos, _defaultMotor, dt);

        /// <summary>
        /// 生成T型轨迹(起始位置,目标位置,电机参数,周期)
        /// </summary>
        public static Result Generate(double startPos, double targetPos, MotorParams motor, double dt = 0.001)
        {
            double distance = Math.Abs(targetPos - startPos);
            if (distance < 1) distance = 1;

            // 从电机参数计算速度、加速度
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double accel = maxVel / motor.AccelTime;
            double decel = accel;  // T型中加减速度相同

            // 安全限制：短距离时自动降速
            double safeVel = Math.Sqrt(distance * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            accel = maxVel / motor.AccelTime;
            decel = accel;

            return Generate(startPos, targetPos, maxVel, accel, decel, 0, 0, dt);
        }
        /// <summary>
        /// 生成T型轨迹 (起始位置,目标位置,最大速度,加速度,减速度,起始速度,结束速度,周期)
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="maxVel">最大速度（≥0）</param>
        /// <param name="accel">加速度（≥0，加速段使用）</param>
        /// <param name="decel">减速度（≥0，减速段使用，绝对值）</param>
        /// <param name="startVel">起始速度（≥0，方向由位移方向决定）</param>
        /// <param name="endVel">结束速度（≥0）</param>
        /// <param name="dt">采样周期</param>
        /// <returns>规划结果</returns>
        public static Result Generate(double startPos, double targetPos, double maxVel, double accel, double decel, double startVel = 0, double endVel = 0, double dt = 0.001)
        {
            var result = new Result();
            // ---------- 参数校验 ----------
            if (maxVel < 0 || accel < 0 || decel < 0 || dt <= 0)
            {
                result.Success = false;
                result.Error = "速度、加速度、减速度、采样周期必须为非负数，且采样周期>0";
                return result;
            }

            double delta = targetPos - startPos;
            double absDelta = Math.Abs(delta);
            double dir = delta >= 0 ? 1.0 : -1.0;

            if (absDelta < 1e-12)
            {
                // 无位移，直接返回
                result.Success = true;
                result.TotalTime = 0;
                result.MaxVelocity = 0;
                result.MaxAcceleration = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = 0,
                    Position = startPos,
                    Velocity = 0,
                    Acceleration = 0
                });
                return result;
            }

            // 将起始速度和结束速度限制在最大速度以内
            startVel = Math.Min(Math.Abs(startVel), maxVel);
            endVel = Math.Min(Math.Abs(endVel), maxVel);

            // 确保加速度和减速度为正值（内部使用绝对值）
            double a = Math.Max(accel, 1e-12);
            double d = Math.Max(decel, 1e-12);

            // ---------- 计算各阶段时间 ----------
            // 加速段：从 startVel 加速到 vMax
            // 减速段：从 vMax 减速到 endVel
            // 假设能达到最大速度，计算所需位移
            double tAcc = (maxVel - startVel) / a;          // 加速时间
            double tDec = (maxVel - endVel) / d;            // 减速时间
            double distAcc = (startVel + maxVel) / 2.0 * tAcc;  // 加速段位移
            double distDec = (maxVel + endVel) / 2.0 * tDec;    // 减速段位移
            double distConst = absDelta - distAcc - distDec;     // 匀速段位移

            double tConst = 0;
            double vPeak = maxVel;

            // 判断是否有匀速段
            if (distConst < 0)
            {
                // 无法达到最大速度，需要计算实际峰值速度（三角形或梯形但无匀速）
                // 使用公式：v_peak = sqrt( (startVel^2/2a + endVel^2/2d + absDelta) / (1/(2a) + 1/(2d)) )
                // 推导：位移 = v_peak^2/(2a) + v_peak^2/(2d) - startVel^2/(2a) - endVel^2/(2d)
                // 整理：v_peak = sqrt( (absDelta + startVel^2/(2a) + endVel^2/(2d)) / (1/(2a) + 1/(2d)) )
                double inv2a = 1.0 / (2.0 * a);
                double inv2d = 1.0 / (2.0 * d);
                double numerator = absDelta + startVel * startVel * inv2a + endVel * endVel * inv2d;
                double denominator = inv2a + inv2d;
                if (denominator > 1e-12)
                {
                    vPeak = Math.Sqrt(numerator / denominator);
                    vPeak = Math.Min(vPeak, maxVel); // 确保不超过设定的最大速度
                }
                else
                {
                    // 极端情况，回退
                    vPeak = Math.Sqrt(absDelta * (a * d) / (a + d));
                }

                // 重新计算加速和减速时间
                tAcc = (vPeak - startVel) / a;
                if (tAcc < 0) tAcc = 0;
                tDec = (vPeak - endVel) / d;
                if (tDec < 0) tDec = 0;
                distAcc = (startVel + vPeak) / 2.0 * tAcc;
                distDec = (vPeak + endVel) / 2.0 * tDec;
                tConst = 0; // 无匀速段
            }
            else
            {
                // 有匀速段，计算匀速段时间
                tConst = distConst / vPeak;
            }

            // 总时间
            double totalTime = tAcc + tConst + tDec;
            if (totalTime < 1e-12)
            {
                // 极短时间，直接返回终点
                result.Success = true;
                result.TotalTime = 0;
                result.MaxVelocity = 0;
                result.MaxAcceleration = 0;
                result.PointCount = 2;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startPos, Velocity = 0, Acceleration = 0 });
                result.Axis1Points.Add(new TrajectoryPoint { Time = dt, Position = targetPos, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // ---------- 采样 ----------
            int minPoints = 50;
            int maxPoints = 100000;
            int sampleCount = (int)(totalTime / dt) + 2;
            sampleCount = Math.Max(sampleCount, minPoints);
            sampleCount = Math.Min(sampleCount, maxPoints);
            double actualDt = totalTime / (sampleCount - 1);

            // 预计算各阶段结束时间点
            double t1 = tAcc;
            double t2 = tAcc + tConst;
            double t3 = totalTime;

            for (int i = 0; i < sampleCount; i++)
            {
                double time = i * actualDt;
                if (time > totalTime) time = totalTime;

                TrajectoryPoint pt = ComputePoint(time, startPos, startVel, vPeak, endVel, a, d, dir, t1, t2, t3);
                result.Axis1Points.Add(pt);
            }

            // 确保终点准确
            var last = result.Axis1Points[result.Axis1Points.Count - 1];
            if (Math.Abs(last.Position - targetPos) > 0.001)
            {
                result.Axis1Points.Add(ComputePoint(totalTime, startPos, startVel, vPeak, endVel, a, d, dir, t1, t2, t3));
            }

            result.Success = true;
            result.TotalTime = totalTime;
            result.MaxVelocity = vPeak;
            result.MaxAcceleration = Math.Max(a, d);
            result.PointCount = result.Axis1Points.Count;
            return result;
        }

        /// <summary>
        /// 计算单个轨迹点（基于分段公式）
        /// </summary>
        private static TrajectoryPoint ComputePoint(double time, double startPos, double startVel, double vPeak, double endVel, double accel, double decel, double dir, double t1, double t2, double t3)
        {
            var pt = new TrajectoryPoint { Time = time };

            if (time <= 0)
            {
                pt.Position = startPos;
                pt.Velocity = startVel * dir;
                pt.Acceleration = 0;
                return pt;
            }

            if (time >= t3)
            {
                pt.Position = startPos + dir * ((startVel + vPeak) / 2.0 * t1 + vPeak * (t2 - t1) + (vPeak + endVel) / 2.0 * (t3 - t2));
                pt.Velocity = endVel * dir;
                pt.Acceleration = 0;
                return pt;
            }

            double pos = 0, vel = 0, acc = 0;

            if (time <= t1)
            {
                // 加速段
                double dt = time;
                vel = startVel + accel * dt;
                pos = startPos * dt + 0.5 * accel * dt * dt; // 从0开始积分，但起始位置为startPos，需加上startPos
                                                             // 修正：从startPos开始，以startVel起始速度加速
                pos = startPos + startVel * dt + 0.5 * accel * dt * dt;
                // 但这里startPos是起始位置，方向为dir，所以我们用dir * ( ... )
                // 更准确：使用绝对位置
                double localPos = startVel * dt + 0.5 * accel * dt * dt;
                pos = startPos + dir * localPos;
                vel = (startVel + accel * dt) * dir;
                acc = accel * dir;
            }
            else if (time <= t2)
            {
                // 匀速段
                double dt = time - t1;
                double posAtT1 = startVel * t1 + 0.5 * accel * t1 * t1;
                double velAtT1 = startVel + accel * t1;
                pos = startPos + dir * (posAtT1 + velAtT1 * dt);
                vel = vPeak * dir;
                acc = 0;
            }
            else
            {
                // 减速段
                double dt = time - t2;
                double posAtT2 = startVel * t1 + 0.5 * accel * t1 * t1 + vPeak * (t2 - t1);
                double velAtT2 = vPeak;
                // 减速段以 -decel 减速
                double localPos = posAtT2 + velAtT2 * dt - 0.5 * decel * dt * dt;
                pos = startPos + dir * localPos;
                vel = (vPeak - decel * dt) * dir;
                acc = -decel * dir;
            }

            pt.Position = pos;
            pt.Velocity = vel;
            pt.Acceleration = acc;
            return pt;
        }

        /// <summary>
        /// 生成T型轨迹(起始位置,目标位置,最大速度,加减速度,周期)
        /// </summary>
        public static Result Generate(double startPos, double targetPos, double maxVel, double accelDecel, double dt = 0.001)
        {
            return Generate(startPos, targetPos, maxVel, accelDecel, accelDecel, 0, 0, dt);
        }

        #endregion

        #region 2轴直线插补
        /// <summary>
        /// 2轴直线插补（起始位置X,起始位置Y,目标位置X,目标位置Y,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double targetX, double targetY, double dt = 0.001)
        {
            return LinearInterpolate(startX, startY, targetX, targetY, _defaultMotor, dt);
        }

        /// <summary>
        /// 2轴直线插补（起始位置X,起始位置Y,目标位置X,目标位置Y,电机参数,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double targetX, double targetY, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            double dx = targetX - startX;
            double dy = targetY - startY;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < 1e-12)
            {
                // 无位移，返回单点
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算插补路径上的速度、加速度
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double accel = maxVel / motor.AccelTime;
            double decel = accel;

            // 短距离安全降速
            double safeVel = Math.Sqrt(distance * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            accel = maxVel / motor.AccelTime;
            decel = accel;

            // 生成单轴T型轨迹（沿路径方向）
            var singleResult = Generate(0, distance, maxVel, accel, decel, 0, 0, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 将路径轨迹映射到X、Y轴
            double cosA = dx / distance;
            double sinA = dy / distance;

            foreach (var point in singleResult.Axis1Points)
            {
                // 沿路径方向的位置映射到X、Y
                double posAlongPath = point.Position;
                double velAlongPath = point.Velocity;
                double accAlongPath = point.Acceleration;

                var xPt = new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startX + posAlongPath * cosA,
                    Velocity = velAlongPath * cosA,
                    Acceleration = accAlongPath * cosA
                };

                var yPt = new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startY + posAlongPath * sinA,
                    Velocity = velAlongPath * sinA,
                    Acceleration = accAlongPath * sinA
                };

                result.Axis1Points.Add(xPt);
                result.Axis2Points.Add(yPt);
            }

            // 确保终点精确
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            if (Math.Abs(lastX.Position - targetX) > 0.001 || Math.Abs(lastY.Position - targetY) > 0.001)
            {
                var lastPoint = singleResult.Axis1Points.Last();
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetY,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }
        #endregion

        #region 2轴圆弧插补
        /// <summary>
        /// 2轴圆弧插补（起始位置X,起始位置Y,圆心位置X,圆心位置Y,角度(弧度π=180度,需要多少度自己算),周期）
        /// </summary>
        public static Result ArcInterpolate(double startX, double startY, double centerX, double centerY, double angle, double dt = 0.001)
        {
            return ArcInterpolate(startX, startY, centerX, centerY, angle, _defaultMotor, dt);
        }
        /// <summary>
        /// 2轴圆弧插补（起始位置X,起始位置Y,圆心位置X,圆心位置Y,角度(弧度π=180度,需要多少度自己算),电机参数,周期）
        /// </summary>
        public static Result ArcInterpolate(double startX, double startY, double centerX, double centerY, double angle, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            // 计算半径和起始角度
            double dx0 = startX - centerX;
            double dy0 = startY - centerY;
            double radius = Math.Sqrt(dx0 * dx0 + dy0 * dy0);

            if (radius < 1e-12)
            {
                result.Success = false;
                result.Error = "半径太小或起点在圆心";
                return result;
            }

            double startAngle = Math.Atan2(dy0, dx0);
            double endAngle = startAngle + angle;
            double arcLength = Math.Abs(angle) * radius;

            if (arcLength < 1e-12)
            {
                // 角度为零，返回起点
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算路径速度
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double accel = maxVel / motor.AccelTime;
            double decel = accel;

            double safeVel = Math.Sqrt(arcLength * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            accel = maxVel / motor.AccelTime;
            decel = accel;

            // 生成沿圆弧路径的T型轨迹
            var singleResult = Generate(0, arcLength, maxVel, accel, decel, 0, 0, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 映射到X、Y
            int direction = angle > 0 ? 1 : -1;

            foreach (var point in singleResult.Axis1Points)
            {
                double s = point.Position; // 沿弧的位移
                double currentAngle = startAngle + direction * (s / radius);
                double velAlongPath = point.Velocity;
                double accAlongPath = point.Acceleration;

                double x = centerX + radius * Math.Cos(currentAngle);
                double y = centerY + radius * Math.Sin(currentAngle);

                // 速度方向：切向
                double vx = -direction * radius * Math.Sin(currentAngle) * (velAlongPath / radius);
                double vy = direction * radius * Math.Cos(currentAngle) * (velAlongPath / radius);

                // 加速度方向：切向 + 向心
                double ax = -direction * radius * Math.Sin(currentAngle) * (accAlongPath / radius)
                           - radius * Math.Cos(currentAngle) * Math.Pow(velAlongPath / radius, 2);
                double ay = direction * radius * Math.Cos(currentAngle) * (accAlongPath / radius)
                           - radius * Math.Sin(currentAngle) * Math.Pow(velAlongPath / radius, 2);

                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = x,
                    Velocity = vx,
                    Acceleration = ax
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = y,
                    Velocity = vy,
                    Acceleration = ay
                });
            }

            // 确保终点精确
            double endX = centerX + radius * Math.Cos(endAngle);
            double endY = centerY + radius * Math.Sin(endAngle);

            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            if (Math.Abs(lastX.Position - endX) > 0.001 || Math.Abs(lastY.Position - endY) > 0.001)
            {
                var lastPoint = singleResult.Axis1Points.Last();
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endY,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }
        #endregion

        #region 2轴圆弧连续插补

        /// <summary>
        /// 2轴多圈圆弧插补(起始X,起始Y,中心X,中心Y,圈数,方向,周期)
        /// </summary>
        public static Result ArcInterpolateMultiTurn(double startX, double startY, double centerX, double centerY, double revolutions, int direction = 1, double dt = 0.001)
        {
            return ArcInterpolateMultiTurn(startX, startY, centerX, centerY, revolutions, direction, _defaultMotor, dt);
        }

        /// <summary>
        /// 2轴多圈圆弧插补(起始X,起始Y,中心X,中心Y,圈数,方向,电机参数,周期)
        /// </summary>
        public static Result ArcInterpolateMultiTurn(double startX, double startY, double centerX, double centerY, double revolutions, int direction, MotorParams motor, double dt = 0.001)
        {
            // 计算总角度（支持多圈）
            double totalAngle = Math.Abs(revolutions) * 2 * Math.PI * direction;
            // 调用圆弧插补
            return ArcInterpolate(startX, startY, centerX, centerY, totalAngle, motor, dt);
        }
        #endregion

        #region 2轴多点连续插补
        /// <summary>
        /// 2轴多点连续插补(多点XY坐标,周期)
        /// </summary>
        public static Result MultiPointInterpolate(IList<(double x, double y)> waypoints, double dt = 0.001) => MultiPointInterpolate(waypoints, _defaultMotor, dt);
        /// <summary>
        /// 2轴多点连续插补(多点XY坐标,电机参数,周期)
        /// </summary>
        public static Result MultiPointInterpolate(IList<(double x, double y)> waypoints, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();
            if (waypoints == null || waypoints.Count < 2)
            {
                result.Success = false;
                result.Error = "至少需要2个路径点";
                return result;
            }

            // 计算总路径长度
            double totalLength = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                double dx = waypoints[i + 1].x - waypoints[i].x;
                double dy = waypoints[i + 1].y - waypoints[i].y;
                totalLength += Math.Sqrt(dx * dx + dy * dy);
            }

            if (totalLength < 1e-12)
            {
                result.Success = true;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].x, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].y, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 计算速度参数
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double accel = maxVel / motor.AccelTime;
            double decel = accel;

            double safeVel = Math.Sqrt(totalLength * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            accel = maxVel / motor.AccelTime;
            decel = accel;

            // 生成总路径的T型轨迹
            var singleResult = Generate(0, totalLength, maxVel, accel, decel, 0, 0, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 构建路径段信息
            var segments = new List<(double startX, double startY, double endX, double endY, double length, double cumLength)>();
            double cumLen = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                double dx = waypoints[i + 1].x - waypoints[i].x;
                double dy = waypoints[i + 1].y - waypoints[i].y;
                double len = Math.Sqrt(dx * dx + dy * dy);
                segments.Add((waypoints[i].x, waypoints[i].y, waypoints[i + 1].x, waypoints[i + 1].y, len, cumLen));
                cumLen += len;
            }

            // 映射每个点
            foreach (var point in singleResult.Axis1Points)
            {
                double s = point.Position;

                // 找到当前所在的段
                int segIndex = 0;
                for (int i = 0; i < segments.Count - 1; i++)
                {
                    if (s >= segments[i].cumLength && s < segments[i + 1].cumLength)
                    {
                        segIndex = i;
                        break;
                    }
                    if (i == segments.Count - 2 && s >= segments[i + 1].cumLength)
                    {
                        segIndex = i + 1;
                    }
                }

                var seg = segments[segIndex];
                double localS = s - seg.cumLength;
                double ratio = seg.length > 0 ? localS / seg.length : 0;
                ratio = ratio < 0 ? 0 : (ratio > 1 ? 1 : ratio);

                double x = seg.startX + (seg.endX - seg.startX) * ratio;
                double y = seg.startY + (seg.endY - seg.startY) * ratio;

                // 速度、加速度投影
                double dx = seg.endX - seg.startX;
                double dy = seg.endY - seg.startY;
                double len = seg.length;
                double cosA = len > 0 ? dx / len : 0;
                double sinA = len > 0 ? dy / len : 0;

                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = x,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = y,
                    Velocity = point.Velocity * sinA,
                    Acceleration = point.Acceleration * sinA
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }


        #endregion

        #region 3轴直线插补

        /// <summary>
        /// 3轴直线插补（起始X,起始Y,起始Z,目标X,目标Y,目标Z,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, double dt = 0.001)
        {
            return LinearInterpolate(startX, startY, startZ, targetX, targetY, targetZ, _defaultMotor, dt);
        }

        /// <summary>
        /// 3轴直线插补（起始X,起始Y,起始Z,目标X,目标Y,目标Z,电机参数,周期）
        /// </summary>
        public static Result LinearInterpolate(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            double dx = targetX - startX;
            double dy = targetY - startY;
            double dz = targetZ - startZ;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (distance < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = startZ, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 从电机参数计算速度、加速度
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double accel = maxVel / motor.AccelTime;
            double decel = accel;

            // 短距离安全降速
            double safeVel = Math.Sqrt(distance * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            accel = maxVel / motor.AccelTime;
            decel = accel;

            // 生成沿路径方向的T型轨迹
            var singleResult = Generate(0, distance, maxVel, accel, decel, 0, 0, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 方向余弦
            double cosA = dx / distance;
            double cosB = dy / distance;
            double cosC = dz / distance;

            foreach (var point in singleResult.Axis1Points)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startX + point.Position * cosA,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startY + point.Position * cosB,
                    Velocity = point.Velocity * cosB,
                    Acceleration = point.Acceleration * cosB
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startZ + point.Position * cosC,
                    Velocity = point.Velocity * cosC,
                    Acceleration = point.Acceleration * cosC
                });
            }

            // 确保终点精确
            var lastPoint = singleResult.Axis1Points.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - targetX) > 0.001 ||
                Math.Abs(lastY.Position - targetY) > 0.001 ||
                Math.Abs(lastZ.Position - targetZ) > 0.001)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetY,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetZ,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }

        /// <summary>
        /// 3轴直线插补（带起始速度和结束速度，使用默认电机参数）
        /// </summary>
        public static Result LinearInterpolate3D(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, double startVel, double endVel, double dt = 0.001)
        {
            return LinearInterpolate3D(startX, startY, startZ, targetX, targetY, targetZ,
                                       startVel, endVel, _defaultMotor, dt);
        }
        /// <summary>
        /// 3轴直线插补（带起始速度和结束速度，使用指定电机参数）
        /// </summary>
        public static Result LinearInterpolate3D(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, double startVel, double endVel, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            double dx = targetX - startX;
            double dy = targetY - startY;
            double dz = targetZ - startZ;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (distance < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = startZ, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 从电机参数计算速度、加速度
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double accel = maxVel / motor.AccelTime;
            double decel = accel;

            // 限制起始速度和结束速度
            startVel = Math.Min(Math.Abs(startVel), maxVel);
            endVel = Math.Min(Math.Abs(endVel), maxVel);

            // 短距离安全降速
            double safeVel = Math.Sqrt(distance * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            accel = maxVel / motor.AccelTime;
            decel = accel;

            // 生成沿路径方向的T型轨迹（带起始和结束速度）
            var singleResult = Generate(0, distance, maxVel, accel, decel, startVel, endVel, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 方向余弦
            double cosA = dx / distance;
            double cosB = dy / distance;
            double cosC = dz / distance;

            foreach (var point in singleResult.Axis1Points)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startX + point.Position * cosA,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startY + point.Position * cosB,
                    Velocity = point.Velocity * cosB,
                    Acceleration = point.Acceleration * cosB
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startZ + point.Position * cosC,
                    Velocity = point.Velocity * cosC,
                    Acceleration = point.Acceleration * cosC
                });
            }

            // 确保终点精确
            var lastPoint = singleResult.Axis1Points.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - targetX) > 0.001 ||
                Math.Abs(lastY.Position - targetY) > 0.001 ||
                Math.Abs(lastZ.Position - targetZ) > 0.001)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetY,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetZ,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }

        /// <summary>
        /// 3轴直线插补（完全自定义参数）
        /// </summary>
        public static Result LinearInterpolate3D(double startX, double startY, double startZ, double targetX, double targetY, double targetZ, double startVel, double endVel, double maxVel, double accel, double decel, double dt = 0.001)
        {
            var result = new Result();

            double dx = targetX - startX;
            double dy = targetY - startY;
            double dz = targetZ - startZ;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (distance < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = startZ, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 参数校验
            if (maxVel < 0 || accel < 0 || decel < 0 || dt <= 0)
            {
                result.Success = false;
                result.Error = "速度、加速度、减速度、采样周期必须为非负数，且采样周期>0";
                return result;
            }

            // 限制起始速度和结束速度
            startVel = Math.Min(Math.Abs(startVel), maxVel);
            endVel = Math.Min(Math.Abs(endVel), maxVel);

            // 短距离安全降速
            double safeVel = Math.Sqrt(distance * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);

            // 确保加速度和减速度为正值
            double a = Math.Max(accel, 1e-12);
            double d = Math.Max(decel, 1e-12);

            // 生成沿路径方向的T型轨迹（完全自定义）
            var singleResult = Generate(0, distance, maxVel, a, d, startVel, endVel, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 方向余弦
            double cosA = dx / distance;
            double cosB = dy / distance;
            double cosC = dz / distance;

            foreach (var point in singleResult.Axis1Points)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startX + point.Position * cosA,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startY + point.Position * cosB,
                    Velocity = point.Velocity * cosB,
                    Acceleration = point.Acceleration * cosB
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = startZ + point.Position * cosC,
                    Velocity = point.Velocity * cosC,
                    Acceleration = point.Acceleration * cosC
                });
            }

            // 确保终点精确
            var lastPoint = singleResult.Axis1Points.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - targetX) > 0.001 ||
                Math.Abs(lastY.Position - targetY) > 0.001 ||
                Math.Abs(lastZ.Position - targetZ) > 0.001)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetY,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = targetZ,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }
        #endregion

        #region 3轴螺旋插补

        /// <summary>
        /// 3轴螺旋插补（起始X,起始Y,起始Z,圆心X,圆心Y,角度,结束Z,周期）
        /// </summary>
        public static Result HelicalInterpolate(double startX, double startY, double startZ, double centerX, double centerY, double angle, double endZ, double dt = 0.001)
        {
            return HelicalInterpolate(startX, startY, startZ, centerX, centerY, angle, endZ, _defaultMotor, dt);
        }

        /// <summary>
        /// 3轴螺旋插补（起始X,起始Y,起始Z,圆心X,圆心Y,角度,结束Z,电机参数,周期）
        /// </summary>
        public static Result HelicalInterpolate(double startX, double startY, double startZ, double centerX, double centerY, double angle, double endZ, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            // 计算圆弧参数
            double dx0 = startX - centerX;
            double dy0 = startY - centerY;
            double radius = Math.Sqrt(dx0 * dx0 + dy0 * dy0);

            if (radius < 1e-12)
            {
                result.Success = false;
                result.Error = "半径太小或起点在圆心";
                return result;
            }

            double startAngle = Math.Atan2(dy0, dx0);
            double endAngle = startAngle + angle;
            double arcLength = Math.Abs(angle) * radius;
            double zDelta = endZ - startZ;
            double zLength = Math.Abs(zDelta);

            // 螺旋总路径长度
            double totalLength = Math.Sqrt(arcLength * arcLength + zLength * zLength);

            if (totalLength < 1e-12)
            {
                result.Success = true;
                result.TotalTime = 0;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = startX, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = startY, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = startZ, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 从电机参数计算速度、加速度
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double accel = maxVel / motor.AccelTime;
            double decel = accel;

            // 安全速度限制（基于总路径长度）
            double safeVel = Math.Sqrt(totalLength * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            accel = maxVel / motor.AccelTime;
            decel = accel;

            // 生成沿螺旋路径的T型轨迹
            var singleResult = Generate(0, totalLength, maxVel, accel, decel, 0, 0, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 计算各分量比例
            double arcRatio = arcLength / totalLength;
            double zRatio = zLength / totalLength;
            int direction = angle > 0 ? 1 : -1;
            double zDirection = zDelta >= 0 ? 1 : -1;

            // 预计算终点位置
            double endX = centerX + radius * Math.Cos(endAngle);
            double endY = centerY + radius * Math.Sin(endAngle);

            // 映射到X、Y、Z
            foreach (var point in singleResult.Axis1Points)
            {
                double s = point.Position;

                // 计算圆弧和Z轴的进度
                double arcProgress = s * arcRatio;
                double zProgress = s * zRatio;

                // 当前位置角度
                double currentAngle = startAngle + direction * (arcProgress / radius);

                // 位置
                double x = centerX + radius * Math.Cos(currentAngle);
                double y = centerY + radius * Math.Sin(currentAngle);
                double z = startZ + zDirection * zProgress;

                // 速度分解
                double v = point.Velocity;
                double vArc = v * arcRatio;
                double vZ = v * zRatio;

                double vx = -direction * vArc * Math.Sin(currentAngle);
                double vy = direction * vArc * Math.Cos(currentAngle);
                double vz = zDirection * vZ;

                // 加速度分解（包含向心加速度）
                double a = point.Acceleration;
                double aArc = a * arcRatio;
                double aZ = a * zRatio;

                double ax = -direction * aArc * Math.Sin(currentAngle)
                           - direction * vArc * vArc / radius * Math.Cos(currentAngle);
                double ay = direction * aArc * Math.Cos(currentAngle)
                           - direction * vArc * vArc / radius * Math.Sin(currentAngle);
                double az = zDirection * aZ;

                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = x,
                    Velocity = vx,
                    Acceleration = ax
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = y,
                    Velocity = vy,
                    Acceleration = ay
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = z,
                    Velocity = vz,
                    Acceleration = az
                });
            }

            // 确保终点精确
            var lastPoint = singleResult.Axis1Points.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - endX) > 0.001 ||
                Math.Abs(lastY.Position - endY) > 0.001 ||
                Math.Abs(lastZ.Position - endZ) > 0.001)
            {
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endX,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endY,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = endZ,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }
        #endregion

        #region 3轴多圈螺旋插补
        /// <summary>
        /// 多圈螺旋插补（起始X,起始Y,起始Z,圆心X,圆心Y,圈数,结束Z,方向(1=逆时针 -1=顺时针),周期）
        /// </summary>
        public static Result HelicalInterpolate(double startX, double startY, double startZ, double centerX, double centerY, double revolutions, double endZ, int direction = 1, double dt = 0.001)
        {
            return HelicalInterpolate(startX, startY, startZ, centerX, centerY, revolutions, endZ, direction, _defaultMotor, dt);
        }

        /// <summary>
        /// 多圈螺旋插补（起始X,起始Y,起始Z,圆心X,圆心Y,圈数,结束Z,方向(1=逆时针 -1=顺时针),电机参数,周期）
        /// </summary>
        public static Result HelicalInterpolate(double startX, double startY, double startZ, double centerX, double centerY, double revolutions, double endZ, int direction, MotorParams motor, double dt = 0.001)
        {
            double angle = Math.Abs(revolutions) * 2 * Math.PI * direction;
            return HelicalInterpolate(startX, startY, startZ, centerX, centerY, angle, endZ, motor, dt);
        }

        #endregion

        #region 3轴多点连续插补

        /// <summary>
        /// 3轴多点连续插补(多点XYZ坐标,周期)
        /// </summary>
        public static Result MultiPointInterpolate(IList<(double x, double y, double z)> waypoints, double dt = 0.001)
        {
            return MultiPointInterpolate(waypoints, _defaultMotor, dt);
        }

        /// <summary>
        /// 3轴多点连续插补(多点XYZ坐标,电机参数,周期)
        /// </summary>
        public static Result MultiPointInterpolate(IList<(double x, double y, double z)> waypoints, MotorParams motor, double dt = 0.001)
        {
            var result = new Result();

            if (waypoints == null || waypoints.Count < 2)
            {
                result.Success = false;
                result.Error = "至少需要2个路径点";
                return result;
            }

            // 计算总路径长度（3D空间）
            double totalLength = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                double dx = waypoints[i + 1].x - waypoints[i].x;
                double dy = waypoints[i + 1].y - waypoints[i].y;
                double dz = waypoints[i + 1].z - waypoints[i].z;
                totalLength += Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }

            if (totalLength < 1e-12)
            {
                result.Success = true;
                result.PointCount = 1;
                result.Axis1Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].x, Velocity = 0, Acceleration = 0 });
                result.Axis2Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].y, Velocity = 0, Acceleration = 0 });
                result.Axis3Points.Add(new TrajectoryPoint { Time = 0, Position = waypoints[0].z, Velocity = 0, Acceleration = 0 });
                return result;
            }

            // 从电机参数计算速度、加速度
            double maxVel = motor.MaxRPM / 60.0 * motor.EncoderResolution;
            double accel = maxVel / motor.AccelTime;
            double decel = accel;

            double safeVel = Math.Sqrt(totalLength * accel * 0.5);
            if (maxVel > safeVel) maxVel = Math.Max(safeVel, 10);
            accel = maxVel / motor.AccelTime;
            decel = accel;

            // 生成总路径的T型轨迹
            var singleResult = Generate(0, totalLength, maxVel, accel, decel, 0, 0, dt);
            if (!singleResult.Success)
            {
                result.Success = false;
                result.Error = singleResult.Error;
                return result;
            }

            // 构建路径段信息
            var segments = new List<(double startX, double startY, double startZ,
                                     double endX, double endY, double endZ,
                                     double length, double cumLength)>();
            double cumLen = 0;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                double dx = waypoints[i + 1].x - waypoints[i].x;
                double dy = waypoints[i + 1].y - waypoints[i].y;
                double dz = waypoints[i + 1].z - waypoints[i].z;
                double len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                segments.Add((waypoints[i].x, waypoints[i].y, waypoints[i].z,
                             waypoints[i + 1].x, waypoints[i + 1].y, waypoints[i + 1].z,
                             len, cumLen));
                cumLen += len;
            }

            // 映射每个点
            foreach (var point in singleResult.Axis1Points)
            {
                double s = point.Position;

                // 找到当前所在的段
                int segIndex = 0;
                for (int i = 0; i < segments.Count - 1; i++)
                {
                    if (s >= segments[i].cumLength && s < segments[i + 1].cumLength)
                    {
                        segIndex = i;
                        break;
                    }
                    if (i == segments.Count - 2 && s >= segments[i + 1].cumLength)
                    {
                        segIndex = i + 1;
                    }
                }

                var seg = segments[segIndex];
                double localS = s - seg.cumLength;
                double ratio = seg.length > 0 ? localS / seg.length : 0;
                ratio = ratio < 0 ? 0 : (ratio > 1 ? 1 : ratio);

                // 位置插值
                double x = seg.startX + (seg.endX - seg.startX) * ratio;
                double y = seg.startY + (seg.endY - seg.startY) * ratio;
                double z = seg.startZ + (seg.endZ - seg.startZ) * ratio;

                // 方向向量
                double dx = seg.endX - seg.startX;
                double dy = seg.endY - seg.startY;
                double dz = seg.endZ - seg.startZ;
                double len = seg.length;

                double cosA = len > 0 ? dx / len : 0;
                double cosB = len > 0 ? dy / len : 0;
                double cosC = len > 0 ? dz / len : 0;

                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = x,
                    Velocity = point.Velocity * cosA,
                    Acceleration = point.Acceleration * cosA
                });

                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = y,
                    Velocity = point.Velocity * cosB,
                    Acceleration = point.Acceleration * cosB
                });

                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = point.Time,
                    Position = z,
                    Velocity = point.Velocity * cosC,
                    Acceleration = point.Acceleration * cosC
                });
            }

            // 确保终点精确
            var lastWaypoint = waypoints.Last();
            var lastX = result.Axis1Points.Last();
            var lastY = result.Axis2Points.Last();
            var lastZ = result.Axis3Points.Last();

            if (Math.Abs(lastX.Position - lastWaypoint.x) > 0.001 ||
                Math.Abs(lastY.Position - lastWaypoint.y) > 0.001 ||
                Math.Abs(lastZ.Position - lastWaypoint.z) > 0.001)
            {
                var lastPoint = singleResult.Axis1Points.Last();
                result.Axis1Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = lastWaypoint.x,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis2Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = lastWaypoint.y,
                    Velocity = 0,
                    Acceleration = 0
                });
                result.Axis3Points.Add(new TrajectoryPoint
                {
                    Time = lastPoint.Time,
                    Position = lastWaypoint.z,
                    Velocity = 0,
                    Acceleration = 0
                });
            }

            result.Success = true;
            result.TotalTime = singleResult.TotalTime;
            result.MaxVelocity = singleResult.MaxVelocity;
            result.MaxAcceleration = singleResult.MaxAcceleration;
            result.PointCount = result.Axis1Points.Count;

            return result;
        }

        #endregion

    }
}
