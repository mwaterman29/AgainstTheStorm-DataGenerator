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

        // Getting dumped to JSON
        public static List<Item> items = new List<Item>();
        public static List<Item> itemsFromGoods = new List<Item>();
        public static List<ProductionBuilding> productionBuildings = new List<ProductionBuilding>();
        public static List<Building> buildings = new List<Building>();
        public static List<Cornerstone> cornerstones = new List<Cornerstone>();
        public static List<Order> orders = new List<Order>();
        public static List<Biome> biomes = new List<Biome>();
        public static List<Species> species = new List<Species>();
        public static List<(string, ExtractableSpriteReference)> sprites = new List<(string, ExtractableSpriteReference)>();

        // Progress tracking:
        public static bool recipesDumped = false;
        public static bool buildingsDumped = false;
        public static bool effectsDumped = false;
        public static bool ordersDumped = false;
        public static bool biomesDumped = false;
        public static bool speciesDumped = false;
        public static bool buildingsFormatted = false;
        public static bool itemsDumped = false;

        // Final output
        public static bool recipesWritten = false;
        public static bool buildingsWritten = false;
        public static bool effectsWritten = false;
        public static bool ordersWritten = false;
        public static bool biomesWritten = false;
        public static bool speciesWritten = false;

        // Images
        public static int imageIndex = 0;
        public static bool imagesDeduplicated;

        // This function can be repeatedly called, and will step through the logic progressively with a manageable chunk of work each frame
        public static void DumpToJson()
        {
            if (!itemsDumped)
            {
                itemsDumped = DumpItems.Step(sprites, itemsFromGoods);
                return;
            }

            if (!speciesDumped)
            {
                speciesDumped = DumpSpecies.Step(species);
                return;
            }

            if (!recipesDumped)
            {
                recipesDumped = DumpRecipes.Step(sprites, productionBuildings, items);
                return;
            }

            if (!buildingsDumped)
            {
                buildingsDumped = DumpBuildings.Step(buildings);
                return;
            }

            if (!buildingsFormatted)
            {
                buildingsFormatted = DumpBuildings.UpdateProductionBuildings(productionBuildings);
                return;
            }

            if (!effectsDumped)
            {
                effectsDumped = DumpEffects.Step(sprites, cornerstones);
                return;
            }

            if (!ordersDumped)
            {
                ordersDumped = DumpOrders.Step(orders);
                return;
            }

            if (!biomesDumped)
            {
                biomesDumped = DumpBiomes.DumpAllBiomes(sprites, biomes);
                return;
            }

            if (!recipesWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing recipes... (items, productionBuildings)");
                    EnsureDirectoryExists(jsonFolder);

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

            if (!speciesWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing species...");
                    EnsureDirectoryExists(jsonFolder);

                    // Serialize species to JSON
                    string speciesJson = JSON.ToJson(species);
                    File.WriteAllText(Path.Combine(jsonFolder, "species.json"), speciesJson);
                }
                catch (Exception e)
                {
                    LogInfo($"Error writing JSON files: {e.Message}");
                }
                speciesWritten = true;
                return;
            }

            if (!buildingsWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing all buildings...");
                    EnsureDirectoryExists(jsonFolder);

                    // Serialize buildings to JSON
                    string buildingsJson = JSON.ToJson(buildings);
                    File.WriteAllText(Path.Combine(jsonFolder, "buildings.json"), buildingsJson);
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
                    EnsureDirectoryExists(jsonFolder);

                    // Serialize effects to JSON
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

            if (!ordersWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing orders...");
                    EnsureDirectoryExists(jsonFolder);

                    // Serialize orders to JSON
                    string ordersJson = JSON.ToJson(orders);
                    File.WriteAllText(Path.Combine(jsonFolder, "orders.json"), ordersJson);
                }
                catch (Exception e)
                {
                    LogInfo($"Error writing JSON files: {e.Message}");
                }
                ordersWritten = true;
                return;
            }

            if (!biomesWritten)
            {
                try
                {
                    LogInfo("[JSON] Writing biomes...");
                    EnsureDirectoryExists(jsonFolder);

                    // Serialize orders to JSON
                    string biomesJson = JSON.ToJson(biomes);
                    File.WriteAllText(Path.Combine(jsonFolder, "biomes.json"), biomesJson);
                }
                catch (Exception e)
                {
                    LogInfo($"Error writing JSON files: {e.Message}");
                }
                biomesWritten = true;
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
                LogInfo($"[Images] Dumping image {imageIndex} / {sprites.Count - 1}");

                try
                {
                    var spriteEntry = sprites[imageIndex];
                    UtilityMethods.DumpImage(jsonFolder, spriteEntry);
                    imageIndex++;
                }
                catch (Exception e)
                {
                    LogInfo($"[Image] Error {e.Message} - {sprites[imageIndex].Item1}");
                    imageIndex++;
                }
            }
            else
            {
                LogInfo("All done :)");
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}