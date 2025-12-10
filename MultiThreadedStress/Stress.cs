using System;
using System.Collections.Generic;

using TheSingularityWorkshop.FSM_API;

public class Stress : IStateContext
{
    public int WorkerCount { get; set; } // Changed from private set to public set
    public int TickInterval { get; private set; }
    public string GroupName { get; private set; }

    public List<FSMHandle> Workers { get; private set; } = new List<FSMHandle>();

    // Payload Data
    public int DataValue { get; set; } = 0;
    public long LastFrameOps { get; set; }

    // NEW: Lock object for thread safety when creating/updating agents in this group.
    private readonly object _lock = new object();
    public object LockObject => _lock;

    public Stress(int workerCount, int tickInterval, string groupName = "Stress")
    {
        this.WorkerCount = workerCount;
        this.TickInterval = tickInterval;
        this.GroupName = groupName;
        Name = "StressManager";

        // 1. Define Manager (Runs every frame)
        if (!FSM_API.Interaction.Exists("StressManagerFSM", GroupName))
        {
            FSM_API.Create.CreateProcessingGroup(GroupName);
            FSM_API.Create.CreateFiniteStateMachine("StressManagerFSM", 1, GroupName)
                .State("Operating", null, null, null)
                .BuildDefinition();
        }

        // 2. Define Worker (Variable Interval)
        string workerFsmName = $"WorkerFSM_{TickInterval}";
        if (!FSM_API.Interaction.Exists(workerFsmName, GroupName))
        {
            FSM_API.Create.CreateFiniteStateMachine(workerFsmName, TickInterval, GroupName)
                .State("Active", null, OnUpdateWorker, null)
                .BuildDefinition();
        }

        // Create Manager Instance
        Status = FSM_API.Create.CreateInstance("StressManagerFSM", this, GroupName);
        IsValid = true;
    }

    public void SpawnWorkers()
    {
        // Only run logic when the lock is acquired. This is the fix for the concurrency issue.
        lock (LockObject)
        {
            string workerFsmName = $"WorkerFSM_{TickInterval}";

            // FIX: Calculate how many new workers to add to reach the target WorkerCount
            int agentsToAdd = WorkerCount - Workers.Count;

            if (agentsToAdd > 0)
            {
                for (int i = Workers.Count; i < WorkerCount; i++)
                {
                    // Create and add the new FSM instance (Agent)
                    var workerCtx = new Stress(0, TickInterval, GroupName) { Name = $"Worker_{GroupName}_{i}" };
                    workerCtx.Status = FSM_API.Create.CreateInstance(workerFsmName, workerCtx, GroupName);
                    Workers.Add(workerCtx.Status);
                }
            }
        }
    }

    // --- The Payload (UNCHANGED) ---
    private void OnUpdateWorker(IStateContext context)
    {
        if (context is Stress worker)
        {
            long ops = 0;

            // 1. Math Ops
            worker.DataValue++;
            worker.DataValue *= 2;
            ops += 2;

            // 2. The String Allocation Bottleneck
            // This is what we expect to speed up when we switch to hashing
            string s = worker.DataValue.ToString();

            // 3. Simple Branching
            if (worker.DataValue % 2 == 0) Math.Abs(worker.DataValue);
            ops += 1;

            worker.LastFrameOps = ops;
        }
    }

    // Interface
    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
    public FSMHandle Status { get; set; }
}