using System;
using System.Collections.Generic;
using BubbleStormTweaks;
using Eremite.Model;
using Eremite.Services;
using System.Linq;

namespace ATSDumpV2
{
    class DumpEffects
    {
        public static int effectIndex = 0;
        public static int effectStepSize = 5;

        public static void LogInfo(object data) => Plugin.LogInfo(data);

        public static List<string> seasonalEffectRewards = new List<string>();
        public static bool seasonalRewardsScanned = false;

        public static void ScanSeasonalRewards()
        {
            foreach (var biome in Serviceable.Settings.biomes)
            {
                foreach (var effectHolder in biome.seasons.SeasonRewards.SelectMany(season => season.effectsTable.effects))
                {
                    string id = effectHolder.effect.DisplayNameKey;
                    if (!seasonalEffectRewards.Contains(id))
                        seasonalEffectRewards.Add(id);
                }
            }
        }

        public static bool Step(List<(string, ExtractableSpriteReference)> sprites, List<Cornerstone> outputEffects)
        {
            var allEffects = Plugin.GameSettings.effects;
            int thisStepMax = Math.Min(allEffects.Length, effectIndex + effectStepSize);

            if (!seasonalRewardsScanned)
            {
                ScanSeasonalRewards();
                seasonalRewardsScanned = true;
                return false;
            }

            LogInfo($"[Effects] Dumping from {effectIndex} to {thisStepMax} of total {allEffects.Length}");
            for (; effectIndex < thisStepMax; effectIndex++)
            {
                EffectModel effectToDump = allEffects[effectIndex];

                var outputEffect = new Cornerstone();
                outputEffect.id = effectToDump.DisplayNameKey;

                LogInfo($"[Effects] Dumping effect {effectToDump.DisplayNameKey} ...");

                outputEffect.label = effectToDump.DisplayName;

                //Many effects list ">Missing key<" - no need to do these
                if (outputEffect.label.Contains("Missing"))
                {
                    continue;
                }

                outputEffect.description = effectToDump.Description;
                outputEffect.tier = effectToDump.rarity.ToString();

                outputEffects.Add(outputEffect);

                /*
                 * Effects are very very widely used.
                 * We don't want to include rewards here, or debug buildings. 
                 * Seems like excluding empty ids, labels that include 'missing', and Tier is 'None' or 'Common' will be sufficient.
                 */

                /*
                 * If in seasonal rewards, it's a cornerstone
                 * If it's not common or none rarity, call it a perk
                 * Otherwise list as an effect
                 */
                string type = "Effect";
                if(seasonalEffectRewards.Contains(outputEffect.id) || outputEffect.label.Contains("Stormforged"))
                {
                    type = "Cornerstone";
                }
                else if(outputEffect.tier != "None" && outputEffect.tier != "Common")
                {
                    type = "Perk";
                }

                outputEffect.type = type;

                if (effectToDump.GetIcon() != null)
                {
                    ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(effectToDump.GetIcon());
                    sprites.Add((outputEffect.label, sr));
                }

            }

            return effectIndex == allEffects.Length;
        }
    }
}