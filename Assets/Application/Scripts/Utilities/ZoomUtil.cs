using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class ZoomUtil
    {
        public enum State
    {
        ZoomIn,
        ZoomOut,
        Reset
    }
    
    public static int currentZoomIndex = 0;

    private static readonly float[] zoomSteps = { 1f, 1.1f, 1.25f, 1.5f, 1.75f, 2f, 2.5f, 3f, 4f };

    public static void Zoom(Material material, GameObject gameObject, Texture2D texture, State zoom, Action<string> zoomText)
    {
        switch (zoom)
        {
           case State.ZoomIn:
               if (currentZoomIndex < zoomSteps.Length - 1)
               {
                   currentZoomIndex++;
                   SetZoom(material, gameObject, texture);
               }
               break;
           case State.ZoomOut:
               if (currentZoomIndex > 0)
               {
                   currentZoomIndex--;
                   SetZoom(material, gameObject, texture);
               }
               break;
           case State.Reset:
               SetZoomIndex(material, 0, gameObject, texture);
               break;
        }
        
        zoomText?.Invoke(GetFormattedZoomPct());
    }

    private static void SetZoom(Material material, GameObject gameObject, Texture2D texture)
    {
        SetZoomIndex(material, currentZoomIndex, gameObject, texture);
    }

    private static void SetZoomIndex(Material material, int newZoomIndex, GameObject gameObject, Texture2D texture)
    {
        currentZoomIndex = newZoomIndex;
        SetZoom(material, zoomSteps[currentZoomIndex]);
        SetZoomOffset(material, gameObject, texture, zoomSteps[currentZoomIndex]);
    }

    private static void ResetZoom()
    {
        
    }

    private static string GetFormattedZoomPct()
    {
        return $"({zoomSteps[currentZoomIndex] * 100}%)";
    }

    private static void SetZoom(Material material, float zoomValue)
    {
        var scale = 1 / zoomValue;
        SetVectorOnRenderList(material, "_ZoomTiling", new Vector2(scale, scale));
    }

    private static void SetZoomOffset(Material material, GameObject gameObject, Texture2D texture, float zoomValue)
    {
        SetVectorOnRenderList(material, "_ZoomOffset", ZoomValToOffset(gameObject, texture, zoomValue));
    }

    public static void SetWidth(List<Renderer> renderers, Texture texture, string tilingVector, string offsetVector)
    {
        foreach (var rend in renderers)
        {
            Vector2 meshSize = rend.gameObject.CalculateRendererBounds().size;
            var ratio = GetNormalisedTiling(texture, meshSize);
            var offset = GetRatioOffset(ratio);

            var mat = rend.material;

            mat.SetVector(tilingVector, ratio);
            mat.SetVector(offsetVector, offset);
        }
    }

    public static void SetWidth(Material material, GameObject gameObject, Texture texture, string tilingVector, string offsetVector)
    {
        Vector2 meshSize = gameObject.CalculateRendererBounds().size;
        var ratio = GetNormalisedTiling(texture, meshSize);
        var offset = GetRatioOffset(ratio);

        // material = renderer.material;

        material.SetVector(tilingVector, ratio);
        material.SetVector(offsetVector, offset);
    }

    private static void SetVectorOnRenderList(List<Renderer> renderers, string property, ref Vector2 value)
    {
        foreach (var rend in renderers)
        {
            rend.material.SetVector(property, value);
        }
    }

    private static Vector2 GetNormalisedTiling(Texture texture, Vector2 meshBounds)
    {
        if (texture != null)
        {
            var ratio = Vector2.zero;

            ratio.x = texture.width / meshBounds.x;
            ratio.y = texture.height / meshBounds.y;

            if (ratio.x < ratio.y)
            {
                ratio.y = ratio.y / ratio.x;
                ratio.x = ratio.x / ratio.x;
            }
            else
            {
                ratio.x = ratio.x / ratio.y;
                ratio.y = ratio.y / ratio.y;
            }

            (ratio.x, ratio.y) = (ratio.y, ratio.x);

            return ratio;
        }
        
        return Vector2.zero;
    }

    private static Vector2 GetRatioOffset(Vector2 ratio)
    {
        var offset = Vector2.zero;

        offset.x = (ratio.x - 1) / 2;
        offset.y = (ratio.y - 1) / 2;

        return -offset;
    }

    private static void SetVectorOnRenderList(Material material, string property, Vector2 value)
    {
        material.SetVector(property, value);
    }

    private static Vector2 ZoomValToOffset(GameObject gameObject, Texture2D curTexture, float zoomValue)
    {
        var ratio = Vector2.one;
        var meshBounds = gameObject.CalculateRendererBounds().size;
        if (curTexture != null)
        {
            ratio.x = curTexture.width / meshBounds.x;
            ratio.y = curTexture.height / meshBounds.y;
        }
        else
        {
            ratio.x = 1 / meshBounds.x;
            ratio.y = 1 / meshBounds.y;
        }

        Vector2 zoomOffset = Vector2.one;
        zoomOffset.x = (1 - (1 / zoomValue)) / 2;
        zoomOffset.y = (1 - (1 / zoomValue)) / 2;

        if (ratio.x > ratio.y)
        {
            zoomOffset.y *= ratio.x / ratio.y;
        }
        else
        {
            zoomOffset.x *= ratio.y / ratio.x;
        }

        return zoomOffset;
    }
    }   
}
