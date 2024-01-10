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
        List<string> ing = new List<string>();
        if(recipeRaw.ingredients != null)
        {
            foreach (var gs in recipeRaw.ingredients)
            {
                if(gs != null)
                {
                    foreach (var g in gs.goods)
                    {
                        if (g != null)
                            ing.Add($"{g.amount}:{g.Name}");
                        else
                            ing.Add($"(null)");
                    }
                }
            }
        }

        var ret = new SerializableRecipe(
                recipeRaw.tier.Count(c => c == '★'),
                (recipeRaw.output != null) ? recipeRaw.output.Name : "(no output?)",
                (recipeRaw.output != null) ? recipeRaw.output.amount : -1,
                ing.ToArray(),
                new int[] { },
                new string[] { },
                new int[] { },
                0
                );

        return ret;
    }

    [System.Serializable]
    public class SerializableRecipe
    {
        public int tier;
        public string output;
        public int outputCount;
        public string[] ingredientsFirst;
        public int[] ingredientsFirstCounts;
        public string[] ingredientsSecond;
        public int[] ingredientsSecondCounts;
        public int timeInSeconds;
        public SerializableRecipe(int tier, string output, int outputCount,
                          string[] ingredientsFirst, int[] ingredientsFirstCounts,
                          string[] ingredientsSecond, int[] ingredientsSecondCounts,
                          int timeInSeconds)
        {
            this.tier = tier;
            this.output = output;
            this.outputCount = outputCount;
            this.ingredientsFirst = ingredientsFirst;
            this.ingredientsFirstCounts = ingredientsFirstCounts;
            this.ingredientsSecond = ingredientsSecond;
            this.ingredientsSecondCounts = ingredientsSecondCounts;
            this.timeInSeconds = timeInSeconds;
        }
    }
}
