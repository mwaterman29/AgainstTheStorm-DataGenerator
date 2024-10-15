using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class UtilityMethods
{
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
}

