using AdventureTools.UI;
using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace AdventureTools.Items;

public sealed class CadastralCrawler : ModItem
{
    public static int ItemType;
    public override void OnCreated(ItemCreationContext context)
    {
        if (context is InitializationItemCreationContext)
            ItemType = Type;
    }
    public override void SetDefaults()
    {
        Item.width = Item.height = 48;
        Item.useTime = Item.useAnimation = 8;
        Item.UseSound = SoundID.Item132;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.holdStyle = ItemHoldStyleID.HoldRadio;
        Item.noUseGraphic = true;
    }
    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
            return null;
        var cadastral = player.GetModPlayer<CadastralPlayer>();
        if (cadastral.operatingCrawler ^= true)
        {
            IngameFancyUI.OpenUIState(CadastralUIState.Instance);
            cadastral.crawlerVisualPosition = cadastral.crawlerPosition = player.Center;
        }
        return true;
    }
}

public sealed class CadastralPlayer : ModPlayer
{
    public static Asset<Texture2D> ProbeTexture { get; } = ModContent.Request<Texture2D>("AdventureTools/Items/CrawlerProbe");
    public static Asset<Texture2D> HeldItemTexture { get; } = ModContent.Request<Texture2D>("AdventureTools/Items/CadastralCrawler_Held");
    private static Asset<Texture2D> JimsDroneHeld = TextureAssets.Extra[ExtrasID.JimsDroneRadio];
    public bool operatingCrawler;
    public Vector2 crawlerPosition;
    public Vector2 crawlerVisualPosition;
    public static Vector2 realMouse;
    public static UPoint16 realPosition;
    static CadastralPlayer()
    {
        On_PlayerDrawLayers.DrawPlayer_JimsDroneRadio += static (orig, ref drawInfo) =>
        {
            var prevType = 0;
            if (drawInfo.drawPlayer.HeldItem is { ModItem: CadastralCrawler })
            {
                ref var hType = ref drawInfo.drawPlayer.HeldItem.type;
                prevType = hType;
                hType = ItemID.JimsDrone;
                TextureAssets.Extra[ExtrasID.JimsDroneRadio] = HeldItemTexture;
            }
            orig(ref drawInfo);
            if (prevType != 0)
            {
                drawInfo.drawPlayer.HeldItem.type = prevType;
                TextureAssets.Extra[ExtrasID.JimsDroneRadio] = JimsDroneHeld;
            }
        };
    }

    public override void SetControls()
    {
        if (!operatingCrawler)
            return;
        if (Player.controlInv && Player.releaseInventory)
        {
            CadastralUIState.GoBack();
            Player.releaseInventory = false;
        }
    }
    public override void PreUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;
        realMouse = Main.MouseWorld;
        realPosition = GeometryUtils.ToUPoint16(realMouse);
    }
    public override void PostUpdateEquips()
    {
        if (!operatingCrawler)
            return;
        var p = Player;
        p.isOperatingAnotherEntity = true;
        var i = p.LocalInputCache;
        float moveSpeed = p.controlTorch ? 12f : 24f;
        var moveX = 0f;
        var moveY = 0f;
        if (i.controlUp)
            moveY--;
        if (i.controlRight)
            moveX++;
        if (i.controlDown)
            moveY++;
        if (i.controlLeft)
            moveX--;
        crawlerPosition += new Vector2(moveX, moveY).SafeNormalize(Vector2.Zero) * moveSpeed;
        if (Main.netMode == NetmodeID.Server)
            RemoteClient.CheckSection(Player.whoAmI, crawlerPosition);
    }
    public override bool CanUseItem(Item item) => !operatingCrawler;
    public override void ModifyScreenPosition()
    {
        if (!operatingCrawler)
            return;
        Main.screenPosition = crawlerPosition - new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
    }
    internal void RenderProbe(SpriteBatch sb)
    {
        if (!operatingCrawler)
            return;
        crawlerVisualPosition = Vector2.Lerp(crawlerVisualPosition, crawlerPosition, 0.2f);
        var rotation = (crawlerPosition.X - crawlerVisualPosition.X) * 0.01f;
        sb.Draw(ProbeTexture.Value, crawlerVisualPosition - Main.screenPosition, null, Color.White, rotation, ProbeTexture.Size() * 0.5f, 2f, 0, 0);
    }
}
