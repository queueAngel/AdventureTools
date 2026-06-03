using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class EquipPicker : UIElement
{
    public Player Dummy;
    public EquipType Type;
    public StyleDimension ButtonHeight;
    public StyleDimension MenuHeight;
    private float _anim;
    public bool Open;
    private bool _lastOpen;
    public bool All;
    public override void OnInitialize()
    {
        
    }
    public override void Update(GameTime gameTime)
    {
        var oldAnim = _anim;
        _anim = float.Lerp(_anim, Open ? 1f : 0f, 0.5f);
        if (_anim != oldAnim)
        {
            Height = new(ButtonHeight.Pixels + (MenuHeight.Pixels * _anim), ButtonHeight.Percent + (MenuHeight.Percent * _anim));
            Recalculate();
        }
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        Open ^= true;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var prevHeight = Height;
        Height = ButtonHeight;
        Recalculate();
        this.DrawConfigPanel(spriteBatch, out var preDims);
        Height = prevHeight;
        Recalculate();
        if (_anim <= float.Epsilon)
            return;
        var x = 0;
        var y = 0;
        var postDims = this.Dimensions;
        var gridSpace = new Rectangle((int)preDims.X, (int)(preDims.Y + preDims.Height), (int)preDims.Width, (int)(postDims.Height - preDims.Height));
        Terraria.ModLoader.Config.UI.ConfigElement.DrawPanel2(spriteBatch, gridSpace.TopLeft(), TextureAssets.SettingsPanel.Value, postDims.Width, gridSpace.Height, UICommon.DefaultUIBlue);
        spriteBatch.FlushBatch();
        var width = 42f;
        var fit = (int)(gridSpace.Width / width);

        if (All)
        {
            ref var equip = ref WorldNPC.GetPlayerEquip(Dummy, Type);
            var old = equip;
            foreach (var slot in EquipLoader.GetSearch(Type)._idToName.Keys)
            {
                equip = slot;
                var pos = gridSpace.TopLeft() + new Vector2(x * width, y * 64f);
                if (pos.X > gridSpace.Right || pos.Y + 48 > gridSpace.Bottom)
                    continue;
                var playerPosition = pos + new Vector2(32f - Dummy.width * 0.5f, 32f - Dummy.height * 0.5f);
                Main.PlayerRenderer.DrawPlayer(Main.Camera, Dummy, playerPosition + Main.screenPosition, 0f, Vector2.Zero, 0f, 1f);
                if (++x == fit)
                {
                    x = 0;
                    y++;
                }
            }
            equip = old;
        }
        else
        {
            foreach (var item in EquipLoader.slotToId[Type].Values)
            {

            }
        }
    }
}
