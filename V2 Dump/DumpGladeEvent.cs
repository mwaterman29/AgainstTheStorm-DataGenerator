using System;
using System.Collections.Generic;
using System.Linq;
using Eremite.Buildings;
using Eremite.Model;
using BubbleStormTweaks;

namespace ATSDumpV2
{
    class DumpGladeEvents
    {
        public static int eventIndex = 0;
        public static int eventStepSize = 5;

        public static void LogInfo(object data) => Plugin.LogInfo(data);

        public static bool Step(List<(string, ExtractableSpriteReference)> sprites, List<GladeEvent> outputEvents)
        {
            var allRelics = Plugin.GameSettings.Relics.Where(r => r.isGladeEvent).ToArray();
            int thisStepMax = Math.Min(allRelics.Length, eventIndex + eventStepSize);

            LogInfo($"[GladeEvents] Dumping from {eventIndex} to {thisStepMax} of total {allRelics.Length}");
            for (; eventIndex < thisStepMax; eventIndex++)
            {
                var relicToDump = allRelics[eventIndex];

                var outputEvent = new GladeEvent
                {
                    Name = relicToDump.name,
                    SolveOptions = relicToDump.decisionsRewards.Select(d => d.ToString()).ToList(),
                    SolveCategories = relicToDump.workplaces.Select(w => w.ToString()).ToList(),
                    EffectsWhileWorking = relicToDump.activeEffects.Select(e => e.name).ToList(),
                    ThreatEffects = relicToDump.effectsTiers.SelectMany(t => t.effect).Select(e => e.name).ToList(),
                    Reward = relicToDump.rewardsTiers.FirstOrDefault()?.ToString() ?? "None",
                    WorkerSlots = relicToDump.WorkplacesCount,
                    TotalTime = relicToDump.GetWorkingTime(0, 0),
                    Difficulty = relicToDump.difficulties.FirstOrDefault()?.ToString() ?? "Unknown",
                };

                ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(relicToDump.icon);
                sprites.Add((outputEvent.Name, sr));

                LogInfo($"[GladeEvents] Dumping event {relicToDump.name} ...");

                outputEvents.Add(outputEvent);
            }

            return eventIndex == allRelics.Length;
        }
    }
}