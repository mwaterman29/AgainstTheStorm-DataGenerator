using System;
using System.Collections.Generic;
using System.Text;
using BubbleStormTweaks;
using System.IO;
using Eremite;
using System.Linq;

namespace ATSDumpV2
{
    class DumpManager
    {
        public static void LogInfo(object data) => Plugin.LogInfo(data);
        public static string jsonFolder => @"G:\_Programming\ATS Data Dump\";

        //Getting dumped to JSON
        public static List<Item> items = new List<Item>();
        public static List<ProductionBuilding> productionBuildings = new List<ProductionBuilding>();
        public static List<Building> buildings = new List<Building>();
        public static List<Cornerstone> cornerstones = new List<Cornerstone>();
        //public static List<Recipe> recipes = new List<Recipe>();
        public static List<(string, ExtractableSpriteReference)> sprites = new List<(string, ExtractableSpriteReference)>();

        // Progress tracking:
        public static bool recipesDumped = false;
        public static bool buildingsDumped = false;
        public static bool effectsDumped = false;


        //Final output
        public static bool recipesWritten = false;
        public static bool buildingsWritten = false;
        public static bool effectsWritten = false;

        //Images
        public static int imageIndex = 0;
        public static bool imagesDeduplicated;

        // This function can be repeatedly called, and will step through the logic progressively with a manageable chunk of work each frame
        public static void DumpToJson()
        {
            //Run all steps for dumping recipes
            if (!recipesDumped)
            {
                recipesDumped = DumpRecipes.Step(sprites, productionBuildings, items);
                return;
            }

            //Run all steps for dumping buildings
            if(!buildingsDumped)
            {
                buildingsDumped = DumpBuildings.Step(buildings);
                return;
            }

            if(!effectsDumped)
            {
                effectsDumped = DumpEffects.Step(sprites, cornerstones);
                return;
            }


            if(!recipesWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing recipes... (items, productionBuildings)");
                    // Ensure the directory exists
                    if (!Directory.Exists(jsonFolder))
                    {
                        Directory.CreateDirectory(jsonFolder);
                    }

                    // Serialize items to JSON
                    string itemsJson = JSON.ToJson(items);
                    File.WriteAllText(Path.Combine(jsonFolder, "items.json"), itemsJson);

                    // Serialize productionBuildings to JSON
                    string productionBuildingsJson = JSON.ToJson(productionBuildings);
                    File.WriteAllText(Path.Combine(jsonFolder, "productionBuildings.json"), productionBuildingsJson);
                }
                catch (Exception e)
                {
                    LogInfo($"Error writing JSON files: {e.Message}");
                }
                recipesWritten = true;
                return;
            }

            if (!buildingsWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing all buildings...");
                    // Ensure the directory exists
                    if (!Directory.Exists(jsonFolder))
                    {
                        Directory.CreateDirectory(jsonFolder);
                    }

                    // Serialize items to JSON
                    string itemsJson = JSON.ToJson(items);
                    File.WriteAllText(Path.Combine(jsonFolder, "items.json"), itemsJson);

                    // Serialize productionBuildings to JSON
                    string productionBuildingsJson = JSON.ToJson(productionBuildings);
                    File.WriteAllText(Path.Combine(jsonFolder, "productionBuildings.json"), productionBuildingsJson);
                }
                catch (Exception e)
                {
                    LogInfo($"Error writing JSON files: {e.Message}");
                }
                buildingsWritten = true;
                return;
            }

            if (!effectsWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing effects...");
                    // Ensure the directory exists
                    if (!Directory.Exists(jsonFolder))
                    {
                        Directory.CreateDirectory(jsonFolder);
                    }

                    // Serialize items to JSON
                    string effectsJson = JSON.ToJson(cornerstones);
                    File.WriteAllText(Path.Combine(jsonFolder, "effects.json"), effectsJson);

                }
                catch (Exception e)
                {
                    LogInfo($"Error writing JSON files: {e.Message}");
                }
                effectsWritten = true;
                return;
            }

            if (!imagesDeduplicated)
            {
                // De-duplicate images
                var uniqueSprites = sprites
                    .GroupBy(sprite => sprite.Item1) // Group by the name
                    .Select(group => group.First())  // Select the first item in each group
                    .ToList();

                sprites = uniqueSprites;
                imagesDeduplicated = true;
                return;
            }

            if (imageIndex < sprites.Count)
            {
                LogInfo($"[Images] Dumping image {imageIndex} / {sprites.Count-1}");
                var spriteEntry = sprites[imageIndex];
                UtilityMethods.DumpImage(jsonFolder, spriteEntry);
                imageIndex++;
            }
            else
            {
                LogInfo("All done :)");
            }
        }

    }
}
