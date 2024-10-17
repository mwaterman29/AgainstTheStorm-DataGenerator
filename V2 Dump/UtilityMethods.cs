using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

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

    public static void DumpImage(string jsonFolder, (string, ExtractableSpriteReference) spriteEntry)
    {
        var imageFolder = Path.Combine(jsonFolder, "img");
        if (!Directory.Exists(imageFolder))
            Directory.CreateDirectory(imageFolder);


        //If no dumpable image, return
        if (spriteEntry.Item2.source == null)
        {
            return;
        }

        string dest = Path.Combine(imageFolder, spriteEntry.Item1 + ".png");

        //If already done, cont.
        if (File.Exists(dest))
        {
            return;
        }

        var convText = DuplicateTexture(spriteEntry.Item2.source);

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
    }

    //https://forum.unity.com/threads/easy-way-to-make-texture-isreadable-true-by-script.1141915/
    public static Texture2D DuplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.sRGB);

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
}

