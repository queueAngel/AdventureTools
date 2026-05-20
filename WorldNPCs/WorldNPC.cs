using AdventureTools.Utilities;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AdventureTools.WorldNPCs;

public sealed class WorldNPC : ModNPC
{
    public static RenderTargetLease OutlinesTarget { get; private set; }
    public static readonly Player Dummy = new();
    public JsonObject Schema;
    public bool Static;
    public bool DrawWithOutline;
    public static bool AnyHasOutline;
    public override string Texture => "Terraria/Images/NPC_" + NPCID.Guide;
    public override void Load()
    {
        if (Main.dedServ)
            return;
        // Main.QueueMainThreadAction(() => OutlinesTarget = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice));
        On_Main.DrawCachedNPCs += static (orig, self, npcCache, behindTiles) =>
        {
            orig(self, npcCache, behindTiles);
            if (!ReferenceEquals(npcCache, Main.instance.DrawCacheNPCProjectiles))
                return;
            if (OutlinesTarget != null && AnyHasOutline)
            {
                var s = Main.spriteBatch;
                s.End(out var ss);
                var effect = DrawUtils.OutlineShader.Value;
                var p = effect.Parameters;
                p["uColor"].SetValue(Color.White.ToVector3());
                p["uImageSize0"].SetValue(OutlinesTarget.Target.Size());
                s.Begin(ss with { CustomEffect = effect });
                s.Draw(OutlinesTarget.Target, Vector2.Zero, Color.White);
                s.Restart(in ss);

                OutlinesTarget.Scope(clearColor: Color.Transparent).Dispose();
                AnyHasOutline = false;
            }
        };
    }

    public override void SetStaticDefaults()
    {
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        Main.npcFrameCount[Type] = 26;
    }
    public override void SetDefaults()
    {
        NPC.townNPC = true; // Sets NPC to be a Town NPC
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.dontTakeDamage = true;
        NPC.knockBackResist = 0.5f;

        AnimationType = NPCID.Guide;
    }
    public override void SaveData(TagCompound tag)
    {
        if (Schema is null)
            return;
        tag["schema"] = Schema.GetElementIndex();
    }
    public override void LoadData(TagCompound tag)
    {
        if (!tag.TryGet<int>("schema", out var schema))
            return;
        Schema = BiomeSystem.biomesNode["NPCs"][schema].AsObject();
    }
    public override void AddShops()
    {
        new NPCShop(Type).Register();
    }
    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        // for some reason this doesn't run on the instance of the NPC that the player is talking to. which is weird, I'll probably PR it later
        var schema = ((WorldNPC)Main.LocalPlayer.TalkNPC.ModNPC).Schema;
        var shopNode = schema["Shop"];
        var index = 0;
        var shopArr = shopNode.AsArray();
        for (int i = 0; i < shopArr.Count; i++)
        {
            var entry = shopArr[i];
            if (entry.GetValueKind() != JsonValueKind.String)
                throw new Exception("Malformed shop");
            var itemType = SItemID((string)entry);
            var nextEntry = i == shopArr.Count - 1 ? null : shopArr[i + 1];
            var hasPrice = nextEntry != null && nextEntry.GetValueKind() == JsonValueKind.Number;
            if (hasPrice)
                i++;
            while (items[index] != null)
                index++;
            var addItem = new Item(itemType);
            if (hasPrice)
                addItem.shopCustomPrice = (int)nextEntry;
            items[index] = addItem;
        }
    }
    public override string GetChat()
    {
        return base.GetChat();
    }
    public override void SetChatButtons(ref string button, ref string button2)
    {
        if (Schema.ContainsKey("Shop"))
            button = Lang.inter[28].Value;
    }
    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
            shopName = "Shop";
    }
    public override bool ModifyDeathMessage(ref NetworkText customText, ref Color color)
    {
        // thoughts: maybe add a chat tag that allows retrieval of a custom NPC's localized name for this
        // otherwise other players will see the name in the language of the person who killed the NPC (or the server's? idk)
        return false;
    }
    public override void ModifyTypeName(ref string typeName)
    {
        if (!(Schema?.TryGetPropertyValue("Name", out var namesNode) == true))
            return;
        if (!SchemaUtils.TryGetLocalizedText(namesNode, out var text))
            return;
        typeName = text;
    }
    public override bool PreAI()
    {
        var shouldMove = Schema != null && !Static;
        if (shouldMove)
            return true;
        NPC.velocity.X = 0f;
        return false;
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (OutlinesTarget is null)
            Main.QueueMainThreadAction(() => OutlinesTarget = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice));

        PrintFromNPC();
        Dummy.Center = NPC.Center;

        var scope = default(RenderTargetScope);
        if (DrawWithOutline)
            scope = OutlinesTarget.Scope(clearColor: Color.Transparent);
        Main.PlayerRenderer.DrawPlayer(Main.Camera, Dummy, Dummy.position, 0f, Vector2.Zero);
        if (DrawWithOutline)
        {
            scope.Dispose();
            DrawWithOutline = false;
        }

        return false;
    }
    public static void PrintOnDummy(JsonObject schema)
    {
        var p = Dummy;
        var a = schema?["Appearance"]?.AsObject();
        var w = Color.White;
        p.hairColor = SchemaUtils.Hex(a, "HairColor") ?? w;
        p.skinColor = SchemaUtils.Hex(a, "SkinColor") ?? w;
        p.eyeColor = SchemaUtils.Hex(a, "EyeColor") ?? w;
        p.shirtColor = SchemaUtils.Hex(a, "ShirtColor") ?? w;
        p.underShirtColor = SchemaUtils.Hex(a, "UndershirtColor") ?? w;
        p.pantsColor = SchemaUtils.Hex(a, "PantsColor") ?? w;
        p.shoeColor = SchemaUtils.Hex(a, "ShoeColor") ?? w;
        if (a?.TryGetPropertyValue("Hair", out var hairNode) == true)
        {
            if (hairNode.GetValueKind() == JsonValueKind.Number)
                p.hair = (int)hairNode;
            else
                p.hair = HairID.Search.GetId((string)hairNode);
        }
        else
            p.hair = 0;
        var clothingStyle = (string?)a?["Style"] ?? "Starter";
        var bodyType = (string?)a?["BodyType"] ?? "Male";
        p.skinVariant = PlayerVariantID.Search.GetId(bodyType + clothingStyle);
        if (a?.TryGetPropertyValue("HairDye", out var hairDyeNode) == true)
        {
            var possibleHairDye = ParseItem((string)hairDyeNode);
            if (possibleHairDye is { hairDye: > -1 })
                p.hairDye = possibleHairDye.hairDye;
            else
                p.hairDye = 0;
        }
        else
            p.hairDye = 0;
        for (var i = EquipType.Head; i <= EquipType.Beard; i++)
        {
            ref var equip = ref GetPlayerEquip(p, i);
            if (!(a?.TryGetPropertyValue(i.ToString(), out var equipmentNode) == true))
                equip = 0;
            else
            {
                var search = EquipLoader.GetSearch(i);
                equip = search.GetId((string)equipmentNode);
            }
            ref var equipDye = ref GetPlayerDye(p, i);
            if (!(a?.TryGetPropertyValue(i.ToString() + "Dye", out var equipmentDyeNode) == true))
            {
                equipDye = 0;
                continue;
            }
            equipDye = ParseItem((string)equipmentDyeNode).dye;
        }
    }
    private void PrintFromNPC()
    {
        var p = Dummy;
        p.direction = NPC.direction;
        var frame = NPC.frame.Y / NPC.frame.Height;
        var bodyY = frame switch
        {
            0 or 19 => 0,
            1 or 2 => 6,
            <= 15 => frame + 4,
            _ => 0,
        };
        p.bodyFrame.Y = p.bodyFrame.Height * bodyY;
        var legsY = frame switch
        {
            0 => 0,
            <= 15 => frame + 4,
            _ => 0,
        };
        p.legFrame.Y = p.legFrame.Height * legsY;
        p.sitting.isSitting = frame == 18;

        PrintOnDummy(Schema);
    }
    /// FROM EQUIPLOADER CHECK AFTER 1.4.5
    private static ref int GetPlayerEquip(Player player, EquipType type)
    {
        switch (type)
        {
            case EquipType.Head: return ref player.head;
            case EquipType.Body: return ref player.body;
            case EquipType.Legs: return ref player.legs;
            case EquipType.HandsOn: return ref player.handon;
            case EquipType.HandsOff: return ref player.handoff;
            case EquipType.Back: return ref player.back;
            case EquipType.Front: return ref player.front;
            case EquipType.Shoes: return ref player.shoe;
            case EquipType.Waist: return ref player.waist;
            case EquipType.Wings: return ref player.wings;
            case EquipType.Shield: return ref player.shield;
            case EquipType.Neck: return ref player.neck;
            case EquipType.Face: return ref player.face;
            case EquipType.Beard: return ref player.beard;
            case EquipType.Balloon: return ref player.balloon;
        }
        throw null;
    }
    private static ref int GetPlayerDye(Player player, EquipType type)
    {
        switch (type)
        {
            case EquipType.Head: return ref player.cHead;
            case EquipType.Body: return ref player.cBody;
            case EquipType.Legs: return ref player.cLegs;
            case EquipType.HandsOn: return ref player.cHandOn;
            case EquipType.HandsOff: return ref player.cHandOff;
            case EquipType.Back: return ref player.cBack;
            case EquipType.Front: return ref player.cFront;
            case EquipType.Shoes: return ref player.cShoe;
            case EquipType.Waist: return ref player.cWaist;
            case EquipType.Wings: return ref player.cWings;
            case EquipType.Shield: return ref player.cShield;
            case EquipType.Neck: return ref player.cNeck;
            case EquipType.Face: return ref player.cFace;
            case EquipType.Beard: return ref player.cBeard;
            case EquipType.Balloon: return ref player.cBalloon;
        }
        throw null;
    }
    private static int SItemID(string name) => ItemID.Search.TryGetId(name, out var id) ? id : ItemID.None;
    private static Item ParseItem(string name)
    {
        var id = SItemID(name);
        return id == 0 ? null : ContentSamples.ItemsByType[id];
    }
}
