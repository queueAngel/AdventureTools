using AdventureTools.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Numerics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Vec = Microsoft.Xna.Framework.Vector2;

namespace AdventureTools.Core;

public sealed class Cursors : ILoadable
{
    private const int CustomCursorCount = 4;
    public static short DragDiagonal { get; private set; } = -1;
    public static short DragDiagonalOutline { get; private set; } = -1;
    public static short DragOrthogonal { get; private set; } = -1;
    public static short DragOrthogonalOutline { get; private set; } = -1;
    private static int CursorOverride(int cursor, int thick)
    {
        if (DynamicPanel.dir != 0)
            return (BitOperations.IsPow2((int)DynamicPanel.dir) ? DragOrthogonal : DragDiagonal) + thick;
        return cursor;
    }
    private static Vec OriginOverride(Vec origin, int cursor)
    {
        if (cursor >= DragDiagonal && cursor <= DragOrthogonalOutline)
            return TextureAssets.Cursors[cursor].Size() * 0.5f;
        return origin;
    }
    private static float RotationOverride(float rotation)
    {
        if (DynamicPanel.dir is ResizeDirection.Up or ResizeDirection.Down or ResizeDirection.UpRight or ResizeDirection.DownLeft)
            return MathHelper.PiOver2;
        return rotation;
    }
    static Cursors()
    {
        IL_Main.DrawThickCursor += static il =>
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After, i => i.MatchAdd());
            c.EmitLdcI4(1);
            c.EmitDelegate(CursorOverride);
            var chosenCursor = 0;
            c.GotoNext(i => i.MatchStloc(out chosenCursor));
            var origin = 0;
            c.GotoNext(MoveType.After,
                i => i.MatchLdloca(out origin),
                i => i.MatchLdcR4(2f),
                i => i.MatchCall(typeof(Vec).GetConstructor([typeof(float)])));
            c.EmitLdloc(origin);
            c.EmitLdloc(chosenCursor);
            c.EmitDelegate(OriginOverride);
            c.EmitStloc(origin);
            c.GotoNext(MoveType.After, i => i.MatchLdloc(out _), i => i.MatchLdcR4(0f));
            c.EmitDelegate(RotationOverride);
        };
        IL_Main.DrawCursor += static il =>
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After, i => i.MatchCall(typeof(Utils).GetMethod(nameof(Utils.ToInt))));
            c.EmitLdcI4(0);
            c.EmitDelegate(CursorOverride);
            var chosenCursor = 0;
            c.GotoNext(i => i.MatchStloc(out chosenCursor));
            for (int i = 0; i < 2; i++)
            {
                c.GotoNext(MoveType.After, i => i.MatchLdcR4(0f));
                c.EmitDelegate(RotationOverride);
                c.GotoNext(i => i.MatchInitobj<Vec>());
                c.GotoNext(MoveType.After, i => i.MatchLdloc(out _));
                c.EmitLdloc(chosenCursor);
                c.EmitDelegate(OriginOverride);
                c.GotoNext(MoveType.After, i => i.MatchLdcR4(0f));
            }
        };
    }

    public void Load(Mod mod)
    {
        var originalLength = TextureAssets.Cursors.Length;
        var newLength = originalLength + CustomCursorCount;

        Array.Resize(ref TextureAssets.Cursors, newLength);

        // Load custom cursor textures
        TextureAssets.Cursors[originalLength] =
            mod.Assets.Request<Texture2D>("Cursors/DragDiagonal");
        TextureAssets.Cursors[originalLength + 1] =
            mod.Assets.Request<Texture2D>("Cursors/DragDiagonalOutline");
        TextureAssets.Cursors[originalLength + 2] =
            mod.Assets.Request<Texture2D>("Cursors/DragOrthogonal");
        TextureAssets.Cursors[originalLength + 3] =
            mod.Assets.Request<Texture2D>("Cursors/DragOrthogonalOutline");

        // Assign cursor IDs
        DragDiagonal = (short)originalLength;
        DragDiagonalOutline = (short)(originalLength + 1);
        DragOrthogonal = (short)(originalLength + 2);
        DragOrthogonalOutline = (short)(originalLength + 3);
    }

    public void Unload()
    {
        var originalLength = TextureAssets.Cursors.Length - CustomCursorCount;
        Array.Resize(ref TextureAssets.Cursors, originalLength);
    }
}
