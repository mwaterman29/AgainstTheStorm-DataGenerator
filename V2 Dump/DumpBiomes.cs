using System;
using System.Collections.Generic;
using System.Text;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using System.Linq;
using BubbleStormTweaks;


namespace ATSDumpV2
{
    class DumpBiomes
    {
        public static void LogInfo(object data) => Plugin.LogInfo(data);

        //There are so few, this can be done in one step
        public static void DumpAllBiomes()
        {
            foreach (var biome in Serviceable.Settings.biomes)
            {
                LogInfo($"biome: {biome.name}");

                List<GoodModel> allAccessibleGoods = new List<GoodModel>();
                allAccessibleGoods.AddRange(biome.GetTreesGoods());
                allAccessibleGoods.AddRange(biome.GetDepositsGoods());

                List<string> goodNames = allAccessibleGoods.Select(item => item.Name).ToList();

                LogInfo(string.Join(",", goodNames));

            }
        }
    }
}
