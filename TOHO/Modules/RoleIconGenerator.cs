using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TOHO.Roles.Core;
using TOHO.Roles.Modifiers;
using UnityEngine;

namespace TOHO.Modules;

public enum RoleIconShape
{
    Circle,
    Diamond,
    Hexagon,
    Octagon,
    Shield,
}

public readonly struct RoleIconStyle(RoleIconShape shape, string glyphId = null)
{
    public readonly RoleIconShape Shape = shape;
    public readonly string GlyphId = glyphId;
}

public static class RoleIconGenerator
{
    private const int Size = 64;

    private const float EdgeInset = 2f;
    private const float EdgeSoftness = 1.5f;
    private const float OutlineWidth = 4f;

    private const float IconPixelsPerUnit = 16f;

    private static readonly Dictionary<Custom_RoleType, RoleIconStyle> CategoryStyles = new()
    {
        // Impostors
        [Custom_RoleType.ImpostorVanilla] = new(RoleIconShape.Diamond, "gem"),
        [Custom_RoleType.ImpostorKilling] = new(RoleIconShape.Diamond, "sword"),
        [Custom_RoleType.ImpostorSupport] = new(RoleIconShape.Diamond, "gear"),
        [Custom_RoleType.ImpostorConcealing] = new(RoleIconShape.Diamond, "mask"),
        [Custom_RoleType.ImpostorHindering] = new(RoleIconShape.Diamond, "chain"),
        [Custom_RoleType.ImpostorGhosts] = new(RoleIconShape.Diamond, "ghost"),
        [Custom_RoleType.Madmate] = new(RoleIconShape.Diamond, "brain"),

        // Crewmate
        [Custom_RoleType.CrewmateVanilla] = new(RoleIconShape.Circle, "gem"),
        [Custom_RoleType.CrewmateVanillaGhosts] = new(RoleIconShape.Circle, "ghost"),
        [Custom_RoleType.CrewmateHindering] = new(RoleIconShape.Hexagon, "chain"),
        [Custom_RoleType.CrewmateInvestigative] = new(RoleIconShape.Circle, "magnifying_glass"),
        [Custom_RoleType.CrewmateSupport] = new(RoleIconShape.Hexagon, "medical"),
        [Custom_RoleType.CrewmateKilling] = new(RoleIconShape.Shield, "sword"),
        [Custom_RoleType.CrewmatePower] = new(RoleIconShape.Shield, "gear"),
        [Custom_RoleType.CrewmateGhosts] = new(RoleIconShape.Circle, "ghost"),

        // Neutral
        [Custom_RoleType.NeutralBenign] = new(RoleIconShape.Hexagon, "compass"),
        [Custom_RoleType.NeutralEvil] = new(RoleIconShape.Octagon, "mask"),
        [Custom_RoleType.NeutralChaos] = new(RoleIconShape.Octagon, "flame"),
        [Custom_RoleType.NeutralKilling] = new(RoleIconShape.Octagon, "skull"),
        [Custom_RoleType.NeutralApocalypse] = new(RoleIconShape.Octagon, "bomb"),
        [Custom_RoleType.NeutralGhosts] = new(RoleIconShape.Circle, "ghost"),

        // Coven
        [Custom_RoleType.CovenPower] = new(RoleIconShape.Octagon, "crown"),
        [Custom_RoleType.CovenKilling] = new(RoleIconShape.Octagon, "poison"),
        [Custom_RoleType.CovenTrickery] = new(RoleIconShape.Octagon, "spider_web"),
        [Custom_RoleType.CovenUtility] = new(RoleIconShape.Octagon, "cauldron"),

        [Custom_RoleType.None] = new(RoleIconShape.Circle),
    };

    private static readonly Dictionary<ModifierTypes, RoleIconStyle> ModifierCategoryStyles = new()
    {
        [ModifierTypes.Impostor] = new(RoleIconShape.Diamond, "circuit"),
        [ModifierTypes.Helpful] = new(RoleIconShape.Hexagon, "gear"),
        [ModifierTypes.Harmful] = new(RoleIconShape.Octagon, "flame"),
        [ModifierTypes.Misc] = new(RoleIconShape.Circle, "key"),
        [ModifierTypes.Guesser] = new(RoleIconShape.Circle, "crosshair"),
        [ModifierTypes.Mixed] = new(RoleIconShape.Hexagon, "gem"),
        [ModifierTypes.Experimental] = new(RoleIconShape.Octagon, "microscope"),
    };

    private static readonly RoleIconStyle FallbackStyle = new(RoleIconShape.Circle);

    private static readonly Dictionary<CustomRoles, Sprite> Cache = [];
    private static readonly Dictionary<string, Color32[]> GlyphPixelCache = [];

    public static Sprite GetIcon(CustomRoles role)
    {
        if (Cache.TryGetValue(role, out var cached) && cached != null)
            return cached;

        var color = Utils.GetRoleColor(role);
        var style = ResolveStyle(role);
        var sprite = BuildIconSprite(style.Shape, color, style.GlyphId);

        return Cache[role] = sprite;
    }

    private static RoleIconStyle ResolveStyle(CustomRoles role)
    {
        if (CategoryStyles.TryGetValue(role.GetCustomRoleType(), out var curated))
            return curated;

        if (CustomRoleManager.RoleClass.TryGetValue(role, out var roleClass) && roleClass != null)
            return CategoryStyles.TryGetValue(roleClass.ThisRoleType, out var roleStyle)
                ? roleStyle
                : FallbackStyle;

        if (CustomRoleManager.ModifierClasses.TryGetValue(role, out var modifierClass) && modifierClass != null)
            return ModifierCategoryStyles.TryGetValue(modifierClass.Type, out var modStyle)
                ? modStyle
                : FallbackStyle;

        return FallbackStyle;
    }

    public static void ClearCache() => Cache.Clear();

    private static Sprite BuildIconSprite(RoleIconShape shape, Color color, string glyphId)
    {
        var texture = new Texture2D(Size, Size, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var cx = Size / 2f;
        var cy = Size / 2f;
        var radius = Size / 2f - EdgeInset;
        var pixels = new Il2CppStructArray<Color32>(Size * Size);

        for (var y = 0; y < Size; y++)
        {
            var dy = (Size - 1 - y) + 0.5f - cy;
            for (var x = 0; x < Size; x++)
            {
                var dx = x + 0.5f - cx;

                var dist = ShapeDistance(shape, dx, dy, radius);

                var alpha = 1f - Mathf.Clamp01((dist + EdgeSoftness) / EdgeSoftness);

                // White outline: blend toward white as dist approaches 0
                // (the true edge) from the inside, fading back to the plain
                // fill color once OutlineWidth deep inside the shape. Uses
                // the same distance field as the fill/anti-aliasing above,
                // so the outline and the fill always agree on where the
                // edge actually is.
                var outlineFactor = 1f - Mathf.Clamp01(-dist / OutlineWidth);
                var pixel = Color.Lerp(color, Color.white, outlineFactor);
                pixel.a = color.a * alpha;
                pixels[y * Size + x] = pixel;
            }
        }

        if (glyphId != null)
            CompositeGlyph(pixels, glyphId);

        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, Size, Size),
            new Vector2(0.5f, 0.5f),
            IconPixelsPerUnit);
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;

        return sprite;
    }

    private static float ShapeDistance(RoleIconShape shape, float dx, float dy, float radius) => shape switch
    {
        RoleIconShape.Circle => (float)Math.Sqrt(dx * dx + dy * dy) - radius,

        RoleIconShape.Diamond => Math.Abs(dx) + Math.Abs(dy) - radius,

        RoleIconShape.Hexagon => HexagonDistance(dx, dy, radius),

        RoleIconShape.Octagon => Math.Max(
            Math.Max(Math.Abs(dx), Math.Abs(dy)),
            (Math.Abs(dx) + Math.Abs(dy)) * 0.70710678f) - radius,

        RoleIconShape.Shield => ShieldDistance(dx, dy, radius),

        _ => (float)Math.Sqrt(dx * dx + dy * dy) - radius,
    };

    private static float HexagonDistance(float dx, float dy, float radius)
    {
        var qx = Math.Abs(dx);
        var qy = Math.Abs(dy);
        var edge1 = qy - radius;
        var edge2 = qx * 0.8660254f + qy * 0.5f - radius;
        return Math.Max(edge1, edge2);
    }

    private static float ShieldDistance(float dx, float dy, float radius)
    {
        var qx = Math.Abs(dx);

        if (dy <= 0)
        {
            var topY = Math.Max(dy, -radius * 0.75f);
            return (float)Math.Sqrt(qx * qx + topY * topY) - radius;
        }

        var bottomTip = radius * 1.15f;
        var t = Mathf.Clamp01(dy / bottomTip);
        var widthAtY = radius * (1f - t);
        return qx - widthAtY;
    }

    private static void CompositeGlyph(Il2CppStructArray<Color32> basePixels, string glyphId)
    {
        var glyphPixels = GetGlyphPixels(glyphId);
        if (glyphPixels == null)
            return;

        for (var i = 0; i < Size * Size; i++)
        {
            var glyph = glyphPixels[i];
            if (glyph.a == 0)
                continue;

            var baseC = basePixels[i];
            var glyphAlpha01 = glyph.a / 255f;

            var outA = glyphAlpha01 + (baseC.a / 255f) * (1f - glyphAlpha01);
            if (outA <= 0f)
                continue;

            byte Blend(byte glyphChannel, byte baseChannel) =>
                (byte)Mathf.Clamp(
                    (glyphChannel * glyphAlpha01 + baseChannel * (baseC.a / 255f) * (1f - glyphAlpha01)) / outA,
                    0f, 255f);

            basePixels[i] = new Color32(
                Blend(glyph.r, baseC.r),
                Blend(glyph.g, baseC.g),
                Blend(glyph.b, baseC.b),
                (byte)Mathf.Clamp(outA * 255f, 0f, 255f));
        }
    }

    private static Color32[] GetGlyphPixels(string glyphId)
    {
        if (GlyphPixelCache.TryGetValue(glyphId, out var cached))
            return cached;

        if (!RoleIconGlyphs.Base64PngById.TryGetValue(glyphId, out var base64))
        {
            Logger.Error($"Unknown role icon glyph id: {glyphId}", "RoleIconGenerator");
            return GlyphPixelCache[glyphId] = null;
        }

        var bytes = Convert.FromBase64String(base64);
        var glyphTexture = new Texture2D(Size, Size, TextureFormat.ARGB32, false);
        glyphTexture.LoadImage(bytes, false);

        var pixels = glyphTexture.GetPixels32();
        UnityEngine.Object.DestroyImmediate(glyphTexture);

        return GlyphPixelCache[glyphId] = pixels;
    }
}