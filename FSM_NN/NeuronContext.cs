using System.Collections.Generic;

using TheSingularityWorkshop.FSM_API;

namespace FSM_NN
{

    public class Synapse
    {
        public int TargetNeuronId;
        public float Weight;
    }

    public struct SignalPacket
    {
        public int SenderId;
        public float SignalStrength;
    }

    public class NeuronContext : IStateContext
    {
        public int NeuronId;
        public string Name { get; set; }
        public FSMHandle Status { get; set; }
        public bool IsValid { get; set; } = false;
        public readonly string ProcessingGroup;

        // --- THE MAILBOX (Environmental Input) ---
        // Photons/Signals land here waiting to be processed.
        public List<SignalPacket> IncomingQueue = new List<SignalPacket>();

        // --- LONG TERM MEMORY (Synaptic Weights) ---
        // "How much do I trust this SenderID?"
        // This is what changes when the brain learns.
        public Dictionary<int, float> SynapticWeights = new Dictionary<int, float>();

        // --- OUTPUTS ---
        public List<Synapse> OutgoingSynapses = new List<Synapse>();

        // --- BIOLOGY ---
        public float MembranePotential = -70f;
        public float RestingPotential = -70f;
        public float FiringThreshold = -55f;
        public float ActionPotential = 40f;

        public bool IsFiring = false;
        public int RefractoryTicks = 0;
        public long LastFiredTick = -100; // Track precise timing for learning
        // --- VISUALIZATION ONLY ---
        // 1.0 = White Hot, 0.0 = Black. 
        // This is separate from voltage so we can see "history"
        public float VisualIntensity = 0f;
        public NeuronContext(int id, string processingGroup)
        {
            NeuronId = id;
            Name = $"Neuron_{id}";
            this.ProcessingGroup = processingGroup;

            if (!FSM_API.Interaction.Exists(processingGroup))
                FSM_API.Create.CreateProcessingGroup(processingGroup);

            string fsmDefName = NeuronBehavior.GetFsmName(processingGroup);
            Status = FSM_API.Create.CreateInstance(fsmDefName, this, processingGroup);
            IsValid = true;
        }
    }
}