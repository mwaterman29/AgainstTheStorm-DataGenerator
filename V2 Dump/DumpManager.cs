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
        public static List<Building> buildings = new List<Building>();
        //public static List<Recipe> recipes = new List<Recipe>();
        public static List<(string, ExtractableSpriteReference)> sprites = new List<(string, ExtractableSpriteReference)>();

        // Progress tracking:
        public static bool recipesDumped = false;

        //Final output
        public static bool recipesWritten = false;

        //Images
        public static int imageIndex = 0;
        public static bool imagesDeduplicated;

        // This function can be repeatedly called, and will step through the logic progressively with a manageable chunk of work each frame
        public static void DumpToJson()
        {
            //Run all steps for dumping recipes
            if (!recipesDumped)
            {
                recipesDumped = DumpRecipes.Step(sprites, buildings, items);
                return;
            }

            if(!recipesWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing recipes... (items, buildings)");
                    // Ensure the directory exists
                    if (!Directory.Exists(jsonFolder))
                    {
                        Directory.CreateDirectory(jsonFolder);
                    }

                    // Serialize items to JSON
                    string itemsJson = JSON.ToJson(items);
                    File.WriteAllText(Path.Combine(jsonFolder, "items.json"), itemsJson);

                    // Serialize buildings to JSON
                    string buildingsJson = JSON.ToJson(buildings);
                    File.WriteAllText(Path.Combine(jsonFolder, "buildings.json"), buildingsJson);
                }
                catch (Exception e)
                {
                    LogInfo($"Error writing JSON files: {e.Message}");
                }
                recipesWritten = true;
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
