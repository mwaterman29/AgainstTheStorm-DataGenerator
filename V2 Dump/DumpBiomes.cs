using System;
using System.Collections.Generic;
using System.Text;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using System.Linq;
using BubbleStormTweaks;
using System.Text.RegularExpressions;


namespace ATSDumpV2
{
    class DumpBiomes
    {
        public static void LogInfo(object data) => Plugin.LogInfo(data);

        static string goodPattern = @"\[[^\]]*\]\s*";

        //There are so few, this can be done in one step
        public static bool DumpAllBiomes(List<(string, ExtractableSpriteReference)> sprites, List<Biome> biomes)
        {
            foreach (var biome in Serviceable.Settings.biomes)
            {
                Biome outputBiome = new Biome();
                outputBiome.name = biome.displayName.GetText();  //biome.Name;
                outputBiome.treeItems = new List<string>();
                outputBiome.depositItems = new List<string>();

                ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(biome.icon);
                sprites.Add((outputBiome.name, sr));

                var trees = biome.GetTreesGoods();
                foreach(var good in trees)
                {
                    string formattedName = Regex.Replace(good.Name, goodPattern, "").Trim();
                    outputBiome.treeItems.Add(formattedName);
                }

                var deposits = biome.GetDepositsGoods();
                foreach (var good in deposits)
                {
                    string formattedName = Regex.Replace(good.Name, goodPattern, "").Trim();
                    outputBiome.depositItems.Add(formattedName);
                }

                biomes.Add(outputBiome);
            }

            return true;
        }
    }
}
