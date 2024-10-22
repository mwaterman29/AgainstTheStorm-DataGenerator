using System;
using System.Collections.Generic;
using System.Text;
using BubbleStormTweaks;
using Eremite.Buildings;

namespace ATSDumpV2
{
    class DumpBuildings
    {
        public static int buildingIndex = 0;
        public static int buildingStepSize = 5;

        public static void LogInfo(object data) => Plugin.LogInfo(data);

        public static bool Step(List<Building> outputBuildings)
        {
            var allBuildings = Plugin.GameSettings.Buildings;
            int thisStepMax = Math.Min(allBuildings.Length, buildingIndex + buildingStepSize);

            LogInfo($"[Buildings] Dumping from {buildingIndex} to {thisStepMax} of total {allBuildings.Length}");
            for (; buildingIndex < thisStepMax; buildingIndex++)
            {
                BuildingModel buildingToDump = allBuildings[buildingIndex];

                var outputBuilding = new Building();
                outputBuilding.id = buildingToDump.Name;

                LogInfo($"[Buildings] Dumping building {buildingToDump.Name} ...");

                outputBuilding.label = buildingToDump.displayName.Text;
                outputBuilding.category = buildingToDump.category.Name;
                outputBuilding.description = buildingToDump.description.Text;

                // Institution model is a city building, eg Tavern, Guild Hall, etc. We should attach the building effects here.
                if(buildingToDump is InstitutionModel institution)
                {
                    outputBuilding.effects = new List<BuildingEffect>();
                    foreach(var effectModel in institution.activeEffects)
                    {
                        BuildingEffect outputEffect = new BuildingEffect();
                        outputEffect.id = effectModel.effect.Name;
                        outputEffect.minWorkerCount = effectModel.minWorkers;
                        outputBuilding.effects.Add(outputEffect);
                    }
                }

                outputBuildings.Add(outputBuilding);
            }

            return buildingIndex == allBuildings.Length;
        
        }
    }
}
