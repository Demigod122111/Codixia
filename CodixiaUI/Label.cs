using Raylib_cs;
using System.Numerics;

namespace Codixia.UI;

public class Label : UIElement
{
    public Font Font;
    public string Text = "";
    public float TextSpacing = 1.0f;
    public int FontSize = 20;
    public Color Color = Color.White;

    public Label()
    {
        Font = Raylib.GetFontDefault();
        MouseFilter = MouseFilter.Ignore;
        AutoSize = true;
    }

    public override void Render()
    {
        base.Render();

        if (!Visible) return;
        var textSize = Raylib.MeasureTextEx(Font, Text, FontSize, TextSpacing);
        Raylib.DrawTextEx(Font, Text,
            new Vector2((GlobalPosition.X + (Size.X - textSize.X) * 0.5f),
            (GlobalPosition.Y + (Size.Y - textSize.Y) * 0.5f)),
            FontSize, TextSpacing, Color);
    }

    public override void ComputeLayout()
    {
        Text ??= "";

        if (AutoSize)
        {
            var textSize = Raylib.MeasureTextEx(Font, Text, FontSize, TextSpacing);
            Size = new Vector2(textSize.X + Padding.X * 2, textSize.Y + Padding.Y * 2);
        }

        base.ComputeLayout();
    }
}