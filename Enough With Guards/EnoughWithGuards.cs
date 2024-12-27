using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using Exiled.API.Interfaces;
using PlayerRoles;
using UnityEngine;
using System.Linq;
using Exiled.API.Features.Doors;
using Random = UnityEngine.Random;

namespace EnoughWithGuards
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "EnoughWithGuards";
        public override string Author => "YourName";
        public override Version Version { get; } = new Version(1, 5, 2);

        private const int MaxGuards = 5;
        private const int MaxChaos = 5;

        public override void OnEnabled()
        {
            Log.Info("EnoughWithGuards Plugin Enabled.");
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Log.Info("EnoughWithGuards Plugin Disabled.");
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            base.OnDisabled();
        }

        private void OnRoundStart()
        {
            Log.Info("Round Started. Adjusting player spawn locations...");

            // Filter out players who are not Facility Guards or Chaos Insurgency players
            var guards = Player.List.Where(p => p.Role == RoleTypeId.FacilityGuard).ToList();
            var chaos = Player.List.Where(p => p.Role == RoleTypeId.ClassD).ToList(); // We assume ClassD players are potential Chaos Insurgency candidates.

            int maxPlayers = Mathf.Min(guards.Count, chaos.Count);

            int guardsSpawned = 0;
            int chaosSpawned = 0;

            // Randomly assign Chaos Insurgency players to Chaos Conscript role and teleport them to Gate A
            foreach (var player in chaos.OrderBy(p => Random.Range(0, 100)).Take(maxPlayers))
            {
                player.Role.Set(RoleTypeId.ChaosConscript);
                var gateA = Door.Get("GATE_A");
                if (gateA != null)
                {
                    player.Position = gateA.Position + new Vector3(0, 1, 0); // Slight offset above door
                    chaosSpawned++;
                    Log.Info($"Chaos Insurgency {player.Nickname} teleported to Gate A as Chaos Conscript.");
                }
                else
                {
                    Log.Warn("GATE_A door not found! Ensure the door exists in your map.");
                }

                if (chaosSpawned >= maxPlayers)
                    break;
            }

            // Randomly assign Facility Guards to Gate B
            foreach (var player in guards.OrderBy(p => Random.Range(0, 100)).Take(maxPlayers))
            {
                var gateB = Door.Get("GATE_B");
                if (gateB != null)
                {
                    player.Position = gateB.Position + new Vector3(0, 1, 0); // Slight offset above door
                    guardsSpawned++;
                    Log.Info($"Facility Guard {player.Nickname} teleported to Gate B.");
                }
                else
                {
                    Log.Warn("GATE_B door not found! Ensure the door exists in your map.");
                }

                if (guardsSpawned >= maxPlayers)
                    break;
            }

            Log.Info("Spawn adjustments complete.");
        }
    }

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; }
    }
}
