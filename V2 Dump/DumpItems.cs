using System;
using System.Collections.Generic;
using System.Text;
using BubbleStormTweaks;
using Eremite.Buildings;
using System.Linq;

namespace ATSDumpV2
{
    class DumpItems
    {
        public static int itemIndex = 0;
        public static int itemStepSize = 5;

        public static void LogInfo(object data) => Plugin.LogInfo(data);

        public static bool Step(List<(string, ExtractableSpriteReference)> sprites, List<Item> outputItems)
        {

            var allGoods = Plugin.GameSettings.Goods;
            int thisStepMax = Math.Min(allGoods.Length, itemIndex + itemStepSize);

            LogInfo($"[Items] Dumping from {itemIndex} to {thisStepMax} of total {allGoods.Length}");
            for (; itemIndex < thisStepMax; itemIndex++)
            {
                var goodToDump = allGoods[itemIndex];

                var outputItem = new Item();

                //the [category] name annoys me
                outputItem.id = goodToDump.displayName.Text; // goodToDump.Name;

                LogInfo($"[Items] Dumping item {goodToDump.Name} ...");

                outputItem.label = goodToDump.displayName.Text;
                outputItem.category = goodToDump.category.Name;
                outputItem.usesFirst = new List<string>();
                outputItem.usesSecond = new List<string>();
                outputItem.usedIn = new List<string>();

                ExtractableSpriteReference sr = UtilityMethods.GetSpriteRef(goodToDump.icon);
                sprites.Add((outputItem.id, sr));

                outputItems.Add(outputItem);
            }

            return itemIndex == allGoods.Length;
        }
    }
}