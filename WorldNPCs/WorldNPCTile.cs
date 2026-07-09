using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AdventureTools.WorldNPCs;

public sealed class WorldNPCTile : ModTile
{
    public override string Texture => "Terraria/Images/NPC_" + NPCID.None;
    public static ushort TileType;
    public override void SetStaticDefaults()
    {
        TileType = Type;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        TileID.Sets.DrawTileInSolidLayer[Type] = true;
    }
    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
    public override bool CanExplode(int i, int j) => false;
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomSolid);
        return false;
    }
    public static readonly List<Point16> TEsToDrawWithOutline = [];
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var pos = new Point16(i, j);
        var entity = (WorldNPCTileEntity)TileEntity.ByPosition[pos];
        if (entity.DrawWithOutline)
        {
            TEsToDrawWithOutline.Add(pos);
            entity.DrawWithOutline = false;
        }
        else
            entity.Draw(i, j);
    }
    public override void MouseOverFar(int i, int j)
    {
        if (Dummy is null)
        {
            Dummy = new();
            Dummy.SetDefaults(ModContent.NPCType<WorldNPC>());
            Dummy.hide = true;
            Dummy.noTileCollide = true;
            Prev = Main.npc[Main.maxNPCs - 1];
        }
        var te = (WorldNPCTileEntity)TileEntity.ByPosition[new Point16(i, j)];
        var world = (WorldNPC)Dummy.ModNPC;
        world.Static = true;
        Dummy.direction = Dummy.spriteDirection = te.Direction;
        Dummy.position = new Vector2(i * 16f, j * 16f - 25f);
        world.Schema = te.Schema;
        Main.npc[Main.maxNPCs - 1] = Dummy;
    }
    private static NPC Dummy;
    private static NPC Prev;
}
public sealed class WorldNPCTileEntity : ModTileEntity
{
    public bool DrawWithOutline;
    public JsonObject Schema;
    public uint Packed;
    public bool Sitting => (Packed & 0b1) != 0;
    public int Direction => ((Packed & 0b10) != 0).ToDirectionInt();
    public int BodyFrame => (int)((Packed >> 2) & 0b11111);
    public int LegFrame => (int)((Packed >> 7) & 0b11111);

    public override bool IsTileValidForEntity(int x, int y) => Main.tile[x, y].TileType == WorldNPCTile.TileType;
    public override void SaveData(TagCompound tag)
    {
        tag["data"] = Packed;
        if (Schema is null)
            return;
        tag["schema"] = Schema.GetElementIndex();
    }
    public override void LoadData(TagCompound tag)
    {
        Packed = tag.Get<uint>("data");
        if (!tag.TryGet<int>("schema", out var schema))
            return;
        Schema = BiomeSystem.biomesNode["NPCs"][schema].AsObject();
    }
    public void Draw(int i, int j)
    {
        WorldNPC.PrintOnDummy(Schema);
        var p = WorldNPC.Dummy;
        p.bodyFrame.Y = p.bodyFrame.Height * BodyFrame;
        p.legFrame.Y = p.legFrame.Height * LegFrame;
        p.direction = Direction;
        p.sitting.isSitting = Sitting;
        p.Bottom = new Vector2(i * 16 + p.width * 0.5f, (j + 1) * 16);
        Main.PlayerRenderer.DrawPlayer(Main.Camera, p, p.position, 0f, Vector2.Zero);
    }
}