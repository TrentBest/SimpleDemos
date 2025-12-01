using System.Diagnostics;
using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class StressTestContext : IStateContext
{
    public bool IsValid { get; set; } = true;
    public string Name { get; set; } = "StressTest";

    // --- Configuration ---
    private const int TARGET_FPS = 30;
    private const int BATCH_SIZE = 1000;

    // Dynamic World Settings
    public int WorldMin = 0;
    public int WorldMax = 2000;
    private const int CELL_SIZE = 5;

    // --- Performance Tracking ---
    private Stopwatch _frameTimer = new Stopwatch();
    private long _frameCount = 0;
    private double _fps = 0;

    // --- Spatial Hashing ---
    public Dictionary<int, GridCell> Grid = new Dictionary<int, GridCell>();
    private List<StressAgent> _allAgents = new List<StressAgent>();
    private Random _rng = new Random();

    // --- Stats ---
    public int CountFoxesAlive => _allAgents.Count(a => a is StressFox f && f.IsAlive);
    public int CountFoxesMangled => _allAgents.Count(a => a is StressFox f && !f.IsAlive);
    public int CountDogs => _allAgents.Count(a => a is StressDog);

    // NEW: Real-time Action Metrics
    public int ActiveChases = 0;
    public int ActiveFlees = 0;
    public int ActiveJumps = 0;

    public StressTestContext()
    {
        Console.Clear();
        Console.WriteLine("=== Stress Test: The Gauntlet (Dynamic) ===");
        Console.WriteLine("Goal: Foxes must traverse an expanding world of sleeping dogs.");
        Console.WriteLine("Logic: Spatial Hashing + Dynamic FSMs (Jump/Flee/Chase).");
        Console.WriteLine("Press 'Esc' to Stop.\n");

        if (!FSM_API.Internal.GetProcessingGroupNames().Contains("StressTest"))
        {
            FSM_API.Create.CreateProcessingGroup("StressTest");
        }

        AddBatch(BATCH_SIZE);
        _frameTimer.Start();
    }

    public void Update()
    {
        // 1. Reset Frame Counters
        Grid.Clear();
        int chases = 0;
        int flees = 0;
        int jumps = 0;

        // 2. Rebuild Grid & Count States (The Overhead Check)
        foreach (var agent in _allAgents)
        {
            if (!agent.IsActive) continue;

            // Spatial Hash
            int cellKey = agent.Position / CELL_SIZE;
            if (!Grid.TryGetValue(cellKey, out var cell))
            {
                cell = new GridCell();
                Grid[cellKey] = cell;
            }

            if (agent is StressFox fox && fox.IsAlive)
            {
                cell.Foxes.Add(fox);

                // Count Actions
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

        // 3. Measure Performance & Render
        long elapsedMs = _frameTimer.ElapsedMilliseconds;
        if (elapsedMs >= 1000)
        {
            _fps = _frameCount / (elapsedMs / 1000.0);
            RenderStats();

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
        }

        // 4. Input & Updates
        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
        {
            IsValid = false;
            Cleanup();
            return;
        }

        FSM_API.Interaction.Update("StressTest");
        _frameCount++;
    }

    private void RenderStats()
    {
        Console.SetCursorPosition(0, 5);
        Console.WriteLine($"[Simulation Status]             ");
        Console.WriteLine($"FPS:            {_fps:F2} / {TARGET_FPS}      ");
        Console.WriteLine($"World Width:    {(WorldMax - WorldMin):N0} units       ");
        Console.WriteLine($"Density:        {_allAgents.Count / (float)(WorldMax - WorldMin):F4} agents/unit");
        Console.WriteLine($"-------------------------------");
        Console.WriteLine($"[Population]                    ");
        Console.WriteLine($"Total Agents:   {_allAgents.Count:N0}       ");
        Console.WriteLine($"Dogs:           {CountDogs:N0}       ");
        Console.WriteLine($"Foxes (Alive):  {CountFoxesAlive:N0}       ");
        Console.WriteLine($"Foxes (Dead):   {CountFoxesMangled:N0}       ");
        Console.WriteLine($"-------------------------------");
        Console.WriteLine($"[Live Actions]                  ");
        Console.WriteLine($"Dogs Awake:     {ActiveChases:N0}       ");
        Console.WriteLine($"Foxes Fleeing:  {ActiveFlees:N0}       ");
        Console.WriteLine($"Foxes Jumping:  {ActiveJumps:N0}       ");
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
        Console.WriteLine("\n\nStopping Stress Test...");
        foreach (var agent in _allAgents) FSM_API.Interaction.DestroyInstance(agent.State);
        FSM_API.Interaction.RemoveProcessingGroup("StressTest");
        _allAgents.Clear();
    }

    // --- Helpers & Agents (Same as before) ---
    public class GridCell { public List<StressFox> Foxes = new(); public List<StressDog> Dogs = new(); }
    public abstract class StressAgent : IStateContext
    {
        public bool IsValid { get; set; } = true; public string Name { get; set; }
        public int Position; public FSMHandle State; public StressTestContext Context; public bool IsActive = true;
    }
    // (Include StressFox and StressDog classes from previous turn here)
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