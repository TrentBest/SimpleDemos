# SimpleDemos: QuickBrownFox

This repository contains a simple C# console application demonstrating the core features and implementation patterns of the **TheSingularityWorkshop.FSM\_API** library.

The project simulates a classic agent interaction problem based on the phrase: **"The quick brown fox jumps over the lazy sleeping dog."**

The simulation uses Finite State Machines (FSMs) to control the behavior of two agents:
1.  **The Quick Brown Fox:** Tries to reach a win position (10).
2.  **The Lazy Sleeping Dog:** Tries to catch and "mangle" the fox.

***

## 💡 Concepts Demonstrated

This simple simulation showcases several key features of the FSM API:

* **Finite State Machine (FSM) Creation:** Defining a FSM with states and transitions (e.g., `QuickBrownFoxFSM` in `QuickBrownFox.cs`).
* **Context-Based Instances:** Creating FSM instances bound to specific data contexts (`QuickBrownFox` and `LazySleepingDog` are both `IStateContext`).
* **Context-Driven Transitions:** Using conditional functions (predicates) within the agents' context to trigger state changes (e.g., `ShouldJump` and `IsMangling`).
* **Update Loops and Processing Groups:** Structuring the simulation with a `Main` processing group for the application context and an `Update` group for all agent FSMs, allowing for concurrent updates.
* **Inter-FSM Communication:** The `LazySleepingDog` directly triggers the `Mangled` state transition on the `QuickBrownFox`'s FSM instance when a collision occurs.

***

## ⚙️ Finite State Machine Diagrams

These diagrams visualize the states and transitions for each agent defined in the code.

### 🦊 The Quick Brown Fox FSM

The fox primarily **Walks**, **Jumps** over obstacles (the dog), or **Flees** if it collides. The `Mangled` state is a terminal state forced by the dog's FSM.

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Walking
    Walking --> Jumping : ShouldJump (Dog in sight, within 2 units)
    Jumping --> Walking : ShouldLand (Jump distance reached)
    Walking --> Fleeing : ShouldFlee (Collision occurred)
    
````

### 🐕 The Lazy Sleeping Dog FSM

The dog starts **Sleeping** until an external force (a collision) wakes it up.

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Sleeping
    Sleeping --> Awake : IsAwake (Collision Occurred)
    Awake --> Chasing : ShouldChase (Fox Visible)
    Chasing --> Mangling : IsMangling (Collision with Fox)
    
    state Mangling <<terminal>>
```

-----

## 🛠️ Getting Started

This project is a standard C\# console application.

### Prerequisites

  * **.NET 8.0 SDK** or later.

### Dependencies

The project relies on the following NuGet package:

```xml
<PackageReference Include="TheSingularityWorkshop.FSM_API" Version="1.0.11" />
```

### Running the Demo

1.  Clone the repository.
2.  Navigate to the `QuickBrownFox` directory.
3.  Run the application from your terminal:
    ```bash
    dotnet run
    ```

The console output will display the simulation frame-by-frame, showing agents moving, seeing each other, and transitioning between states. The simulation terminates when the fox reaches position 10 or is mangled.

-----

## 🔍 Simulation Logic Summaries

The core logic is contained within the agents' FSM definitions.

### The Quick Brown Fox (Position 0)

| State | Transition Condition | Action (OnUpdate) |
| :--- | :--- | :--- |
| **Walking** | *ShouldJump*: A visible agent is within 2 units. *ShouldFlee*: Collision with a dog. | Increments position by 1 (Speed). |
| **Jumping** | *ShouldLand*: Position is greater than or equal to `JumpEnd` (2 steps after jumping started). | Increments position by 1. |
| **Fleeing** | None (Terminal state). | Increments position by 2 (Speed). |
| **Mangled** | None (Terminal state). | Outputs current status. |

### The Lazy Sleeping Dog (Position 3)

| State | Transition Condition | Action (OnUpdate) |
| :--- | :--- | :--- |
| **Sleeping** | *IsAwake*: A collision has occurred (The fox bumps into the dog while the dog is asleep). | Outputs sleeping status. |
| **Awake** | *ShouldChase*: The fox is visible. | Outputs awake status. |
| **Chasing** | *IsMangling*: A collision has occurred with the fox. | Increments position by 3 (Speed is set to 3 on enter). |
| **Mangling** | None (Terminal state). | **Triggers the Fox's 'Mangled' state** and outputs mangling status. |

-----

## 📄 License

This project is licensed under the **MIT License**.
