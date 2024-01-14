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
        if(!started)
        {
            started = true;
            LogInfo("Starting Dump to JSON");
        }

        if(srI == 0 || srI <= recipes.Count)
        {
            DumpRecipes();
        }
        if(srI != 0 && srI >= recipes.Count)
        {
            LogInfo("Finished Dump to JSON");
        }
    }

    public static List<Dumper.RecipeRaw> rawRecipes = new List<Dumper.RecipeRaw>();
    public static List<SerializableRecipe> recipes = new List<SerializableRecipe>();
    public static List<RecipeEdge> recipeEdges = new List<RecipeEdge>();
    public static List<GoodNode> goodNodes = new List<GoodNode>();
    public static List<Building> buildings = new List<Building>();
    public static List<(string, ExtractableSpriteReference)> sprites = new List<(string, ExtractableSpriteReference)>();
    public static int srI;
    public static int imgI = 0;
    public static bool started = false;

    public static void DumpRecipes()
    {
        if(rawRecipes.Count == 0 || recipes.Count == 0)
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

                        ExtractableSpriteReference sr = GetSpriteRef(good.Icon);
                        sprites.Add((good.DisplayName, sr));
                    }
                }

                if(r.output == null || r.output.DisplayName == null || r.output.Icon == null)
                {
                    LogInfo($"Skipping no output");
                }
                else
                {
                    ExtractableSpriteReference sr = GetSpriteRef(r.output.Icon);
                    sprites.Add((r.output.DisplayName, sr));
                }

            }

            LogInfo($"All recipes dumped from original buildings, converting:");
        }

        //Populate JSON contents
        int thisStepMax = Math.Min(recipes.Count, srI + 10);
        LogInfo($"Dumping from {srI} to {thisStepMax} of total {recipes.Count}");
        for(; srI < thisStepMax; srI++)//foreach(SerializableRecipe sr in recipes)
        {

            SerializableRecipe sr = recipes[srI];

            if (sr.output == "(no output)")
                continue;

            LogInfo($"Convering recipe for {sr.output}, Tier {sr.tier} from {sr.producedBy} [{srI}]");
            try
            {
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

                if(allIngredients.Count != allCounts.Count)
                {
                    LogInfo($"Count mismatch: {allIngredients.Count} to {allCounts.Count}!");
                    return;
                }

                for (int i = 0; i < allIngredients.Count; i++)
                {
                    string input = allIngredients[i];
                    string output = sr.output;
                    RecipeEdge existingEdge = recipeEdges.Find(e => e.id == $"{input}->{output}");

                    if (existingEdge == null)
                    {
                        existingEdge = new RecipeEdge($"{input}->{output}", input, output, string.Empty, string.Empty, string.Empty, string.Empty);
                        recipeEdges.Add(existingEdge);
                    }

                    //Update tiers
                    switch (sr.tier)
                    {
                        case 0:
                            existingEdge.RT0 = $"{allCounts[i]}:{sr.outputCount}|{sr.timeInSeconds}";
                            break;
                        case 1:
                            existingEdge.RT1 = $"{allCounts[i]}:{sr.outputCount}|{sr.timeInSeconds}";
                            break;
                        case 2:
                            existingEdge.RT2 = $"{allCounts[i]}:{sr.outputCount}|{sr.timeInSeconds}";
                            break;
                        case 3:
                            existingEdge.RT3 = $"{allCounts[i]}:{sr.outputCount}|{sr.timeInSeconds}";
                            break;
                        default:
                            LogInfo($"Error: sr {sr.output} sr.tier is{sr.tier} - outside range 0-3.");
                            return;
                    }
                }

                //For each good in the recipe, add a node to the graph, if it doesn't already exist:
                foreach (string ingredient in allIngredients)
                {
                    GoodNode existingNode = goodNodes.Find(gn => gn.id == ingredient);
                    if (existingNode == null)
                    {
                        existingNode = new GoodNode(ingredient, ingredient, new List<string>(), new List<string>());
                        goodNodes.Add(existingNode);
                    }

                    //if (!existingNode.producedBy.Contains($"{sr.producedBy}:T{sr.tier}"))
                    //    existingNode.producedBy.Add($"{sr.producedBy}:T{sr.tier}");
                    if (!existingNode.usedIn.Contains(sr.output))
                        existingNode.usedIn.Add(sr.output);
                }

                //Check output as well
                GoodNode existingNodeOut = goodNodes.Find(gn => gn.id == sr.output);
                if (existingNodeOut == null)
                {
                    existingNodeOut = new GoodNode(sr.output, sr.output, new List<string>(), new List<string>());
                    goodNodes.Add(existingNodeOut);
                }

                //Ensure recipe ingredients are appended when they're found
                if (existingNodeOut.usesFirst == null)
                    existingNodeOut.usesFirst = new List<string>();
                if (existingNodeOut.usesSecond == null)
                    existingNodeOut.usesSecond = new List<string>();
                existingNodeOut.usesFirst.AddRange(sr.ingredientsFirst);
                existingNodeOut.usesFirst = existingNodeOut.usesFirst.Distinct().ToList();
                existingNodeOut.usesSecond.AddRange(sr.ingredientsSecond);
                existingNodeOut.usesSecond = existingNodeOut.usesSecond.Distinct().ToList();

                if (!existingNodeOut.producedBy.Contains($"{sr.producedBy}:T{sr.tier}"))
                    existingNodeOut.producedBy.Add($"{sr.producedBy}:T{sr.tier}");

            }
            catch(Exception e)
            {
                LogInfo($"Error @ {srI}: {e.Message}");
            }
           
        }

        if (srI == recipes.Count)
        {
            LogInfo($"All recipes dumped correctly! Converting to serializable object.");

            FinalJSONData finalJSONData = new FinalJSONData();
            finalJSONData.recipes = recipeEdges.ToArray();
            finalJSONData.goods = goodNodes.ToArray();
            finalJSONData.buildings = buildings.ToArray();

            string jsonString = JSON.ToJson(finalJSONData);

            LogInfo($"Writing {jsonString.Length} json chars to {Path.Combine(JsonFolder, "data.json")}");

            File.WriteAllText(Path.Combine(JsonFolder, "data.json"), jsonString);
        }
    }

    public static void DumpImages()
    {

        LogInfo($"Dumping all images: {imgI} / {sprites.Count}");

        var imageFolder = Path.Combine(JsonFolder, "img");
        if (!Directory.Exists(imageFolder))
            Directory.CreateDirectory(imageFolder);

        var spriteEntry = sprites[imgI];

        if (spriteEntry.Item2.source == null)
        {
            imgI++;
            return;
        }
        string dest = Path.Combine(imageFolder, spriteEntry.Item1 + ".png");

        if (File.Exists(dest))
        {
            imgI++;
            return;
        }

        var convText = duplicateTexture(spriteEntry.Item2.source);

        //Extract region from texture
        // Create a new texture for the extracted region
        Texture2D extractedTexture = new Texture2D((int)spriteEntry.Item2.source_w, (int)spriteEntry.Item2.source_h);

        // Define the region to extract
        Rect sourceRect = new Rect(spriteEntry.Item2.source_x, spriteEntry.Item2.source_y, spriteEntry.Item2.source_w, spriteEntry.Item2.source_h);

        // Read pixels from the original texture into the extracted texture
        Color[] pixels = convText.GetPixels((int)sourceRect.x, (int)sourceRect.y, (int)sourceRect.width, (int)sourceRect.height);
        extractedTexture.SetPixels(pixels);

        // Apply changes to the extracted texture
        extractedTexture.Apply();

        var imageByteData = ImageConversion.EncodeToPNG(extractedTexture);
        File.WriteAllBytes(dest, imageByteData);

        imgI++;
    }

    public static ExtractableSpriteReference GetSpriteRef(Sprite icon)
    {
        ExtractableSpriteReference spriteRef = new()
        {
            source = icon.texture,
            source_w = icon.textureRect.width,
            source_h = icon.textureRect.height,
            source_x = icon.textureRect.x,
            source_y = icon.textureRect.y,
        };

        return spriteRef;
    }

    public class ExtractableSpriteReference
    {
        public Texture2D source;
        public float source_w;
        public float source_h;
        public float source_x;
        public float source_y;
    }

    //https://forum.unity.com/threads/easy-way-to-make-texture-isreadable-true-by-script.1141915/
    public static Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
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
        public List<string> usesFirst;
        public List<string> usesSecond;
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
