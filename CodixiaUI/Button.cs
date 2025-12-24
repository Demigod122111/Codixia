using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

namespace Codixia.UI;

public class Button : UIElement
{
    public Font Font;
    public string Text = "";
    public float TextSpacing = 1.0f;
    public int FontSize = 20;
    public Color NormalColor = Color.Gray;
    public Color HoverColor = Color.LightGray;
    public Color PressedColor = Color.Black;
    public Color DisabledColor = Color.DarkGray;
    public Color TextColor = Color.White;
    public Action? OnClick;


    private bool _isPressed;
    private Stopwatch? _pressedTick;
    private bool _isHovered;

    public Button()
    {
        Padding = new(10, 5);
        Font = Raylib.GetFontDefault();
    }

    public override bool HandleMouseInput()
    {
        return true;
    }

    public override void Input()
    {
        base.Input();
        
        _isHovered = IsMouseOver();

        if (!Visible || !Enabled) return;

        if (IsMouseOver() && Raylib.IsMouseButtonPressed(MouseButton.Left) && !_isPressed)
        {
            _isPressed = true;
            OnClick?.Invoke();

            _pressedTick = new();
            _pressedTick.Start();
        }

        if (_pressedTick != null && _pressedTick.ElapsedMilliseconds >= 100 && !(IsMouseOver() && Raylib.IsMouseButtonDown(MouseButton.Left)))
        {
            _isPressed = false;
            _pressedTick.Stop();
            _pressedTick = null;
        }
    }

    public override void Render()
    {
        base.Render();
        if (!Visible) return;

        Color color = !Enabled ? DisabledColor : (_isPressed ? PressedColor : (_isHovered ? HoverColor : NormalColor));
        var textSize = Raylib.MeasureTextEx(Font, Text, FontSize, TextSpacing);
        Raylib.DrawRectangle((int)GlobalPosition.X, (int)GlobalPosition.Y, (int)Size.X, (int)Size.Y, color);
        Raylib.DrawTextEx(Font, Text,
            new Vector2((GlobalPosition.X + (Size.X - textSize.X) * 0.5f), 
            (GlobalPosition.Y + (Size.Y - textSize.Y) * 0.5f)),
            FontSize, TextSpacing, TextColor);

        _isHovered = false;
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
