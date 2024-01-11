﻿using System;
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

    public static List<SerializableRecipe> recipes = new List<SerializableRecipe>();
    public static List<RecipeEdge> recipeEdges = new List<RecipeEdge>();
    public static List<GoodNode> goodNodes = new List<GoodNode>();
    public static List<Building> buildings = new List<Building>();

    public static void DumpRecipes()
    {

        foreach(var r in Dumper.DumpBuildings(null))
        {
            if(r != null)
                recipes.Add(ConvertRecipe(r));
        }

        LogInfo($"All recipes dumped from original buildings, converting:");

        //Populate JSON contents

        for(int srI = 0; srI < 10; srI++)//foreach(SerializableRecipe sr in recipes)
        {
            SerializableRecipe sr = recipes[srI];
            LogInfo($"Convering recipe for {sr.output}, Tier {sr.tier} from {sr.producedBy}");

            //First, add this recipe to what the building produces:
            Building producedBy = buildings.Find(b => b.id == sr.producedBy);
            if (producedBy != null)
            {
                producedBy.produces.Add(sr);
            }
            else
            {
                Building temp = new Building(sr.producedBy, new List<SerializableRecipe>(), -1);
                temp.produces.Add(sr);
                buildings.Add(temp);
            }

            //For each possible combination of goods in the recipe, add or update an edge in the graph:
            List<string> allIngredients = new List<string>();
            allIngredients.AddRange(sr.ingredientsFirst);
            allIngredients.AddRange(sr.ingredientsSecond);

            List<int> allCounts = new List<int>();
            allCounts.AddRange(sr.ingredientsFirstCounts);
            allCounts.AddRange(sr.ingredientsSecondCounts);

            for (int i = 0; i < allIngredients.Count; i++)
            {
                string input = allIngredients[i];
                string output = sr.output;
                RecipeEdge existingEdge = recipeEdges.Find(e => e.id == $"{input}->{output}");

                if(existingEdge == null)
                {
                    existingEdge = new RecipeEdge($"{input}->{output}", input, output, string.Empty, string.Empty, string.Empty, string.Empty);
                    recipeEdges.Add(existingEdge);
                }

                //Update tiers
                switch (sr.tier)
                {
                    case 0:
                        if (!string.IsNullOrEmpty(existingEdge.RT0))
                            return;
                        existingEdge.RT0 = $"{allCounts[i]}:{sr.outputCount}|{sr.timeInSeconds}";
                        break;
                    case 1:
                        if (!string.IsNullOrEmpty(existingEdge.RT1))
                            return;
                        existingEdge.RT1 = $"{allCounts[i]}:{sr.outputCount}|{sr.timeInSeconds}";
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(existingEdge.RT2))
                            return;
                        existingEdge.RT2 = $"{allCounts[i]}:{sr.outputCount}|{sr.timeInSeconds}";
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(existingEdge.RT3))
                            return;
                        existingEdge.RT3 = $"{allCounts[i]}:{sr.outputCount}|{sr.timeInSeconds}";
                        break;
                    default:
                        LogInfo($"Error: sr {sr.output} sr.tier is{sr.tier} - outside range 0-3.");
                        return;
                }
            }

            //For each good in the recipe, add a node to the graph, if it doesn't already exist:
            foreach(string ingredient in allIngredients)
            {
                GoodNode existingNode = goodNodes.Find(gn => gn.id == ingredient);
                if(existingNode == null)
                {
                    existingNode = new GoodNode(ingredient, ingredient, new List<string>(), new List<string>());
                }
                
                if(!existingNode.producedBy.Contains($"{sr.producedBy}:T{sr.tier}"))
                    existingNode.producedBy.Add($"{sr.producedBy}:T{sr.tier}");
                if (!existingNode.usedIn.Contains(sr.output)) 
                    existingNode.usedIn.Add(sr.output);
            }
        }

        LogInfo($"All recipes dumped correctly! Converting to serializable object.");

        FinalJSONData finalJSONData = new FinalJSONData();
        finalJSONData.recipes = recipeEdges.ToArray();
        finalJSONData.goods = goodNodes.ToArray();
        finalJSONData.buildings = buildings.ToArray();

        string jsonString = JSON.ToJson(finalJSONData);

        LogInfo($"Writing {jsonString.Length} json chars to {Path.Combine(JsonFolder, "data.json")}");

        File.WriteAllText(Path.Combine(JsonFolder, "data.json"), jsonString);
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
                (int)Math.Round(recipeRaw.processingTime)
                );


        return ret;
    }

    [System.Serializable]
    public class FinalJSONData
    {
        public RecipeEdge[] recipes;
        public GoodNode[] goods;
        public Building[] buildings;
    }


    [System.Serializable]
    public class RecipeEdge
    {
        public string id;
        public string source;
        public string target;
        public string RT3;
        public string RT2;
        public string RT1;
        public string RT0;

        // Default constructor
        public RecipeEdge()
        {
        }

        // Parameterized constructor
        public RecipeEdge(string id, string source, string target, string RT3, string RT2, string RT1, string RT0)
        {
            this.id = id;
            this.source = source;
            this.target = target;
            this.RT3 = RT3;
            this.RT2 = RT2;
            this.RT1 = RT1;
            this.RT0 = RT0;
        }
    }

    [System.Serializable]
    public class GoodNode
    {
        public string id;
        public string label;
        public List<string> producedBy;
        public List<string> usedIn;

        // Default constructor
        public GoodNode()
        {
            producedBy = new List<string>();
            usedIn = new List<string>();
        }

        // Parameterized constructor
        public GoodNode(string id, string label, List<string> producedBy, List<string> usedIn)
        {
            this.id = id;
            this.label = label;
            this.producedBy = producedBy ?? new List<string>();
            this.usedIn = usedIn ?? new List<string>();
        }
    }

    [System.Serializable]
    public class Building
    {
        public string id;
        public List<SerializableRecipe> produces;
        public int workerSlots;

        // Default constructor
        public Building()
        {
            produces = new List<SerializableRecipe>();
        }

        // Parameterized constructor
        public Building(string id, List<SerializableRecipe> produces, int workerSlots)
        {
            this.id = id;
            this.produces = produces ?? new List<SerializableRecipe>();
            this.workerSlots = workerSlots;
        }
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
