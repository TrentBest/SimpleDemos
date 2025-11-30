using System.Numerics;

using TheSingularityWorkshop.FSM_API;

internal class AdvancedFoxContext : IFoxAgent
{
    private AdvancedDemoEnvironment Environment { get; }
    private Random random = new Random();
    private Vector2 JumpStart; // Tracks the position before the jump
    private Vector2 JumpEnd;   // Tracks the intended landing position
    private int Speed { get; set; } = 1;

    // IAdvancedAgent Properties
    public Vector2 Position { get; set; }
    public Vector2 Destination { get; set; }
    public float SightRange { get; set; } = 2; // Fox's sight
    public List<IAdvancedAgent> VisibleAgents { get; } = new List<IAdvancedAgent>();
    public List<IAdvancedAgent> CollidingAgents { get; } = new List<IAdvancedAgent>();
    public FSMHandle Status { get; }
    public bool IsValid { get; set; } = true;
    public string Name { get; set; }

    public AdvancedFoxContext(int id, Vector2 pos, AdvancedDemoEnvironment environment)
    {
        this.Position = pos;
        this.Environment = environment;
        this.Name = $"Fox[{id}]";

        if (!FSM_API.Interaction.Exists("AdvancedFoxFSM", "Foxes"))
        {
            FSM_API.Create.CreateFiniteStateMachine("AdvancedFoxFSM", -1, "Foxes")
                .State("Idle", null, OnUpdateIdle, null)
                .State("Walking", OnEnterWalking, OnUpdateWalking, OnExitWalking)
                .State("Jumping", OnEnterJumping, OnUpdateJumping, OnExitJumping)
                .State("Fleeing", OnEnterFleeing, OnUpdateFleeing, OnExitFleeing)
                .State("Mangled", OnEnterMangled, OnUpdateManagled, OnExitMangled)
                .Transition("Idle", "Walking", HasDestination)
                .Transition("Walking", "Idle", HasNoDestination)
                .Transition("Walking", "Jumping", ShouldJump)
                .Transition("Jumping", "Walking", JumpComplete) // Jump is instantaneous, transitions immediately
                .Transition("Walking", "Fleeing", ShouldFlee)
                .Transition("Fleeing", "Walking", ShouldStopFleeing)
                .BuildDefinition();
        }
        Status = FSM_API.Create.CreateInstance("AdvancedFoxFSM", this, "Foxes");
    }

    // --- MOVEMENT HELPERS ---

    /// <summary>
    /// Calculates the next position using orthogonal movement, prioritizing the axis with the larger difference (Dijkstra-like path).
    /// </summary>
    private Vector2 CalculateNextStep(AdvancedFoxContext fox)
    {
        if (Vector2.Distance(fox.Position, fox.Destination) < 1.0f)
        {
            return fox.Destination; // Already reached or adjacent to destination
        }

        float dx = Math.Abs(fox.Destination.X - fox.Position.X);
        float dy = Math.Abs(fox.Destination.Y - fox.Position.Y);

        int stepX = 0;
        int stepY = 0;

        // Follow the rule: reduce the larger axis number.
        if (dx > dy)
        {
            // Reduce X
            stepX = fox.Destination.X > fox.Position.X ? 1 : -1;
        }
        else if (dy > dx)
        {
            // Reduce Y
            stepY = fox.Destination.Y > fox.Position.Y ? 1 : -1;
        }
        else // dx == dy, choose randomly (orthogonal only)
        {
            if (fox.random.Next(2) == 0)
            {
                stepX = fox.Destination.X > fox.Position.X ? 1 : -1;
            }
            else
            {
                stepY = fox.Destination.Y > fox.Position.Y ? 1 : -1;
            }
        }

        return new Vector2(fox.Position.X + stepX, fox.Position.Y + stepY);
    }

    // --- DESTINATION LOGIC (Quadrant Exclusion) ---

    /// <summary>
    /// Determines the boundaries of the quadrant the fox is currently in.
    /// The destination must be outside this quadrant.
    /// </summary>
    private (int minX, int maxX, int minY, int maxY) GetExcludedQuadrantBounds(Vector2 currentPos, int width, int height)
    {
        int halfW = width / 2;
        int halfH = height / 2;

        bool isRight = currentPos.X >= halfW;
        bool isTop = currentPos.Y >= halfH;

        if (isRight && isTop) // Q1: Top-Right
            return (halfW, width, halfH, height);
        if (!isRight && isTop) // Q2: Top-Left
            return (0, halfW, halfH, height);
        if (!isRight && !isTop) // Q3: Bottom-Left
            return (0, halfW, 0, halfH);

        return (halfW, width, 0, halfH); // Q4: Bottom-Right
    }

    public Vector2 FindRandomDestination()
    {
        int width = Environment.EnvironmentWidth;
        int height = Environment.EngironmentHeight;
        (int exMinX, int exMaxX, int exMinY, int exMaxY) = GetExcludedQuadrantBounds(this.Position, width, height);

        Vector2 newDest = Vector2.Zero;
        bool valid = false;

        // Loop until we find a destination OUTSIDE the current quadrant
        while (!valid)
        {
            int x = random.Next(0, width);
            int y = random.Next(0, height);
            newDest = new Vector2(x, y);

            // Check if the random position is outside the excluded quadrant bounds
            bool isInExcludedX = x >= exMinX && x < exMaxX;
            bool isInExcludedY = y >= exMinY && y < exMaxY;

            if (!(isInExcludedX && isInExcludedY))
            {
                valid = true;
                return newDest;
            }
        }
        return newDest;
    }

    // --- FSM STATE LOGIC ---

    private void OnUpdateIdle(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            // If we have "no" destination (i.e., we reached the old one), find a new one.
            if (Vector2.Distance(fox.Position, fox.Destination) < 0.1f)
            {
                fox.Destination = fox.FindRandomDestination();
                // Transition to Walking handled by HasDestination
            }
        }
    }

    private void OnEnterWalking(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            Console.WriteLine($"{fox.Name} started walking to {fox.Destination}.");
        }
    }

    private void OnUpdateWalking(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            Vector2 nextPos = fox.CalculateNextStep(fox);

            // Snap to destination if effectively reached
            if (Vector2.Distance(fox.Position, fox.Destination) < 1.0f)
            {
                fox.Position = fox.Destination;
                Console.WriteLine($"{fox.Name} reached destination: {fox.Position}");
                return;
            }

            // Move one orthogonal step to the calculated position (if not jumping)
            fox.Position = nextPos;
            Console.WriteLine($"{fox.Name} walking: {fox.Position} -> Target {fox.Destination}");
        }
    }

    private void OnExitWalking(IStateContext context) { }

    private void OnEnterJumping(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            Console.WriteLine($"{fox.Name} started jumping from {fox.JumpStart} to {fox.JumpEnd}!");

            // The jump is instantaneous (1 update cycle). Check landing condition now.

            // 1. Check if the landing square has a dog (FAILURE CONDITION)
            var dogAtLandingSpot = fox.Environment.GetAgents()
                .OfType<AdvancedDogContext>()
                .Any(d => Vector2.Distance(d.Position, fox.JumpEnd) < 0.5f);

            if (dogAtLandingSpot)
            {
                // FOX IS HAD! Immediately transition to Mangled
                Console.WriteLine($"{fox.Name} landed on a dog at {fox.JumpEnd} and is MANGLED!");
                fox.Status.TransitionTo("Mangled");
                fox.Position = fox.JumpEnd; // Fox lands on dog to trigger collision/mangling
            }
            else
            {
                // 2. Successful jump
                fox.Position = fox.JumpEnd;
            }
        }
    }

    private void OnUpdateJumping(IStateContext context)
    {
        // Jump is instantaneous and logic is handled in OnEnterJumping.
    }

    private void OnExitJumping(IStateContext context) { }

    private void OnEnterFleeing(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            fox.Speed = 2; // Fleeing is faster
            Console.WriteLine($"{fox.Name} started fleeing!");
        }
    }

    private void OnUpdateFleeing(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            // Fleeing: move away from the nearest active dog using orthogonal steps
            var dogCollision = fox.CollidingAgents
                .OfType<AdvancedDogContext>()
                .OrderBy(d => Vector2.Distance(d.Position, fox.Position))
                .FirstOrDefault();

            if (dogCollision != null)
            {
                // Calculate orthogonal direction AWAY from the dog
                Vector2 fleeVector = fox.Position - dogCollision.Position;
                int stepX = 0;
                int stepY = 0;

                // Prioritize the larger axis difference for orthogonal movement away
                if (Math.Abs(fleeVector.X) > Math.Abs(fleeVector.Y))
                {
                    stepX = fleeVector.X > 0 ? fox.Speed : -fox.Speed;
                }
                else if (Math.Abs(fleeVector.Y) > Math.Abs(fleeVector.X))
                {
                    stepY = fleeVector.Y > 0 ? fox.Speed : -fox.Speed;
                }
                else // Equal distance, pick randomly
                {
                    if (fox.random.Next(2) == 0)
                    {
                        stepX = fleeVector.X > 0 ? fox.Speed : -fox.Speed;
                    }
                    else
                    {
                        stepY = fleeVector.Y > 0 ? fox.Speed : -fox.Speed;
                    }
                }

                fox.Position = new Vector2(fox.Position.X + stepX, fox.Position.Y + stepY);
            }
            else
            {
                // Fallback to moving towards destination if no dog collision is present
                Vector2 nextPos = fox.CalculateNextStep(fox);
                fox.Position = nextPos;
            }
            Console.WriteLine($"{fox.Name} fleeing to {fox.Position}");
        }
    }

    private void OnExitFleeing(IStateContext context) { }

    private void OnEnterMangled(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            Console.WriteLine($"{fox.Name} has been MANGLED!");
        }
    }

    private void OnUpdateManagled(IStateContext context)
    {
        // Fox remains stationary
    }

    private void OnExitMangled(IStateContext context) { }

    // --- FSM TRANSITION LOGIC ---

    private bool HasDestination(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            // Check if distance is >= 1.0f (allows for a single orthogonal step)
            return Vector2.Distance(fox.Position, fox.Destination) >= 1.0f;
        }
        return false;
    }

    private bool HasNoDestination(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            // Check if distance is < 1.0f (meaning the fox is at or adjacent to destination)
            return Vector2.Distance(fox.Position, fox.Destination) < 1.0f;
        }
        return false;
    }

    private bool ShouldJump(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            Vector2 nextStep = fox.CalculateNextStep(fox);

            // 1. Check if the NEXT position is occupied by a dog (the one we need to jump over)
            var dogToJumpOver = fox.Environment.GetAgents()
                .OfType<AdvancedDogContext>()
                .Any(d => Vector2.Distance(d.Position, nextStep) < 0.5f);

            if (dogToJumpOver)
            {
                // Prepare for a jump: Jump over the dog (2 steps away from current)

                // Direction vector for the two-step jump (must be orthogonal)
                int stepX = (int)(nextStep.X - fox.Position.X) * 2;
                int stepY = (int)(nextStep.Y - fox.Position.Y) * 2;

                // The intended landing position (two units away)
                fox.JumpStart = fox.Position;
                fox.JumpEnd = new Vector2(fox.Position.X + stepX, fox.Position.Y + stepY);

                // 2. Check for environment bounds before jumping
                if (fox.JumpEnd.X < 0 || fox.JumpEnd.X >= fox.Environment.EnvironmentWidth ||
                    fox.JumpEnd.Y < 0 || fox.JumpEnd.Y >= fox.Environment.EngironmentHeight)
                {
                    Console.WriteLine($"{fox.Name} blocked by dog and cannot jump out of bounds.");
                    return false;
                }

                return true;
            }
        }
        return false;
    }

    private bool JumpComplete(IStateContext context)
    {
        // Jump is instantaneous (completed in OnEnterJumping), so it is always complete
        return true;
    }

    private bool ShouldFlee(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            // Flee if a dog in the Chasing state is visible, or if there's a collision.
            var activeDog = fox.VisibleAgents.OfType<AdvancedDogContext>()
               .Any(d => d.Status.CurrentState == "Chasing");

            var collision = fox.CollidingAgents.OfType<AdvancedDogContext>().Any();

            return activeDog || collision;
        }
        return false;
    }

    private bool ShouldStopFleeing(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {
            // Stop fleeing if no dogs are visible and no longer colliding.
            return !fox.VisibleAgents.OfType<AdvancedDogContext>().Any()
                && !fox.CollidingAgents.OfType<AdvancedDogContext>().Any();
        }
        return false;
    }
}
