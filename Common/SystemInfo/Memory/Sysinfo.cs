﻿namespace NetX.Common;

/// <summary>
/// 
/// </summary>
public struct Sysinfo
{
    /// <summary>
    /// Seconds since boot
    /// </summary>
    public long uptime;

    /// <summary>
    /// 获取 1，5，15 分钟内存的平均使用量，数组大小为 3
    /// </summary>
    unsafe public fixed ulong loads[3];

    /// <summary>
    /// 总物理内存
    /// </summary>
    public ulong totalram;

    /// <summary>
    /// 可用内存
    /// </summary>
    public ulong freeram;

    /// <summary>
    /// 共享内存
    /// </summary>
    public ulong sharedram;

    /// <summary>
    /// Memory used by buffers
    /// </summary>
    public ulong bufferram;

    /// <summary>
    /// Total swap space size
    /// </summary>
    public ulong totalswap;

    /// <summary>
    /// swap space still available
    /// </summary>
    public ulong freeswap;

    /// <summary>
    /// Number of current processes
    /// </summary>
    public ushort procs;

    /// <summary>
    /// Total high memory size 
    /// </summary>
    public ulong totalhigh;

    /// <summary>
    /// Available high memory size 
    /// </summary>
    public ulong freehigh;

    /// <summary>
    /// Memory unit size in bytes
    /// </summary>
    public uint mem_unit;

    /// <summary>
    /// Padding to 64 bytes
    /// </summary>
    unsafe public fixed byte _f[64];
}
