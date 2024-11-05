using System;
using System.Collections.Generic;
using BubbleStormTweaks;
using Eremite.Buildings;
using System.Linq;

namespace ATSDumpV2
{
    class DumpSpecies
    {
        public static int speciesIndex = 0;
        public static int speciesStepSize = 5;

        public static void LogInfo(object data) => Plugin.LogInfo(data);

        public static bool Step(List<Species> outputSpecies)
        {
            var allRaces = Plugin.GameSettings.Races;
            int thisStepMax = Math.Min(allRaces.Length, speciesIndex + speciesStepSize);

            LogInfo($"[Species] Dumping from {speciesIndex} to {thisStepMax} of total {allRaces.Length}");
            for (; speciesIndex < thisStepMax; speciesIndex++)
            {
                var raceToDump = allRaces[speciesIndex];

                var outputSpeciesItem = new Species();

                outputSpeciesItem.name = raceToDump.Name;
                LogInfo($"[Species] Dumping species {raceToDump.Name} ...");

                outputSpeciesItem.needs = raceToDump.needs.Select(need => need.Name.ToString()).ToArray();

                outputSpecies.Add(outputSpeciesItem);
            }

            return speciesIndex == allRaces.Length;
        }
    }
}