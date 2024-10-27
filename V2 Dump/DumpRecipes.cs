using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Eremite;
using Eremite.Buildings;
using Eremite.Characters.Villagers;
using Eremite.Controller;
using Eremite.Model;
using Eremite.Model.Effects;
using Eremite.Model.Meta;
using Eremite.Model.Orders;
using Eremite.Services;
using Eremite.WorldMap;
using Eremite.Model.Goals;
using HarmonyLib;
using Newtonsoft.Json;
using QFSW.QC;
using QFSW.QC.Utilities;
using UnityEngine;
using BubbleStormTweaks;

namespace ATSDumpV2
{
    /*
     * - Pull all recipes from the GameSettings
     * - Convert to Serializable Recipes
     * - Based on the Serializable Recipes, convert to the output classes
     * - Log to a file
     */
    class DumpRecipes
    {
        public static void LogInfo(object data) => Plugin.LogInfo(data);

        //Raw recipes extracted from the models
        public static List<Dumper.RecipeRaw> rawRecipes = new List<Dumper.RecipeRaw>();

        //Recipes converted to serializable outputs
        public static List<SerializableRecipe> recipes = new List<SerializableRecipe>();

        //Indices and step size
        public static int recipeIndex = 0;
        public static int recipeStepSize = 5;

        //Step function
        public static bool Step(List<(string, ExtractableSpriteReference)> sprites, List<ProductionBuilding> buildings, List<Item> items)
        {
            if (rawRecipes.Count == 0 || recipes.Count == 0)
            {
                DumpRawRecipes(sprites);
                LogInfo($"[Recipes] All recipes dumped from original buildings, converting...");
                return false;

            }

            int thisStepMax = Math.Min(recipes.Count, recipeIndex + recipeStepSize);
            LogInfo($"[Recipes] Dumping from {recipeIndex} to {thisStepMax} of total {recipes.Count}");
            for (; recipeIndex < thisStepMax; recipeIndex++)
            {
                SerializableRecipe recipeToProcess = recipes[recipeIndex];
                Process(recipeToProcess, buildings, items);
            }

            if (recipeIndex == recipes.Count)
            {
                LogInfo($"[Recipes] All {recipes.Count} dumped successfully!");
                return true;
            }

            return false;
        }



        //Convert a single raw recipe to a SerializableRecipe
        public static SerializableRecipe ConvertRecipe(Dumper.RecipeRaw recipeRaw)
        {
            List<string> ing1 = new List<string>();
            List<int> count1 = new List<int>();
            List<string> ing2 = new List<string>();
            List<int> count2 = new List<int>();

            if (recipeRaw.ingredients != null)
            {
                for (int i = 0; i < recipeRaw.ingredients.Length; i++)
                {
                    foreach (var g in recipeRaw.ingredients[i].goods)
                    {
                        if (g != null)
                        {
                            if (i == 0)
                            {
                                ing1.Add($"{g.DisplayName}");
                                count1.Add(g.amount);
                            }
                            if (i == 1)
                            {
                                ing2.Add($"{g.DisplayName}");
                                count2.Add(g.amount);
                            }
                        }
                    }

                }
            }

            var ret = new SerializableRecipe(
                    tier: recipeRaw.tier.Count(c => c == '★'),
                    output: (recipeRaw.output != null) ? recipeRaw.output.DisplayName : "(no output)",
                    outputCount: (recipeRaw.output != null) ? recipeRaw.output.amount : -1,
                    producedBy: recipeRaw.building,
                    ing1.ToArray(),
                    count1.ToArray(),
                    ing2.ToArray(),
                    count2.ToArray(),
                    (int)Math.Round(recipeRaw.processingTime)
                    );


            return ret;
        }

        //Dump raw recipes
        public static void DumpRawRecipes(List<(string, ExtractableSpriteReference)> sprites)
        {
            rawRecipes = Dumper.DumpBuildings(null);

            //In addition to using the dump buildings code that exists, we should pull the icons as well
            foreach (var source in Plugin.GameSettings.Buildings)
            {
                sprites.Add((source.name, UtilityMethods.GetSpriteRef(source.icon)));
                LogInfo($"Dumping texture for {source.name} -> icon name {source.icon.name}");
            }

            foreach (var r in rawRecipes)
            {
                if (r != null)
                    recipes.Add(ConvertRecipe(r));

                if (r == null || r.ingredients == null)
                {
                    LogInfo($"Skipping null rawRecipe");
                    continue;
                }

                foreach (var ing in r.ingredients)
                {
                    if (ing == null || ing.goods == null)
                    {
                        LogInfo($"Skipping null ing/ing Items");
                        continue;
                    }

                    foreach (var Item in ing.goods)
                    {
                        if (Item == null || Item.Icon == null || Item.DisplayName == null)
                        {
                            LogInfo($"Skipping null Item / Item.icon");
                            continue;
                        }

                        ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(Item.Icon);
                        sprites.Add((Item.DisplayName, sr));
                    }
                }

                if (r.output == null || r.output.DisplayName == null || r.output.Icon == null)
                {
                    //LogInfo($"Skipping no output");
                }
                else
                {
                    ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(r.output.Icon);
                    sprites.Add((r.output.DisplayName, sr));
                }

            }
        }

        public static void Process(SerializableRecipe sr, List<ProductionBuilding> buildings, List<Item> items)
        {
            try
            {
                //First, create an item for the output
                string outputName = sr.output;
                Item existingOutputItem = items.Find(item => item.id == outputName);

                //If it doesn't exist, create a new one
                if (existingOutputItem == null)
                {
                    existingOutputItem = new Item(outputName, outputName, sr.ingredientsFirst.ToList(), sr.ingredientsSecond.ToList(), new List<string>());
                    items.Add(existingOutputItem);
                }
                //If it does exist, see if the ingredients are present.
                else
                {
                    // Union the existing usesFirst with sr.ingredientsFirst
                    existingOutputItem.usesFirst = existingOutputItem.usesFirst
                        .Union(sr.ingredientsFirst)
                        .Distinct()
                        .ToList();

                    // Union the existing usesSecond with sr.ingredientsSecond
                    existingOutputItem.usesSecond = existingOutputItem.usesSecond
                        .Union(sr.ingredientsSecond)
                        .Distinct()
                        .ToList();
                }

                //For each first ingredient
                for (int i = 0; i < sr.ingredientsFirst.Length; i++)
                {
                    string itemName = sr.ingredientsFirst[i];
                    var existingItem = items.Find(item => item.id == itemName);

                    //If it doesn't exist, create it
                    if (existingItem == null)
                    {
                        existingItem = new Item(itemName, itemName, new List<string>(), new List<string>(), new List<string>());
                        items.Add(existingItem);
                    }

                    //Add this to used in, if it's not already present.
                    if (!existingItem.usedIn.Contains(outputName))
                    {
                        existingItem.usedIn.Add(outputName);
                    }
                }

                //For each second ingredient
                for (int i = 0; i < sr.ingredientsSecond.Length; i++)
                {
                    string itemName = sr.ingredientsSecond[i];
                    var existingItem = items.Find(item => item.id == itemName);

                    //If it doesn't exist, create it
                    if (existingItem == null)
                    {
                        existingItem = new Item(itemName, itemName, new List<string>(), new List<string>(), new List<string>());
                        items.Add(existingItem);
                    }

                    //Add this to used in, if it's not already present.
                    if (!existingItem.usedIn.Contains(outputName))
                    {
                        existingItem.usedIn.Add(outputName);
                    }
                }

                //Once the items for all ingredients are processed, process the recipe.
                ProductionBuilding existingBuilding = buildings.Find(building => building.id == sr.producedBy);
                if (existingBuilding == null)
                {
                    existingBuilding = new ProductionBuilding(sr.producedBy, new List<SerializableRecipe>(), -1);
                    buildings.Add(existingBuilding);
                }

                //Once a building exists, see if it contains this recipe
                SerializableRecipe existingRecipe = existingBuilding.produces.Find(recipe => recipe.tier == sr.tier && recipe.output == sr.output);
                if (existingRecipe == null)
                {
                    //No need to create the recipe, just insert it.
                    existingBuilding.produces.Add(sr);
                }

            }
            catch (Exception e)
            {
                LogInfo($"[Recipes] Error @ {sr.output}: {e.Message}");
            }
        }
    }
}
