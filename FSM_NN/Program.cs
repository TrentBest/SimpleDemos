using System;
using System.Collections.Generic;
using System.Linq; // Required for Average()
using System.Threading;

using TheSingularityWorkshop.FSM_API;

namespace FSM_NN
{
    class Program
    {
        // --- GLOBAL REGISTRY ---
        public static Dictionary<int, NeuronContext> Brain = new Dictionary<int, NeuronContext>();

        // --- BRAIN ANATOMY ---
        static List<string> CorticalPipeline = new List<string>
        {
            "RetinaLeft", "RetinaRight",
            "OccipitalLobe", "TemporalLobe",
            "ParietalLobe", "FrontalLobe", "MotorCortex"
        };

        // --- VISUAL INPUT PATTERN (Vertical Line) ---
        static bool[,] Pattern_Vertical = new bool[5, 5] {
            {false, false, true, false, false},
            {false, false, true, false, false},
            {false, false, true, false, false},
            {false, false, true, false, false},
            {false, false, true, false, false}
        };

        static void Main(string[] args)
        {
            Console.WriteLine("BOOTING CORTEX...");

            // 1. SETUP HARDWARE & SOFTWARE
            foreach (var lobe in CorticalPipeline)
            {
                FSM_API.Create.CreateProcessingGroup(lobe);
                NeuronBehavior.Define(lobe);
            }

            // 2. NEUROGENESIS (Spawn Neurons)
            int idCounter = 0;
            SpawnLobe("RetinaLeft", 25, ref idCounter);
            SpawnLobe("RetinaRight", 25, ref idCounter);
            SpawnLobe("OccipitalLobe", 100, ref idCounter);
            SpawnLobe("TemporalLobe", 100, ref idCounter);
            SpawnLobe("ParietalLobe", 100, ref idCounter);
            SpawnLobe("FrontalLobe", 100, ref idCounter);
            SpawnLobe("MotorCortex", 50, ref idCounter);

            // 3. WIRING (The "Blank Slate")
            Console.WriteLine("Forging Weak Synaptic Pathways...");
            WireLobes("RetinaLeft", "OccipitalLobe");
            WireLobes("RetinaRight", "OccipitalLobe");
            WireLobes("OccipitalLobe", "TemporalLobe");
            WireLobes("TemporalLobe", "ParietalLobe");
            WireLobes("ParietalLobe", "FrontalLobe");
            WireLobes("FrontalLobe", "MotorCortex");

            Console.Clear();

            // 4. THE CONSCIOUSNESS LOOP
            bool shouldRun = true;
            long tick = 0;

            while (shouldRun)
            {
                // A. CURRICULUM
                // Repetition is key. We pulse the image for 10 ticks, then rest for 10.
                // The brain must "hold" the concept during the gaps.
                if (tick % 20 < 10)
                {
                    InjectImage("RetinaLeft", Pattern_Vertical);
                }

                // B. PROCESS (The Pulse Packet System)
                PropagateSignals();
                foreach (var lobe in CorticalPipeline)
                {
                    FSM_API.Interaction.Update(lobe);
                }

                // C. VISUALIZE CONNECTOME (Plasticity View)
                // Shows weights getting stronger (White) over time
                if (tick % 2 == 0) VisualizeConnectome(tick);

                // D. SYSTEM
                tick++;
                Thread.Sleep(50);
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q) shouldRun = false;
            }
        }

        // --- BRAIN BUILDING ---

        static void SpawnLobe(string lobeName, int count, ref int idCounter)
        {
            for (int i = 0; i < count; i++)
            {
                int id = idCounter++;
                var n = new NeuronContext(id, lobeName);
                Brain.Add(id, n);
            }
        }

        static void WireLobes(string sourceLobe, string targetLobe)
        {
            var sources = GetNeuronsInLobe(sourceLobe);
            var targets = GetNeuronsInLobe(targetLobe);
            Random rng = new Random();

            foreach (var src in sources)
            {
                // Connectivity Density: 6 random connections per neuron
                for (int i = 0; i < 6; i++)
                {
                    var tgt = targets[rng.Next(targets.Count)];

                    // Axon (Sender -> Target)
                    src.OutgoingSynapses.Add(new Synapse { TargetNeuronId = tgt.NeuronId });

                    // Dendrite/Memory (Target knows Sender)
                    // *** CRITICAL FOR LEARNING ***
                    if (!tgt.SynapticWeights.ContainsKey(src.NeuronId))
                    {
                        // Start WEAK (0.1 - 0.5). 
                        // Threshold is 15.0. Input is 10.0.
                        // 10 * 0.5 = 5.0. 
                        // It will NOT fire unless 3 sources hit it at once, forcing it to learn.
                        tgt.SynapticWeights.Add(src.NeuronId, 0.1f + ((float)rng.NextDouble() * 0.4f));
                    }
                }
            }
        }

        // --- SIMULATION LOGIC ---

        static void InjectImage(string lobeName, bool[,] image)
        {
            var neurons = GetNeuronsInLobe(lobeName);
            int width = image.GetLength(0);
            int height = image.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (image[x, y])
                    {
                        int index = x + (y * width);
                        if (index < neurons.Count)
                        {
                            // Photon Hit: Massive voltage spike
                            neurons[index].MembranePotential += 50f;
                        }
                    }
                }
            }
        }

        static void PropagateSignals()
        {
            foreach (var src in Brain.Values)
            {
                if (src.IsFiring)
                {
                    var pulse = new SignalPacket
                    {
                        SenderId = src.NeuronId,
                        SignalStrength = 10.0f
                    };

                    foreach (var synapse in src.OutgoingSynapses)
                    {
                        if (Brain.TryGetValue(synapse.TargetNeuronId, out var target))
                        {
                            target.IncomingQueue.Add(pulse);
                        }
                    }
                }
            }
        }

        static List<NeuronContext> GetNeuronsInLobe(string lobe)
        {
            List<NeuronContext> list = new List<NeuronContext>();
            foreach (var n in Brain.Values)
            {
                if (n.ProcessingGroup == lobe) list.Add(n);
            }
            return list;
        }

        // --- CONNECTOME VISUALIZER ---

        static void VisualizeConnectome(long tick)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"--- CONNECTOME PLASTICITY (T={tick}) ---".PadRight(80));

            // Define layout
            DrawLobeWeights("L-EYE", "RetinaLeft", 0, 2, 5);
            DrawLobeWeights("R-EYE", "RetinaRight", 10, 2, 5);
            DrawLobeWeights("OCCIPITAL", "OccipitalLobe", 0, 9, 20);
            DrawLobeWeights("TEMPORAL", "TemporalLobe", 25, 9, 20);
            DrawLobeWeights("PARIETAL", "ParietalLobe", 50, 9, 20);
            DrawLobeWeights("FRONTAL", "FrontalLobe", 0, 16, 20);
        }

        static void DrawLobeWeights(string label, string groupName, int left, int top, int width)
        {
            var neurons = GetNeuronsInLobe(groupName);
            Console.SetCursorPosition(left, top);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(label);

            int x = 0;
            int y = 1;

            foreach (var n in neurons)
            {
                Console.SetCursorPosition(left + x, top + y);

                if (n.IsFiring)
                {
                    // FLASH RED when actively firing
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write("█");
                }
                else
                {
                    // Calculate "Intelligence" (Average Synaptic Weight)
                    float avgWeight = 0f;
                    if (n.SynapticWeights.Count > 0)
                        avgWeight = n.SynapticWeights.Values.Average();

                    // RENDER LEARNING LEVEL
                    if (avgWeight > 1.5f)      // Genius Neuron (Strongly Connected)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write(" ");
                    }
                    else if (avgWeight > 0.8f) // Learning Neuron
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.Write("▒");
                    }
                    else if (avgWeight > 0.3f) // Novice Neuron
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.Write("░");
                    }
                    else // Dead/Unconnected
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("·");
                    }
                }

                x++;
                if (x >= width) { x = 0; y++; }
            }
            Console.ResetColor();
        }
    }
}