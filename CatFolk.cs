using Eremite;
using Eremite.Buildings;
using Eremite.MapObjects.Tooltips;
using Eremite.Model;
using Eremite.Model.Meta;
using Eremite.View.HUD;
using Eremite.View.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BubbleStormTweaks
{


    [StringProvider]
    public static class CatStrings
    {
        public static SimpleString displayName = "Catfolk";
        public static SimpleString pluralName = "Catfolk";
        public static SimpleString description = "Catfolk are a race of natural explorers who rarely tire of trailblazing, but such trailblazing is not limited merely to the search for new horizons in distant lands. Many catfolk see personal growth and development as equally valid avenues of exploration.";
        public static SimpleString resilienceLabel = "low";
        public static SimpleString demandingLabel = "low";
        public static SimpleString decadentLabel = "low";
        public static SimpleString houseName = "Cat House";
        public static SimpleString houseDesc = "A luxurious parade of hammocks and towers for Catfolk. Has to be built near a Hearth. Number of places: 2.";
    }

    public class TestController : MonoBehaviour
    {
        public void Update()
        {
            Plugin.LogInfo("test");
        }
    }

    public class CatFolk : ISettingInjector
    {

        public void Inject(Settings settings)
        {
            AssetLoader.AddBundle("bubblestorm");
            var lizard = settings.GetRace("Lizard");

            var catTag = SO.CreateInstance<ModelTag>();
            catTag.name = "cat.tag";

            var cat = SO.CreateInstance<RaceModel>();
            cat.name = "Cat";
            cat.icon = AssetLoader.LoadInternal("cat_temp", new(255, 255));
            cat.roundIcon = AssetLoader.LoadInternal("cat_circle", new(255, 255));
            cat.widePortrait = AssetLoader.LoadInternal("cat_temp", new(255, 255));
            cat.isActive = true;
            cat.isEssential = true;
            cat.displayName = CatStrings.displayName;
            cat.pluralName = CatStrings.pluralName;
            cat.description = CatStrings.description;
            cat.order = 2;
            cat.assignAction = new();
            cat.tag = catTag;
            cat.malePrefab = lizard.malePrefab;
            cat.femalePrefab = lizard.femalePrefab;
            cat.avatarClickSound = lizard.avatarClickSound;
            cat.ambient = lizard.ambient;
            cat.maleNames = new string[]
            {
            "Oliver", "Leo", "Milo", "Charlie", "Simba", "Max", "Jack", "Loki", "Tiger", "Jasper", "Ollie", "Oscar", "George", "Buddy", "Toby", "Smokey", "Finn", "Felix", "Simon", "Shadow",
            };
            cat.femaleNames = new string[]
            {
            "Luna", "Bella", "Lily", "Lucy", "Nala", "Kitty", "Chloe", "Stella", "Zoe", "Lola"
            };
            cat.initialResolve = 5;
            cat.minResolve = 0;
            cat.maxResolve = 50;
            cat.resolvePositveChangePerSec = 0.15f;
            cat.resolveNegativeChangePerSec = 0.05f;
            cat.resolveNegativeChangeDiffFactor = 0.02f;
            cat.reputationPerSec = 0.0003f;
            cat.minPopulationToGainReputation = 1;
            cat.resolveForReputationTreshold = new(15, 50);
            cat.maxReputationFromResolvePerSec = 0.025f;
            cat.reputationTresholdIncreasePerReputation = 7;
            cat.resolveToReputationRatio = 0.1f;
            cat.populationToReputationRatio = 0.7f;

            cat.resilienceLabel = CatStrings.resilienceLabel;
            cat.demandingLabel = CatStrings.demandingLabel;
            cat.decadentLabel = CatStrings.decadentLabel;
            var anyHousing = settings.Needs.First(n => n.name == "Any Housing");
            cat.needs = new NeedModel[]
            {
            anyHousing,
            };

            cat.needsInterval = 100;
            cat.hungerEffect = lizard.hungerEffect;
            cat.homelessPenalty = null;
            cat.initialEffects = Array.Empty<ResolveEffectModel>();
            cat.characteristics = new RaceCharacteristicModel[]
            {
            };

            cat.needsCategoriesLookup = new();

            var lizardHouseModel = settings.GetBuilding("Lizard House") as HouseModel;
            var catHouseModel = SO.CreateInstance<HouseModel>();

            catHouseModel.name = "Cat House";
            catHouseModel.icon = AssetLoader.LoadInternal("cat_house", new(128, 128));
            catHouseModel.category = lizardHouseModel.category;
            catHouseModel.constructionPerSec = 0.025f;
            catHouseModel.requiresConstruction = true;
            catHouseModel.refundMaterials = true;
            catHouseModel.baseRefundRate = 1.0f;
            catHouseModel.maxBuilders = 2;
            catHouseModel.hasCustomPlacingSound = false;
            catHouseModel.isInShop = true;
            catHouseModel.order = 4;
            catHouseModel.requiredPopulation = 0;
            catHouseModel.allowedTerrains = Misc.Array(FieldType.Sand, FieldType.Grass);
            var planks = settings.GetGood("[Mat Processed] Planks");
            var cloth = settings.GetGood("[Mat Processed] Fabric");
            Plugin.LogInfo(planks.Name);
            //catHouseModel.requiredGoods = Misc.Goods(
            //    new("[Mat Processed] Planks", 4),
            //    new("[Mat Processed] Fabric", 2));
            catHouseModel.requiredGoods = Misc.GoodsSingle(
                new("[Mat Raw] Wood", 1));

            catHouseModel.raceRequirements = Array.Empty<RaceRequirement>();

            catHouseModel.isActive = true;
            catHouseModel.showDebugName = false;
            catHouseModel.displayName = CatStrings.houseName;
            catHouseModel.description = CatStrings.houseDesc;
            catHouseModel.progressScore = 5;
            catHouseModel.size = new(2, 2);
            catHouseModel.traversable = false;
            catHouseModel.repeatable = true;
            catHouseModel.destroyable = true;
            catHouseModel.destroyableByEffects = true;

            catHouseModel.canBeRuined = false;
            catHouseModel.ruin = null;

            catHouseModel.movable = true;
            catHouseModel.movingCost = new Misc.GoodRefWrapper("[Mat Raw] Wood", 5).Resolved;
            catHouseModel.canBeMovedBetween = true;

            catHouseModel.prefabHeight = 1.2f;
            catHouseModel.iconXOffset = 0;
            catHouseModel.iconSizeOffset = 0.0000001f;
            catHouseModel.waitFramesForIcon = 0;

            catHouseModel.useFootprintGraphic = false;
            catHouseModel.showPlacedEffect = true;
            catHouseModel.showFinishedEffect = true;
            catHouseModel.skipOnExport = false;
            catHouseModel.skipIconGeneration = false;
            catHouseModel.tags = Array.Empty<BuildingTagModel>();
            catHouseModel.usabilityTags = Misc.Array(catTag);

            catHouseModel.initiallyEssential = true;
            catHouseModel.canBePicked = false;
            catHouseModel.costRange = new(1, 2);
            catHouseModel.requireFuel = false;

            catHouseModel.housingPlaces = 2;
            catHouseModel.decorationsRadius = 5.5f;
            catHouseModel.level = 2;
            catHouseModel.housingRaces = Misc.Array(cat);
            catHouseModel.servedNeeds = Array.Empty<NeedModel>();


            //var metaRewardHarpyHouse = settings.GetMetaReward("Meta Reward Essential Harpy House") as EssentialBuildingMetaRewardModel;

            //var catHouseReward = SO.CreateInstance<EssentialBuildingMetaRewardModel>();
            //catHouseReward.label = metaRewardHarpyHouse.label;
            //catHouseReward.building = catHouseModel;
            //catHouseReward.forceUnlock = false;
            //catHouseReward.displayName = metaRewardHarpyHouse.displayName;
            //catHouseReward.description = metaRewardHarpyHouse.description;


            //settings.metaConfig.levels[4].rewards


            /**
             * 
                            ],
              "prefab": {
                "m_FileID": 0,
                "m_PathID": 145664
              }
*/


            var lizHouse = GameObject.Instantiate<House>(lizardHouseModel.prefab);
            //catHouse.view = GameObject.Instantiate(catHouse.view);
            //Plugin.LogInfo(catHouse.view.animator);
            //animator.transform.Translate(Vector3.up * 4.0f);
            //GameObject.DontDestroyOnLoad(catHouse.gameObject);
            //GameObject.DontDestroyOnLoad(catHouse.view.gameObject);

            GameObject prefab = AssetLoader.LoadAsset<GameObject>("Lizard House 1.prefab");
            GameObject.DontDestroyOnLoad(prefab);
            GameObject.DontDestroyOnLoad(lizHouse);
            var catHouseBuilding = prefab.AddComponent<House>();
            var catHouseView = prefab.AddComponent<HouseView>();

            catHouseBuilding.model = catHouseModel;
            catHouseBuilding.view = catHouseView;
            catHouseBuilding.view.panelBackgroundSound = lizHouse.view.panelBackgroundSound;

            var construction = prefab.AddComponent<DummyConstructionAnimator>();
            catHouseBuilding.view.constructionAnimator = construction;

            catHouseBuilding.entrance = prefab.transform.Find("ToRotate/Entrance");

            catHouseBuilding.view.planOverlay = prefab.AddComponent<HousePlanOverlay>();
            catHouseBuilding.view.planOverlay.lastFrameRect = new(60, 65, 2, 2);
            catHouseBuilding.view.planOverlay.availableColor = new(1.0f, 0.8f, 0.2f);
            catHouseBuilding.view.planOverlay.blockedColor = new(1.0f, 0.0f, 102f);

            catHouseBuilding.view.iconsLayout = prefab.transform.Find("UI").gameObject.AddComponent<SpritesLayout>();
            catHouseBuilding.view.iconsLayout.iconSize = 0.7f;
            catHouseBuilding.view.iconsLayout.padding = 0.2f;
            catHouseBuilding.view.iconsLayout.elements = Misc.Array<GameObject>(
                    prefab.transform.Find("UI/NoWorkersIcon").gameObject,
                    prefab.transform.Find("UI/NoBuildersIcon").gameObject,
                    prefab.transform.Find("UI/NoGoodsIcon").gameObject,
                    prefab.transform.Find("UI/IdleIcon").gameObject,
                    prefab.transform.Find("UI/ProblemIcon").gameObject
                );

            catHouseBuilding.view.entranceIcon = prefab.transform.Find("ToRotate/Entrance/Icon").gameObject;
            catHouseBuilding.view.noHearthIcon = prefab.transform.Find("UI/ProblemIcon").gameObject;
            catHouseBuilding.view.rotationParent = prefab.transform.Find("ToRotate");
            catHouseBuilding.view.noBuildersIcon = prefab.transform.Find("UI/NoBuildersIcon").gameObject;
            catHouseBuilding.view.noGoodsIcon = prefab.transform.Find("UI/NoGoodsIcon").gameObject;
            catHouseBuilding.view.uiParent = prefab.transform.Find("UI");


            //Re-hydrate icon tooltip stuff
            var iconParent = prefab.transform.Find("UI");
            for (int i = 0; i < iconParent.childCount; i++)
            {
                var icon = iconParent.GetChild(i);

                //bleh
                if (icon.name == "DebugName") continue;

                var trigger = icon.GetChild(1);
                var target = trigger.GetChild(0);

                var collider = trigger.gameObject.AddComponent<BoxCollider>();

                var tooltipTrigger = trigger.gameObject.AddComponent<SimpleTooltipWorldTrigger>();
                tooltipTrigger.descKey = CatStrings.houseDesc.Key;
                tooltipTrigger.headerKey = CatStrings.houseName.Key;
                tooltipTrigger.target = target;


                icon.gameObject.AddComponent<LookAtCamera>();
                var mapObject = icon.gameObject.AddComponent<MapObjectIcon>();
                mapObject.tooltipCollider = collider;
            }


            catHouseModel.prefab = catHouseBuilding;// lizardHouseModel.prefab;

            settings.AddRace(cat);
            settings.AddBuilding(catHouseModel);

            Misc.Append(ref settings.metaConfig.initialBuildings, catHouseModel);
        }
    }

    public class DummyConstructionAnimator : ConstructionAnimator
    {
        public override bool IsReady => true;

        public override void Dispose()
        {
        }

        public override void SetUpBuildingProcess(Vector2Int size)
        {
        }

        public override void UpdateScaffoldings(float buildingProgress)
        {
        }
    }
}
