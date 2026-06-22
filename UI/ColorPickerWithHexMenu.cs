using Daybreak.Common.UI;
using AdventureTools.Utilities;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;
using Terraria.UI.Chat;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Daybreak.Common.Rendering;

namespace AdventureTools.UI;

public sealed class ColorPickerWithHexMenu : UIElement
{
    public ColorPicker Picker;
    public TextButton CopyHex;
    public TextButton PasteHex;
    public InputField HexString;
    private Color? _last;
    public ColorPickerWithHexMenu()
    {
        var d = 26f;
        var dS = new StyleDimension(d, 0f);
        Picker = new() { Height = StyleDimension.Fill - (d + 4f), Width = StyleDimension.Fill};
        CopyHex = new()
        {
            Height = dS,
            Width = dS,
            VAlign = 1f,
            Image = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Copy"),
            Action = () => Platform.Get<IClipboard>().Value = Picker.Color.Hex3(),
        };
        PasteHex = new()
        {
            Height = dS,
            Width = dS,
            Left = dS,
            VAlign = 1f,
            Image = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Paste"),
            Action = () =>
            {
                var hex = SchemaUtils.Hex(Platform.Get<IClipboard>().Value);
                if (hex.HasValue)
                    Picker.Color = hex.Value;
            },
        };
        HexString = new(string.Empty) { Height = dS, Width = StyleDimension.Fill - d * 2f, MaxChars = 6, VAlign = 1f, HAlign = 1f, TextAlignX = 1f, TextScale = 0.8f };
        var hex = "0123456789ABCDEFabcdef";
        for (int i = 0; i < hex.Length; i++)
            HexString.WhitelistedChars.Add(hex[i]);
        HexString.OnEnter += s =>
        {
            s.Text = s.Text.ToLowerInvariant();
            Picker.Color = SchemaUtils.Hex(s.Text).Value;
        };

        Append(Picker);
        Append(CopyHex);
        Append(PasteHex);
        Append(HexString);
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        // more reliable than Picker.OnChanged
        var ss = default(SpriteBatchSnapshot);
        if (IgnoresMouseInteraction)
        {
            spriteBatch.End(out ss);
            spriteBatch.Begin(ss with { SortMode = SpriteSortMode.Immediate, CustomEffect = DrawUtils.GrayScaleShader.Value });
        }

        if (!_last.HasValue || _last.Value != Picker.Color)
        {
            _last = Picker.Color;
            HexString.Text = Picker.Color.Hex3();
        }
        base.Draw(spriteBatch);
        var font = FontAssets.MouseText.Value;
        var text = "#";
        var size = ChatManager.GetStringSize(font, text, Vector2.One);
        var dims = HexString._dimensions;
        var yPos = dims.Y + dims.Height * 0.5f;
        var pos = new Vector2(dims.X + 4f, yPos - size.Y * 0.4f);
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, pos, Color.Gray, 0f, default, Vector2.One);

        if (IgnoresMouseInteraction)
            spriteBatch.Restart(in ss);
    }
}
