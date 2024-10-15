using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

public class ExtractableSpriteReference
{
    public Texture2D source;
    public float source_w;
    public float source_h;
    public float source_x;
    public float source_y;
}
