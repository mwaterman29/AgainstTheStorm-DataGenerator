using System;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class GoodUsage
{
    public string id;
    public int count;

    public GoodUsage() { }

    public GoodUsage(string id, int count)
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
public class Good
{
    public string id;
    public string label;
    public List<GoodUsage> usesFirst;
    public List<GoodUsage> usesSecond;
    public List<string> usedIn;

    public Good()
    {
        usesFirst = new List<GoodUsage>();
        usesSecond = new List<GoodUsage>();
        usedIn = new List<string>();
    }

    public Good(string id, string label, List<GoodUsage> usesFirst, List<GoodUsage> usesSecond, List<string> usedIn)
    {
        this.id = id;
        this.label = label;
        this.usesFirst = usesFirst ?? new List<GoodUsage>();
        this.usesSecond = usesSecond ?? new List<GoodUsage>();
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
public class Building
{
    public string id;
    public List<Recipe> produces;
    public int workerSlots;

    public Building()
    {
        produces = new List<Recipe>();
    }

    public Building(string id, List<Recipe> produces, int workerSlots)
    {
        this.id = id;
        this.produces = produces ?? new List<Recipe>();
        this.workerSlots = workerSlots;
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
    public List<GoodUsage> solveOptions1;
    public List<GoodUsage> solveOptions2;
    public List<GoodUsage> reward;

    public GladeEvent()
    {
        solveOptions1 = new List<GoodUsage>();
        solveOptions2 = new List<GoodUsage>();
        reward = new List<GoodUsage>();
    }

    public GladeEvent(string id, string label, string description, string category, string threat, string workingEffect, int baseSolveTime, List<GoodUsage> solveOptions1, List<GoodUsage> solveOptions2, List<GoodUsage> reward)
    {
        this.id = id;
        this.label = label;
        this.description = description;
        this.category = category;
        this.threat = threat;
        this.workingEffect = workingEffect;
        this.baseSolveTime = baseSolveTime;
        this.solveOptions1 = solveOptions1 ?? new List<GoodUsage>();
        this.solveOptions2 = solveOptions2 ?? new List<GoodUsage>();
        this.reward = reward ?? new List<GoodUsage>();
    }
}

[System.Serializable]
public class CornerstoneSale
{
    public string traderId;
    public string goodId;
    public float price;
    public float weight;

    public CornerstoneSale() { }

    public CornerstoneSale(string traderId, string goodId, float price, float weight)
    {
        this.traderId = traderId;
        this.goodId = goodId;
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

    public Cornerstone()
    {
        biomeLock = new List<string>();
        soldBy = new List<CornerstoneSale>();
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