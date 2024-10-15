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

namespace BubbleStormTweaks.V2_Dump
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

        //Outputs:
        public static List<Building> buildings = new List<Building>();
        public static List<Good> goods = new List<Good>();

        public static List<(string, ExtractableSpriteReference)> sprites = new List<(string, ExtractableSpriteReference)>();
        /*public static int srI;
        public static int imgI = 0;
        public static int goalI = 0;
        public static bool started = false;*/

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
        public static void DumpRawRecipes()
        {
            rawRecipes = Dumper.DumpBuildings(null);
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
                        LogInfo($"Skipping null ing/ing goods");
                        continue;
                    }

                    foreach (var good in ing.goods)
                    {
                        if (good == null || good.Icon == null || good.DisplayName == null)
                        {
                            LogInfo($"Skipping null good / good.icon");
                            continue;
                        }

                        ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(good.Icon);
                        sprites.Add((good.DisplayName, sr));
                    }
                }

                if (r.output == null || r.output.DisplayName == null || r.output.Icon == null)
                {
                    LogInfo($"Skipping no output");
                }
                else
                {
                    ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(r.output.Icon);
                    sprites.Add((r.output.DisplayName, sr));
                }

            }
        }
        public static void Process(SerializableRecipe sr)
        {
            try
            {
                // Add this recipe to what the building produces:
                Building producedBy = buildings.Find(b => b.id == sr.producedBy);
                if (producedBy != null)
                {
                    producedBy.produces.Add(new Recipe(sr.output, sr.producedBy, sr.output, new Dictionary<int, RecipeTier>()));
                }
                else
                {
                    Building temp = new Building(sr.producedBy, new List<Recipe>(), -1);
                    temp.produces.Add(new Recipe(sr.output, sr.producedBy, sr.output, new Dictionary<int, RecipeTier>()));
                    buildings.Add(temp);
                }

                // Combine all ingredients and counts
                List<string> allIngredients = sr.ingredientsFirst.Concat(sr.ingredientsSecond).ToList();
                List<int> allCounts = sr.ingredientsFirstCounts.Concat(sr.ingredientsSecondCounts).ToList();

                if (allIngredients.Count != allCounts.Count)
                {
                    LogInfo($"Count mismatch: {allIngredients.Count} to {allCounts.Count}!");
                    return;
                }

                // Process each ingredient
                for (int i = 0; i < allIngredients.Count; i++)
                {
                    string input = allIngredients[i];
                    string output = sr.output;

                    // Find or create the recipe
                    Recipe recipe = new Recipe(sr.output, sr.producedBy, sr.output, new Dictionary<int, RecipeTier>());
                    if (!recipe.tiers.ContainsKey(sr.tier))
                    {
                        recipe.tiers[sr.tier] = new RecipeTier(allCounts[i], sr.outputCount, sr.timeInSeconds);
                    }

                    // Find or create the goods
                    Good existingGood = goods.Find(g => g.id == input);
                    if (existingGood == null)
                    {
                        existingGood = new Good(input, input, new List<GoodUsage>(), new List<GoodUsage>(), new List<string>());
                        goods.Add(existingGood);
                    }

                    if (!existingGood.usedIn.Contains(output))
                        existingGood.usedIn.Add(output);
                }

                // Check output as well
                Good existingGoodOut = goods.Find(g => g.id == sr.output);
                if (existingGoodOut == null)
                {
                    existingGoodOut = new Good(sr.output, sr.output, new List<GoodUsage>(), new List<GoodUsage>(), new List<string>());
                    goods.Add(existingGoodOut);
                }

                // Ensure recipe ingredients are appended when they're found
                existingGoodOut.usesFirst.AddRange(sr.ingredientsFirst.Select(ing => new GoodUsage(ing, sr.ingredientsFirstCounts[Array.IndexOf(sr.ingredientsFirst, ing)])));
                existingGoodOut.usesSecond.AddRange(sr.ingredientsSecond.Select(ing => new GoodUsage(ing, sr.ingredientsSecondCounts[Array.IndexOf(sr.ingredientsSecond, ing)])));

                existingGoodOut.usesFirst = existingGoodOut.usesFirst.Distinct().ToList();
                existingGoodOut.usesSecond = existingGoodOut.usesSecond.Distinct().ToList();

            }
            catch (Exception e)
            {
                LogInfo($"Error @ {sr.output}: {e.Message}");
            }
        }
    }
}
