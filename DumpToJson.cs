using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
using HarmonyLib;
using Newtonsoft.Json;
using QFSW.QC;
using QFSW.QC.Utilities;
using UnityEngine;
using BubbleStormTweaks;

public static class DumpToJson
{
    public static void LogInfo(object data) => Plugin.LogInfo(data);
    public static Settings GameSettings => Plugin.GameSettings;
    public static string JsonFolder => @"G:\_Programming\ATS Data Dump\";

    public static void DumpFull()
    {
        LogInfo("Starting Dump to JSON");

        DumpRecipes();

        LogInfo("Finished Dump to JSON");
    }

    public static void DumpRecipes()
    {
        List<SerializableRecipe> recipes = new List<SerializableRecipe>();

        foreach(var r in Dumper.DumpBuildings(null))
        {
            if(r != null)
                recipes.Add(ConvertRecipe(r));
        }

        string recipesString = JSON.ToJson(recipes);
        File.WriteAllText(Path.Combine(JsonFolder, "recipes.json"), recipesString);
    }

    public static SerializableRecipe ConvertRecipe(Dumper.RecipeRaw recipeRaw)
    {
        List<string> ing1 = new List<string>();
        List<int> count1 = new List<int>();
        List<string> ing2 = new List<string>();
        List<int> count2 = new List<int>();

        if(recipeRaw.ingredients != null)
        {
            for(int i = 0; i < recipeRaw.ingredients.Length; i++)
            {
                foreach (var g in recipeRaw.ingredients[i].goods)
                {
                    if (g != null)
                    {
                        if(i == 0)
                        {
                            ing1.Add($"{g.DisplayName}");
                            count1.Add(g.amount);
                        }
                        if(i == 1)
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
                output: (recipeRaw.output != null) ? recipeRaw.output.DisplayName : "(no output?)",
                outputCount: (recipeRaw.output != null) ? recipeRaw.output.amount : -1,
                producedBy: recipeRaw.building,
                ing1.ToArray(),
                count1.ToArray(),
                ing2.ToArray(),
                count2.ToArray(),
                0
                );


        return ret;
    }

    [System.Serializable]
    public class SerializableRecipe
    {
        public string output;
        public int tier;
        public int outputCount;
        public string producedBy;
        public string[] ingredientsFirst;
        public int[] ingredientsFirstCounts;
        public string[] ingredientsSecond;
        public int[] ingredientsSecondCounts;
        public int timeInSeconds;

        public SerializableRecipe(int tier, string output, int outputCount, string producedBy,
                                  string[] ingredientsFirst, int[] ingredientsFirstCounts,
                                  string[] ingredientsSecond, int[] ingredientsSecondCounts,
                                  int timeInSeconds)
        {
            this.tier = tier;
            this.output = output;
            this.outputCount = outputCount;
            this.producedBy = producedBy;
            this.ingredientsFirst = ingredientsFirst;
            this.ingredientsFirstCounts = ingredientsFirstCounts;
            this.ingredientsSecond = ingredientsSecond;
            this.ingredientsSecondCounts = ingredientsSecondCounts;
            this.timeInSeconds = timeInSeconds;
        }
    }
}
