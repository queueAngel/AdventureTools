using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class EquipPicker : UIElement
{
    public Player Dummy;
    public EquipType Type;
    public StyleDimension ButtonHeight;
    public bool Open;
    public bool All;
    public override void OnInitialize()
    {
        
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var prevHeight = Height;
        Height = ButtonHeight;
        Recalculate();
        this.DrawConfigPanel(spriteBatch, out var preDims);
        Height = prevHeight;
        Recalculate();
        var x = 0;
        var y = 0;
        var postDims = this.Dimensions;
        var gridSpace = new Rectangle((int)preDims.X, (int)(preDims.Y + preDims.Height), (int)preDims.Width, (int)(postDims.Height - preDims.Height));
        if (All)
        {
            foreach (var slot in EquipLoader.GetSearch(Type)._idToName.Keys)
            {

            }
        }
        else
        {
            foreach (var item in EquipLoader.slotToId[Type].Values)
            {

            }
        }
    }
}
