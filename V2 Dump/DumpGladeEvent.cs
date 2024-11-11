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
                RelicModel relicToDump = allRelics[eventIndex];

                var outputEvent = new GladeEvent();
                outputEvent.id = relicToDump.name;
                outputEvent.label = relicToDump.displayName.GetText();
                outputEvent.EffectsWhileWorking = relicToDump.activeEffects?.Select(e => e.name).ToList() ?? new List<string>();
                outputEvent.ThreatEffects = relicToDump.effectsTiers?.SelectMany(t => t.effect)?.Select(e => e.displayName.GetText()).ToList() ?? new List<string>();
                outputEvent.workerSlots = relicToDump.WorkplacesCount;
                outputEvent.totalTime = relicToDump.GetWorkingTime(0, 0);

                //tyyyyyy
                outputEvent.difficulty = GetRelicGroup(relicToDump).Item1;

                if (relicToDump.difficulties != null)
                {
                    outputEvent.difficulties = relicToDump.difficulties.Select(d => new GladeDifficulty
                    {
                        difficultyClass = d.difficulty.ToString(),
                        gladeSolveOptions = d.decisions?.Select(decision => new GladeSolveOption
                        {
                            name = decision.label?.displayName.GetText(),
                            decisionTag = decision.decisionTag?.displayName.GetText(),
                            options1 = decision.requriedGoods?.sets.FirstOrDefault()?.goods.Select(g => new ItemUsage(g.good.Name, g.amount)).ToList() ?? new List<ItemUsage>(),
                            options2 = decision.requriedGoods?.sets.ElementAtOrDefault(1)?.goods.Select(g => new ItemUsage(g.good.Name, g.amount)).ToList() ?? new List<ItemUsage>()
                        }).ToList() ?? new List<GladeSolveOption>()
                    }).ToList();
                }

                // Dump the rewards table
                /* outputEvent.gladeRewards = new List<GladeReward>();
                 foreach (var rewardStep in relicToDump.rewardsTiers)
                 {
                     if (rewardStep.rewardsTable != null)
                     {
                         foreach (var entity in rewardStep.rewardsTable.effects)
                         {
                             outputEvent.gladeRewards.Add(new GladeReward
                             {
                                 effect = entity.effect.name,
                                 chance = entity.chance
                             });
                         }

                         foreach (var entity in rewardStep.rewardsTable.guaranteedEffects)
                         {
                             outputEvent.gladeRewards.Add(new GladeReward
                             {
                                 effect = entity.name,
                                 chance = 100
                             });
                         }
                     }
                 }
                */
                //outputEvent.rewardTableName = relicToDump.rewardsTiers.FirstOrDefault()?.rewardsTable?.Name ?? "No Rewards Table";
                outputEvent.rewardTableNames = relicToDump.decisionsRewards.Select(t => t.Name).ToList();

                ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(relicToDump.icon);
                sprites.Add((outputEvent.id, sr));

                LogInfo($"[GladeEvents] Dumping event {relicToDump.name} ...");

                outputEvents.Add(outputEvent);
            }

            return eventIndex == allRelics.Length;
        }
        
        private static Tuple<string, int> GetRelicGroup(RelicModel relic)
        {
            if (relic.Name.StartsWith("DebugNode"))
                return new("Debug", 0);
            if (relic.dangerLevel == DangerLevel.Forbidden)
                return new("Forbidden", 1);
            if (relic.dangerLevel == DangerLevel.Dangerous)
                return new("Dangerous", 2);
            if (relic.name.Contains("Treasure Stag") || relic.name.Contains("Rainpunk Drill") || relic.name.Contains("Monolith"))
                return new("Small", 3);
            if (relic.displayName.Text.Contains("Encampment") || relic.displayName.Text.Contains("Abandoned Cache"))
                return new("Other", 4);
            if (relic.interactionType == RelicInteractionType.TraderPanel)
                return new("Trader", 5);
            if (relic.orderModel != null)
                return new("Ghosts", 6);

            return new("Ruins", 99);
        }
    }
}