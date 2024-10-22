using System;
using System.Collections.Generic;
using BubbleStormTweaks;
using Eremite.Model;

namespace ATSDumpV2
{
    class DumpEffects
    {
        public static int effectIndex = 0;
        public static int effectStepSize = 5;

        public static void LogInfo(object data) => Plugin.LogInfo(data);

        public static bool Step(List<(string, ExtractableSpriteReference)> sprites, List<Cornerstone> outputEffects)
        {
            var allEffects = Plugin.GameSettings.effects;
            int thisStepMax = Math.Min(allEffects.Length, effectIndex + effectStepSize);

            LogInfo($"[Effects] Dumping from {effectIndex} to {thisStepMax} of total {allEffects.Length}");
            for (; effectIndex < thisStepMax; effectIndex++)
            {
                EffectModel effectToDump = allEffects[effectIndex];

                var outputEffect = new Cornerstone();
                outputEffect.id = effectToDump.DisplayNameKey;

                LogInfo($"[Effects] Dumping effect {effectToDump.DisplayNameKey} ...");

                outputEffect.label = effectToDump.DisplayName;
                outputEffect.description = effectToDump.Description;
                outputEffect.tier = effectToDump.rarity.ToString();

                outputEffects.Add(outputEffect);


                if(effectToDump.GetIcon() != null)
                {
                    ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(effectToDump.GetIcon());
                    sprites.Add((outputEffect.label, sr));
                }

            }

            return effectIndex == allEffects.Length;
        }
    }
}