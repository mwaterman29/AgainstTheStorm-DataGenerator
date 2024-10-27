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

        //public static List<string> seasonalEffectRewards = new List<string>();
        public static List<EffectModel> seasonalEffectRewards = new List<EffectModel>();
        public static List<EffectModel> tutorialSeasonalEffectRewards = new List<EffectModel>();
        public static List<EffectModel> altarAvailableEffects = new List<EffectModel>();

        //Scan through altar rewards, seasonal rewards to see what's actually currently available
        public static bool seasonalRewardsScanned = false;

        //Once the output is formatted, we need to tag relevant effects
        //public static bool effectsTagged = false;


        public static void ScanSeasonalRewards()
        {
            //Serviceable.AltarService.
            //Serviceable.Settings.altarConfig.
            foreach(var effect in Serviceable.Settings.altarEffects)
            {
                LogInfo($"Found altar effect: ${effect.upgradedEffect.Name}");
                altarAvailableEffects.Add(effect.upgradedEffect);
            }

            foreach (var biome in Serviceable.Settings.biomes)
            {
                LogInfo($"biome: {biome.name}");

                if (biome.Name.Contains("Missing"))
                {
                    continue;
                }

                foreach (var effectHolder in biome.seasons.SeasonRewards.SelectMany(season => season.effectsTable.effects))
                {
                    /*string id = effectHolder.effect.DisplayNameKey;
                    if (!seasonalEffectRewards.Contains(id))
                        seasonalEffectRewards.Add(id);
                    */

                    if(biome.name.Contains("Tutorial"))
                    {
                        tutorialSeasonalEffectRewards.Add(effectHolder.effect);
                    }
                    else
                    {
                        if (!seasonalEffectRewards.Contains(effectHolder.effect))
                            seasonalEffectRewards.Add(effectHolder.effect);
                    }
                }
            }
        }

        /*public static void TagEffects(List<Cornerstone> outputEffects)
        {
            
        }*/

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
                if(tutorialSeasonalEffectRewards.Contains(effectToDump) || seasonalEffectRewards.Contains(effectToDump) || outputEffect.label.Contains("Stormforged"))
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

                // If this is a stormforged cornerstone, check if it's actually offered
                if(effectToDump.rarity == EffectRarity.Mythic && !altarAvailableEffects.Contains(effectToDump))
                {
                    outputEffect.tags.Add("hidden");
                }

                // If this is only available during the tutorial, then mark that as well
                if(tutorialSeasonalEffectRewards.Contains(effectToDump) && !seasonalEffectRewards.Contains(effectToDump))
                {
                    outputEffect.tags.Add("tutorial");
                }

            }

            return effectIndex == allEffects.Length;

            /*
            if (effectIndex == allEffects.Length && !effectsTagged)
            {
                TagEffects(outputEffects);
                effectsTagged = true;
                return false;
            }
            */
        }
    }
}