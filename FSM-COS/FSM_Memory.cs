using System;
using System.Collections.Generic;
using System.Diagnostics;

using TheSingularityWorkshop.FSM_API;

// The "Memory Supervisor" Agent
public class FSM_Memory : IStateContext
{
    private static Shelf[] shelves = null;
    private static FSM_Memory _instance = null;
    public static long OS_BUFFER_MB =1024L;

    private const long BYTES_PER_KB = 1024L;
    private const long BYTES_PER_MB = BYTES_PER_KB * 1024L;
    private const long BYTES_PER_GB = BYTES_PER_MB * 1024L;
  

    public static void Initialize(int initialNumberPages = 32)
    {
        if (_instance != null) return;
        _instance = new FSM_Memory(initialNumberPages);
    }

    private FSM_Memory(int initialNumberPages)
    {
       

        long availableRAM = GetTotalPhysicalMemory();
        long allocatedRAM = availableRAM - OS_BUFFER_MB;
        //We now want to allocate our memory warehouse,essentially all free memory except the buffer we leave the OS and for our few managed classes.

        //The warehouse should itself be an abstraction into how to access it's internal data shelves.
    }

    private long GetTotalPhysicalMemory()
    {
        return GC.GetTotalMemory(false);
    }

    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
}

public class Shelf : IStateContext
{
    public FSMHandle Status { get; private set; }

    public Shelf(int pageSize)
    {

    }

    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
}