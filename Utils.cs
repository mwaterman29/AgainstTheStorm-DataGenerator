using Eremite.Buildings;
using Eremite.Characters.Villagers;
using Eremite.Controller;
using Eremite.Controller.Generator;
using Eremite.Model;
using Eremite.Model.Configs;
using Eremite.Model.Goals;
using Eremite.Model.LandPatches;
using Eremite.Model.Meta;
using Eremite.Model.Orders;
using Eremite.Model.Trade;
using Eremite.WorldMap;
using Eremite.WorldMap.Model;
using HarmonyLib;
using System;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace BubbleStormTweaks
{
    public static class Misc
    {
        public static void Append<T>(ref T[] arr, T newValue)
        {
            arr = arr.AddToArray(newValue);
        }

        public static LocaText Loc(this SimpleString s) => new() { key = s.Key };

        internal static T[] Array<T>(params T[] vals)
        {
            return vals;
        }

        internal static GoodRef[] GoodsSingle(GoodRefWrapper good)
        {
            return Array(good.Resolved);
        }

        internal static GoodRef[] Goods(params GoodRefWrapper[] goods)
        {
            return goods.Select(g => g.Resolved).ToArray();
        }

        internal class GoodRefWrapper
        {
            public readonly string name;
            public readonly int amount;

            public GoodRefWrapper(string name, int amount)
            {
                this.name = name;
                this.amount = amount;
            }

            public GoodRef Resolved => new() { good = Plugin.GameSettings.GetGood(name), amount = amount };
        }
    }
    public class AssetLoader
    {
        // Loosely based on https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        public static class Image2Sprite
        {
            public static string icons_folder = "";

            public static Sprite Create(string filePath, Vector2Int size)
            {
                var bytes = File.ReadAllBytes(icons_folder + filePath);
                var texture = new Texture2D(size.x, size.y, TextureFormat.DXT5, false);
                _ = texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, size.x, size.y), new Vector2(0, 0));
            }
        }

        public static Dictionary<string, GameObject> Objects = new();
        public static Dictionary<string, Sprite> Sprites = new();
        public static Dictionary<string, Mesh> Meshes = new();
        public static Dictionary<string, Material> Materials = new();

        public static void RemoveBundle(string bundleName, bool unloadAll = false)
        {
            AssetBundle bundle;
            if (bundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(x => x.name == bundleName))
                bundle.Unload(unloadAll);
            if (unloadAll)
            {
                Objects.Clear();
                Sprites.Clear();
                Meshes.Clear();
            }
        }

        public static UnityEngine.Object[] Assets;

        private static Dictionary<string, ResourceReference> assets = new();

        public static T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            name = $"assets/stuff/{name.ToLower()}";
            if (assets.TryGetValue(name, out var reference))
            {
                if (reference.value == null)
                {
                    Plugin.LogInfo("Lazily loading: " + name + " from bundle: " + reference.bundle.name);
                    reference.value = reference.bundle.LoadAsset<T>(name);
                }
                return reference.value as T;
            }
            else
            {
                Plugin.LogInfo("No such asset with name: " + name);
                return null;
            }
        }

        public class ResourceReference
        {
            public AssetBundle bundle;
            public UnityEngine.Object value = null;
        };
    }

    public static class SettingsHelper
    {

        public static void AddOrderTier(this Settings raw, OrderTierModel item) => Misc.Append(ref raw.ordersTiers, item);
        public static void AddGoal(this Settings raw, GoalModel item) => Misc.Append(ref raw.goals, item);
        public static void AddLandPatch(this Settings raw, LandPatchModel item) => Misc.Append(ref raw.landPatches, item);
        public static void AddTrader(this Settings raw, TraderModel item) => Misc.Append(ref raw.traders, item);
        public static void AddEffect(this Settings raw, EffectModel item) => Misc.Append(ref raw.effects, item);
        public static void AddRace(this Settings raw, RaceModel item) => Misc.Append(ref raw.Races, item);
        public static void AddMetaReward(this Settings raw, MetaRewardModel item) => Misc.Append(ref raw.metaRewards, item);
        public static void AddCapitalUpgrade(this Settings raw, CapitalUpgradeModel item) => Misc.Append(ref raw.capitalUpgrades, item);
        public static void AddCapitalStructure(this Settings raw, CapitalStructureModel item) => Misc.Append(ref raw.capitalStructures, item);
        public static void AddOrder(this Settings raw, OrderModel item) => Misc.Append(ref raw.orders, item);
        public static void AddAltarEffect(this Settings raw, AltarEffectModel item) => Misc.Append(ref raw.altarEffects, item);
        public static void AddResolveEffect(this Settings raw, ResolveEffectModel item) => Misc.Append(ref raw.resolveEffects, item);
        public static void AddModifier(this Settings raw, ModifierModel item) => Misc.Append(ref raw.modifiers, item);
        public static void AddResourceDeposit(this Settings raw, ResourceDepositModel item) => Misc.Append(ref raw.ResourcesDeposits, item);
        public static void AddNaturalResource(this Settings raw, NaturalResourceModel item) => Misc.Append(ref raw.NaturalResources, item);
        public static void AddGoodCategory(this Settings raw, GoodCategoryModel item) => Misc.Append(ref raw.GoodsCategories, item);
        public static void AddGood(this Settings raw, GoodModel item) => Misc.Append(ref raw.Goods, item);
        public static void AddFaction(this Settings raw, FactionModel item) => Misc.Append(ref raw.factions, item);
        public static void AddAscensionModifier(this Settings raw, AscensionModifierModel item) => Misc.Append(ref raw.ascensionModifiers, item);
        public static void AddChallangeModifier(this Settings raw, ChallangeModifierModel item) => Misc.Append(ref raw.challangeModifiers, item);
        public static void AddWorldEvent(this Settings raw, WorldEventModel item) => Misc.Append(ref raw.worldEvents, item);
        public static void AddBiome(this Settings raw, BiomeModel item) => Misc.Append(ref raw.biomes, item);
        public static void AddMetaCurrency(this Settings raw, MetaCurrencyModel item) => Misc.Append(ref raw.metaCurrencies, item);
        public static void AddProfession(this Settings raw, ProfessionModel item) => Misc.Append(ref raw.Professions, item);
        public static void AddNeed(this Settings raw, NeedModel item) => Misc.Append(ref raw.Needs, item);
        public static void AddOre(this Settings raw, OreModel item) => Misc.Append(ref raw.Ore, item);
        public static void AddDifficulty(this Settings raw, DifficultyModel item) => Misc.Append(ref raw.difficulties, item);
        public static void AddWikiCategory(this Settings raw, WikiCategory item) => Misc.Append(ref raw.wikiCategories, item);
        public static void AddWikiTopic(this Settings raw, WikiTopic item) => Misc.Append(ref raw.wikiTopics, item);
        public static void AddMapData(this Settings raw, MapData item) => Misc.Append(ref raw.maps, item);
        public static void AddHubTier(this Settings raw, HubTier item) => Misc.Append(ref raw.hubsTiers, item);
        public static void AddBuildingTag(this Settings raw, BuildingTagModel item) => Misc.Append(ref raw.buildingsTags, item);
        public static void AddVillagerPerk(this Settings raw, VillagerPerkModel item) => Misc.Append(ref raw.villagersPerks, item);
        public static void AddDecorationTier(this Settings raw, DecorationTier item) => Misc.Append(ref raw.decorationsTiers, item);
        public static void AddRecipeGrade(this Settings raw, RecipeGradeModel item) => Misc.Append(ref raw.recipesGrades, item);
        public static void AddRecipe(this Settings raw, RecipeModel item) => Misc.Append(ref raw.recipes, item);
        public static void AddWorkshopRecipe(this Settings raw, WorkshopRecipeModel item) => Misc.Append(ref raw.workshopsRecipes, item);
        public static void AddBlightPostRecipe(this Settings raw, BlightPostRecipeModel item) => Misc.Append(ref raw.blightPostsRecipes, item);
        public static void AddCampRecipe(this Settings raw, CampRecipeModel item) => Misc.Append(ref raw.campsRecipes, item);
        public static void AddGathererHutRecipe(this Settings raw, GathererHutRecipeModel item) => Misc.Append(ref raw.gatherersHutsRecipes, item);
        public static void AddMineRecipe(this Settings raw, MineRecipeModel item) => Misc.Append(ref raw.minesRecipes, item);
        public static void AddCollectorRecipe(this Settings raw, CollectorRecipeModel item) => Misc.Append(ref raw.collectorsRecipes, item);
        public static void AddFarmRecipe(this Settings raw, FarmRecipeModel item) => Misc.Append(ref raw.farmsRecipes, item);
        public static void AddInstitutionRecipe(this Settings raw, InstitutionRecipeModel item) => Misc.Append(ref raw.institutionRecipes, item);
        public static void AddBuilding(this Settings raw, BuildingModel item)
        {
            Misc.Append(ref raw.Buildings, item);
            if (raw.buildingsCache.cache != null)
                raw.buildingsCache.cache.Add(item.Name, item);
        }
        public static void AddRelic(this Settings raw, RelicModel item) => Misc.Append(ref raw.Relics, item);
        public static void AddWorkshop(this Settings raw, WorkshopModel item) => Misc.Append(ref raw.workshops, item);
        public static void AddInstitution(this Settings raw, InstitutionModel item) => Misc.Append(ref raw.Institutions, item);
        public static void AddBuildingCategory(this Settings raw, BuildingCategoryModel item) => Misc.Append(ref raw.BuildingCategories, item);


    }
}