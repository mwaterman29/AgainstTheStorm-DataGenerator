using System;
using System.Collections.Generic;
using System.Text;

namespace ATSDumpV2
{

    [System.Serializable]
    public class ItemUsage
    {
        public string id;
        public int count;

        public ItemUsage() { }

        public ItemUsage(string id, int count)
        {
            this.id = id;
            this.count = count;
        }
    }

    [System.Serializable]
    public class ProducedBy
    {
        public string building;
        public int tier;

        public ProducedBy() { }

        public ProducedBy(string building, int tier)
        {
            this.building = building;
            this.tier = tier;
        }
    }

    [System.Serializable]
    public class Item
    {
        public string id;
        public string label;
        public string category;
        public List<string> usesFirst;
        public List<string> usesSecond;
        public List<string> usedIn;

        public Item()
        {
            usesFirst = new List<string>();
            usesSecond = new List<string>();
            usedIn = new List<string>();
        }

        public Item(string id, string label, List<string> usesFirst, List<string> usesSecond, List<string> usedIn)
        {
            this.id = id;
            this.label = label;
            this.usesFirst = usesFirst ?? new List<string>();
            this.usesSecond = usesSecond ?? new List<string>();
            this.usedIn = usedIn ?? new List<string>();
        }
    }

    [System.Serializable]
    public class RecipeTier
    {
        public int inputCount;
        public int outputCount;
        public int duration;

        public RecipeTier() { }

        public RecipeTier(int inputCount, int outputCount, int duration)
        {
            this.inputCount = inputCount;
            this.outputCount = outputCount;
            this.duration = duration;
        }
    }

    [System.Serializable]
    public class Recipe
    {
        public string id;
        public string source;
        public string target;
        public Dictionary<int, RecipeTier> tiers;

        public Recipe()
        {
            tiers = new Dictionary<int, RecipeTier>();
        }

        public Recipe(string id, string source, string target, Dictionary<int, RecipeTier> tiers)
        {
            this.id = id;
            this.source = source;
            this.target = target;
            this.tiers = tiers ?? new Dictionary<int, RecipeTier>();
        }
    }

    [System.Serializable]
    public class ProductionBuilding
    {
        public string id;
        public List<SerializableRecipe> produces;
        public string category;
        public int workerSlots;

        public ProductionBuilding()
        {
            produces = new List<SerializableRecipe>();
        }

        public ProductionBuilding(string id, List<SerializableRecipe> produces, int workerSlots)
        {
            this.id = id;
            this.produces = produces ?? new List<SerializableRecipe>();
        }
    }

    [System.Serializable]
    public class GladeEvent
    {
        public string id;
        public string label;
        public string description;
        public string category;
        public string threat;
        public string workingEffect;
        public int baseSolveTime;
        public List<ItemUsage> solveOptions1;
        public List<ItemUsage> solveOptions2;
        public List<ItemUsage> reward;

        public GladeEvent()
        {
            solveOptions1 = new List<ItemUsage>();
            solveOptions2 = new List<ItemUsage>();
            reward = new List<ItemUsage>();
        }

        public GladeEvent(string id, string label, string description, string category, string threat, string workingEffect, int baseSolveTime, List<ItemUsage> solveOptions1, List<ItemUsage> solveOptions2, List<ItemUsage> reward)
        {
            this.id = id;
            this.label = label;
            this.description = description;
            this.category = category;
            this.threat = threat;
            this.workingEffect = workingEffect;
            this.baseSolveTime = baseSolveTime;
            this.solveOptions1 = solveOptions1 ?? new List<ItemUsage>();
            this.solveOptions2 = solveOptions2 ?? new List<ItemUsage>();
            this.reward = reward ?? new List<ItemUsage>();
        }
    }

    [System.Serializable]
    public class CornerstoneSale
    {
        public string traderId;
        public string ItemId;
        public float price;
        public float weight;

        public CornerstoneSale() { }

        public CornerstoneSale(string traderId, string ItemId, float price, float weight)
        {
            this.traderId = traderId;
            this.ItemId = ItemId;
            this.price = price;
            this.weight = weight;
        }
    }

    [System.Serializable]
    public class Cornerstone
    {
        public string id;
        public string label;
        public string description;
        public string tier;
        public string type;
        public List<string> biomeLock;
        public List<CornerstoneSale> soldBy;
        public List<string> tags;

        public Cornerstone()
        {
            biomeLock = new List<string>();
            soldBy = new List<CornerstoneSale>();
            tags = new List<string>();
        }

        public Cornerstone(string id, string label, string description, string tier, string type, List<string> biomeLock, List<CornerstoneSale> soldBy)
        {
            this.id = id;
            this.label = label;
            this.description = description;
            this.tier = tier;
            this.type = type;
            this.biomeLock = biomeLock ?? new List<string>();
            this.soldBy = soldBy ?? new List<CornerstoneSale>();
        }
    }

    [Serializable]
    public class Effect
    {
        public string id;
        public string label;
        public string description;
        public string rarity;
    }

    [Serializable]
    public class BuildingEffect
    {
        public string id;
        public int minWorkerCount;
    }

    [Serializable]
    public class Building
    {
        public string id;
        public string label;
        public string description;
        public string category;
        public List<BuildingEffect> effects;
    }

    [Serializable]
    public class Order
    {
        public string Name { get; set; }
        public List<OrderLogicSet> LogicSets { get; set; }
        public List<string> Rewards { get; set; }
        public List<string> ExcludedBiomes { get; set; }
        public string ReputationReward { get; set; }
    }

    [Serializable]
    public class OrderLogicSet
    {
        public string Difficulty { get; set; }
        public List<string> Logics { get; set; }
        public List<string> Rewards { get; set; }
    }

    [Serializable]
    public class Biome
    {
        public string name;
        public List<string> treeItems;
        public List<string> depositItems;
    }

    [Serializable]
    public class Species
    {
        public string name;
        public string[] needs;
    }
}
