using System;
using System.Collections.Generic;
using System.Linq; // Needed for ToList()

using TheSingularityWorkshop.FSM_API;

namespace FSM_NN
{
    public static class NeuronBehavior
    {
        // Helper to generate unique names like "BioNeuron_OccipitalLobe"
        public static string GetFsmName(string group) => $"BioNeuron_{group}";

        /// <summary>
        /// Defines a unique FSM Blueprint for a specific brain region.
        /// </summary>
        public static void Define(string groupName)
        {
            string fsmName = GetFsmName(groupName);

            // 1. Safety Check: Don't redefine if it exists
            if (FSM_API.Interaction.Exists(fsmName, groupName)) return;

            // 2. BIOLOGICAL TUNING
            float decayRate = 0.9f;
            int refractoryPeriod = 5;

            // Tuning based on Lobe function
            if (groupName == "OccipitalLobe")
            {
                decayRate = 0.8f;       // Fast decay (needs constant visual stream)
                refractoryPeriod = 2;   // Fast recovery (high refresh rate)
            }
            else if (groupName == "FrontalLobe")
            {
                decayRate = 0.99f;      // Slow decay (Short term memory persistence)
                refractoryPeriod = 10;  // Deliberate firing
            }

            // GLOBAL UPDATE (Runs in every state)
            // We use a trick: Apply visual decay regardless of what the FSM is doing.
            // Note: In FSM_API, we usually put this in the State Update, but here is a clean way:
            Action<IStateContext> decayVisuals = (ctx) =>
            {
                var n = (NeuronContext)ctx;
                n.VisualIntensity *= 0.85f; // Fade out by 15% per tick
                if (n.VisualIntensity < 0.05f) n.VisualIntensity = 0f;
            };

            // 3. BUILD THE FSM
            FSM_API.Create.CreateFiniteStateMachine(fsmName, -1, groupName)

                // --- STATE: POLARIZED (Resting / Integrating) ---
                .State("Polarized", null,
                    onUpdate: (ctx) =>
                    {
                        var n = (NeuronContext)ctx;
                        decayVisuals(ctx);
                        // A. PROCESS THE MAILBOX (Integration)
                        // Read all Pulse Packets delivered by the Environment
                        if (n.IncomingQueue.Count > 0)
                        {
                            foreach (var packet in n.IncomingQueue)
                            {
                                // NEUROPLASTICITY: New Connection
                                // If we don't know this sender, create a synapse with low trust
                                if (!n.SynapticWeights.ContainsKey(packet.SenderId))
                                {
                                    n.SynapticWeights[packet.SenderId] = 0.1f;
                                }

                                // Apply Weight: Voltage += Signal * Trust
                                float weight = n.SynapticWeights[packet.SenderId];
                                n.MembranePotential += packet.SignalStrength * weight;
                            }

                            // Clear the mailbox immediately. We consumed the photons.
                            n.IncomingQueue.Clear();
                        }

                        // B. BIOLOGICAL DECAY (Leak Current)
                        // If not firing, voltage leaks back to resting state
                        if (n.MembranePotential > n.RestingPotential)
                        {
                            n.MembranePotential = (n.MembranePotential - n.RestingPotential) * decayRate + n.RestingPotential;
                        }
                    }, null)

                // --- STATE: ACTION_POTENTIAL (Firing) ---
                .State("ActionPotential",
                    onEnter: (ctx) =>
                    {
                        var n = (NeuronContext)ctx;
                        n.IsFiring = true;
                        n.MembranePotential = n.ActionPotential; // Snap to peak (+40mV)
                       n.VisualIntensity = 1.0f;              // Visual Spike

                        // --- HEBBIAN LEARNING ---
                        // "Neurons that fire together, wire together."
                        // We check our memory of neighbors. If they fired recently, they helped us.

                        // Copy keys to list to avoid "Collection Modified" errors during iteration
                        var knownSenders = n.SynapticWeights.Keys.ToList();

                        foreach (var senderId in knownSenders)
                        {
                            // Look up the sender in the God-Object (Brain) to see their state
                            if (Program.Brain.TryGetValue(senderId, out var sender))
                            {
                                // CHECK: Was this sender active recently?
                                // If they are in Refractory mode, they fired 1-5 ticks ago. Perfect timing.
                                if (sender.RefractoryTicks > 0)
                                {
                                    // LTP (Long Term Potentiation) - REWARD
                                    n.SynapticWeights[senderId] += 0.5f;

                                    // Cap the weight so it doesn't explode
                                    if (n.SynapticWeights[senderId] > 10.0f)
                                        n.SynapticWeights[senderId] = 10.0f;
                                }
                                else
                                {
                                    // LTD (Long Term Depression) - PUNISH (Optional)
                                    // Weakens connections that were silent during this decision.
                                    n.SynapticWeights[senderId] -= 0.01f;

                                    if (n.SynapticWeights[senderId] < 0.1f)
                                        n.SynapticWeights[senderId] = 0.1f; // Don't delete, just minimize
                                }
                            }
                        }
                    }, null,
                    onExit: (ctx) =>
                    {
                        var n = (NeuronContext)ctx;
                        n.IsFiring = false;
                        n.RefractoryTicks = refractoryPeriod; // Recharge timer
                        n.MembranePotential = -90f;           // Hyperpolarization (Overshoot)
                    })

                // --- STATE: REFRACTORY (Recharging) ---
                .State("Refractory", null,
                    onUpdate: (ctx) =>
                    {
                        decayVisuals(ctx);
                        var n = (NeuronContext)ctx;
                        n.RefractoryTicks--;

                        // Active Pumping: Voltage climbs back up to Resting (-70mV)
                        n.MembranePotential += 5f;
                    }, null)

                // --- TRANSITIONS ---

                // 1. Threshold Met -> Fire
                .Transition("Polarized", "ActionPotential",
                    (ctx) => ((NeuronContext)ctx).MembranePotential >= ((NeuronContext)ctx).FiringThreshold)

                // 2. Fired -> Recharge (Immediate)
                .Transition("ActionPotential", "Refractory", (ctx) => true)

                // 3. Recharged -> Ready to Listen
                .Transition("Refractory", "Polarized",
                    (ctx) => ((NeuronContext)ctx).RefractoryTicks <= 0)

                .BuildDefinition();
        }
    }
}