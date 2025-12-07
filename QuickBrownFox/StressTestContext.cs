using System.Diagnostics;
using System.Numerics;
using System.IO;

using TheSingularityWorkshop.FSM_API;

public class StressTestContext : IStateContext
{
    public bool IsValid { get; set; } = true;
    public string Name { get; set; } = "StressTest_Optimized";

    // --- Configuration ---
    private const int TARGET_FPS = 30;
    private const int BATCH_SIZE = 1000;
    private string csvPath = "qbf_optimized_data.csv"; // New File Name
    private long _startMem = 0;

    // Dynamic World Settings
    public int WorldMin = 0;
    public int WorldMax = 2000;
    private const int CELL_SIZE = 5;

    // --- Performance Tracking ---
    private Stopwatch _frameTimer = new Stopwatch();
    private long _frameCount = 0;
    private double _fps = 0;

    // --- Spatial Hashing & Pooling ---
    public Dictionary<int, GridCell> Grid = new Dictionary<int, GridCell>();

    // THE OPTIMIZATION: A Pool of reusable cells
    private Queue<GridCell> _cellPool = new Queue<GridCell>();
    private List<GridCell> _activeCells = new List<GridCell>();

    private List<StressAgent> _allAgents = new List<StressAgent>();
    private Random _rng = new Random();

    // --- Stats ---
    public int CountFoxesAlive => _allAgents.Count(a => a is StressFox f && f.IsAlive);
    public int CountFoxesMangled => _allAgents.Count(a => a is StressFox f && !f.IsAlive);
    public int CountDogs => _allAgents.Count(a => a is StressDog);
    public int ActiveChases = 0;
    public int ActiveFlees = 0;
    public int ActiveJumps = 0;

    public StressTestContext()
    {
        Console.Clear();
        Console.WriteLine("=== Stress Test: The Gauntlet (OPTIMIZED POOLING) ===");
        Console.WriteLine($"[Logging Data to: {Path.GetFullPath(csvPath)}]");

        if (!FSM_API.Internal.GetProcessingGroupNames().Contains("StressTest"))
        {
            FSM_API.Create.CreateProcessingGroup("StressTest");
        }

        using (StreamWriter sw = new StreamWriter(csvPath, false))
        {
            sw.WriteLine("Agents,FPS,MemTotalMB,MemDeltaMB,WorldSize,Density");
        }

        AddBatch(BATCH_SIZE);
        GC.Collect();
        _startMem = GC.GetTotalMemory(true);
        _frameTimer.Start();
    }

    public void Update()
    {
        // 1. Return active cells to the pool instead of GC'ing them
        foreach (var cell in _activeCells)
        {
            cell.Foxes.Clear();
            cell.Dogs.Clear();
            _cellPool.Enqueue(cell);
        }
        _activeCells.Clear();
        Grid.Clear();

        int chases = 0;
        int flees = 0;
        int jumps = 0;

        // 2. Rebuild Grid using POOL
        foreach (var agent in _allAgents)
        {
            if (!agent.IsActive) continue;

            int cellKey = agent.Position / CELL_SIZE;
            if (!Grid.TryGetValue(cellKey, out var cell))
            {
                // Get from pool or create new if empty
                if (_cellPool.Count > 0)
                {
                    cell = _cellPool.Dequeue();
                }
                else
                {
                    cell = new GridCell();
                }

                Grid[cellKey] = cell;
                _activeCells.Add(cell);
            }

            if (agent is StressFox fox && fox.IsAlive)
            {
                cell.Foxes.Add(fox);
                string state = fox.State.CurrentState;
                if (state == "Fleeing") flees++;
                if (state == "Jumping") jumps++;
            }
            else if (agent is StressDog dog)
            {
                cell.Dogs.Add(dog);
                if (dog.State.CurrentState == "Chasing") chases++;
            }
        }

        ActiveChases = chases;
        ActiveFlees = flees;
        ActiveJumps = jumps;

        // 3. Measure Performance
        long elapsedMs = _frameTimer.ElapsedMilliseconds;
        if (elapsedMs >= 1000)
        {
            _fps = _frameCount / (elapsedMs / 1000.0);
            long currentMem = GC.GetTotalMemory(false);
            long memDelta = (currentMem - _startMem) / 1024 / 1024;
            long memTotal = currentMem / 1024 / 1024;

            RenderStats(memDelta);
            LogData(memTotal, memDelta);

            // Dynamic Scaling
            if (_fps > TARGET_FPS + 2)
            {
                int growth = (int)((WorldMax - WorldMin) * 0.1f);
                WorldMax += growth;
                WorldMin -= growth / 2;
                AddBatch(BATCH_SIZE);
            }

            _frameCount = 0;
            _frameTimer.Restart();
            _startMem = GC.GetTotalMemory(false);
        }

        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
        {
            IsValid = false;
            Cleanup();
            return;
        }

        FSM_API.Interaction.Update("StressTest");
        _frameCount++;
    }

    private void LogData(long memTotal, long memDelta)
    {
        float density = _allAgents.Count / (float)(WorldMax - WorldMin);
        using (StreamWriter sw = new StreamWriter(csvPath, true))
        {
            sw.WriteLine($"{_allAgents.Count},{_fps:F2},{memTotal},{memDelta},{WorldMax - WorldMin},{density:F4}");
        }
    }

    private void RenderStats(long memDelta)
    {
        Console.SetCursorPosition(0, 5);
        Console.WriteLine($"[OPTIMIZED Status]              ");
        Console.WriteLine($"FPS:            {_fps:F2} / {TARGET_FPS}      ");
        Console.WriteLine($"Mem Delta:      {memDelta} MB/sec      "); // Visual feedback
        Console.WriteLine($"Pool Size:      {_cellPool.Count}      ");
        Console.WriteLine($"Agents:         {_allAgents.Count:N0}       ");
    }

    private void AddBatch(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int pos = _rng.Next(WorldMin, WorldMax);
            if (_rng.NextDouble() < 0.3) _allAgents.Add(new StressDog(pos, this));
            else _allAgents.Add(new StressFox(pos, this));
        }
    }

    private void Cleanup()
    {
        foreach (var agent in _allAgents) FSM_API.Interaction.DestroyInstance(agent.State);
        FSM_API.Interaction.RemoveProcessingGroup("StressTest");
    }

    public class GridCell { public List<StressFox> Foxes = new(); public List<StressDog> Dogs = new(); }
    public abstract class StressAgent : IStateContext
    {
        public bool IsValid { get; set; } = true; public string Name { get; set; }
        public int Position; public FSMHandle State; public StressTestContext Context; public bool IsActive = true;
    }
    // (StressFox and StressDog classes remain unchanged from previous file)
    public class StressFox : StressAgent
    {
        public bool IsAlive = true; public int Destination; public int Speed = 1;
        public StressFox(int pos, StressTestContext ctx)
        {
            Context = ctx; Position = pos; Name = "Fox";
            Destination = (pos < (ctx.WorldMax + ctx.WorldMin) / 2) ? ctx.WorldMax : ctx.WorldMin;
            if (!FSM_API.Interaction.Exists("StressFox", "StressTest"))
            {
                FSM_API.Create.CreateFiniteStateMachine("StressFox", -1, "StressTest")
                    .State("Walking", null, OnUpdateWalking, null)
                    .State("Jumping", OnEnterJump, OnUpdateJump, null)
                    .State("Fleeing", null, OnUpdateFleeing, null)
                    .State("Mangled", OnEnterMangled, null, null)
                    .Transition("Walking", "Jumping", ShouldJump)
                    .Transition("Walking", "Fleeing", ShouldFlee)
                    .Transition("Jumping", "Walking", JumpComplete)
                    .Transition("Fleeing", "Walking", SafetyReached)
                    .BuildDefinition();
            }
            State = FSM_API.Create.CreateInstance("StressFox", this, "StressTest");
        }
        private static void OnUpdateWalking(IStateContext c)
        {
            var f = (StressFox)c; int dir = Math.Sign(f.Destination - f.Position); f.Position += dir * f.Speed;
            if ((dir > 0 && f.Position >= f.Context.WorldMax) || (dir < 0 && f.Position <= f.Context.WorldMin))
                f.Destination = (f.Destination == f.Context.WorldMax) ? f.Context.WorldMin : f.Context.WorldMax;
        }
        private static bool ShouldJump(IStateContext c)
        {
            var f = (StressFox)c; int lookAhead = f.Position + Math.Sign(f.Destination - f.Position);
            int cellKey = lookAhead / 5;
            if (f.Context.Grid.TryGetValue(cellKey, out var cell)) return cell.Dogs.Any(d => d.State.CurrentState == "Sleeping");
            return false;
        }
        private static void OnEnterJump(IStateContext c)
        {
            var f = (StressFox)c; int dir = Math.Sign(f.Destination - f.Position); f.Position += dir * 2;
            int cellKey = f.Position / 5;
            if (f.Context.Grid.TryGetValue(cellKey, out var cell))
            {
                foreach (var dog in cell.Dogs) if (Math.Abs(dog.Position - f.Position) < 1) dog.WokenBy = f;
            }
        }
        private static void OnUpdateJump(IStateContext c) { }
        private static bool JumpComplete(IStateContext c) => true;
        private static bool ShouldFlee(IStateContext c)
        {
            var f = (StressFox)c; int cellKey = f.Position / 5;
            if (f.Context.Grid.TryGetValue(cellKey, out var cell)) return cell.Dogs.Any(d => d.State.CurrentState == "Chasing");
            return false;
        }
        private static void OnUpdateFleeing(IStateContext c)
        {
            var f = (StressFox)c; int dir = Math.Sign(f.Destination - f.Position); f.Position += dir * 2;
        }
        private static bool SafetyReached(IStateContext c) => new Random().NextDouble() > 0.8;
        private static void OnEnterMangled(IStateContext c) { var f = (StressFox)c; f.IsAlive = false; f.IsValid = false; }
    }
    public class StressDog : StressAgent
    {
        public StressFox WokenBy = null;
        public StressDog(int pos, StressTestContext ctx)
        {
            Context = ctx; Position = pos; Name = "Dog";
            if (!FSM_API.Interaction.Exists("StressDog", "StressTest"))
            {
                FSM_API.Create.CreateFiniteStateMachine("StressDog", -1, "StressTest")
                    .State("Sleeping", null, null, null).State("Chasing", null, OnUpdateChasing, null).State("Mangling", null, null, null)
                    .Transition("Sleeping", "Chasing", IsWoken).Transition("Chasing", "Mangling", CanMangle)
                    .Transition("Chasing", "Sleeping", LostTarget).Transition("Mangling", "Sleeping", c => true).BuildDefinition();
            }
            State = FSM_API.Create.CreateInstance("StressDog", this, "StressTest");
        }
        private static bool IsWoken(IStateContext c) => ((StressDog)c).WokenBy != null;
        private static void OnUpdateChasing(IStateContext c)
        {
            var d = (StressDog)c; if (d.WokenBy == null || !d.WokenBy.IsAlive) return;
            int dir = Math.Sign(d.WokenBy.Position - d.Position); d.Position += dir;
        }
        private static bool CanMangle(IStateContext c)
        {
            var d = (StressDog)c; if (d.WokenBy != null && d.WokenBy.IsAlive && Math.Abs(d.Position - d.WokenBy.Position) < 1)
            {
                d.WokenBy.State.TransitionTo("Mangled"); return true;
            }
            return false;
        }
        private static bool LostTarget(IStateContext c)
        {
            var d = (StressDog)c; if (d.WokenBy == null || !d.WokenBy.IsAlive || Math.Abs(d.Position - d.WokenBy.Position) > 10)
            {
                d.WokenBy = null; return true;
            }
            return false;
        }
    }
}