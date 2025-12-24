using Raylib_cs;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Codixia.UI;

public abstract class UIElement
{
    public static UIElement? FocusedElement => _focusedElement;
    private static UIElement? _focusedElement;

    public Vector2 Position;
    public Vector2 Size;           // computed or fixed
    public Vector2 MinSize = Vector2.Zero; // minimum size
    public bool Visible = true;
    public bool IgnoreLayoutWhenInvisible = false;
    public bool Enabled = true;
    public UIContainer? Parent;
    public bool AutoSize = false;
    public bool IsRoot = false;
    public bool IsFocused => FocusedElement == this;

    public LayoutPosition LayoutPosition = LayoutPosition.Relative;
    public Alignment Align = Alignment.Start;
    public Vector2 Margin = Vector2.Zero;
    public Vector2 Padding = Vector2.Zero;

    public MouseFilter MouseFilter = MouseFilter.Stop;

    public Vector2 GlobalPosition => Parent != null ? Parent.GlobalPosition + Position : Position;

    /// <summary>
    /// Handles input for the UI element when hovered. (Called automatically in root <see cref="UIContainer"/> update loop)
    /// </summary>
    public virtual void Input()
    {
        if (!Visible || !Enabled)
            return;

        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && IsMouseOver())
            Focus();
    }

    /// <summary>
    /// Called once per frame to update the UI element's state.
    /// </summary>
    public virtual void Update() 
    {
        if (!Visible || !Enabled)
            return;
    }

    /// <summary>
    /// Called once per frame to render the UI element.
    /// </summary>
    public virtual void Render() 
    {
        if (!Visible) return;

        if (IsRoot)
            ComputeLayout();
    }

    public virtual void ComputeLayout() 
    {
        Size.X = MathF.Max(Size.X, MinSize.X);
        Size.Y = MathF.Max(Size.Y, MinSize.Y);
    }

    /// <summary>
    /// Focus the specified UI element.
    /// </summary>
    /// <param name="element"></param>
    /// <returns><see langword="true" /> if element was not focused before</returns>
    public static bool Focus(UIElement element)
    {
        var res = _focusedElement != element;
        _focusedElement = element;
        return res;
    }

    /// <summary>
    /// Focus this UI element.
    /// </summary>
    /// <returns><inheritdoc cref="Focus(UIElement)"/></returns>
    public bool Focus() => Focus(this);

    /// <summary>
    /// Unfocus the specified UI element.
    /// </summary>
    /// <param name="element"></param>
    /// <returns><see langword="true" /> if element was focused before</returns>
    public static bool Unfocus(UIElement? element = null, bool onlyElement = false)
    {
        var res = element != null && _focusedElement == element;
        if (!onlyElement || res) _focusedElement = null;
        return res;
    }

    /// <summary>
    /// Unfocus this UI element.
    /// </summary>
    /// <returns><inheritdoc cref="Unfocus(UIElement)"/></returns>
    public bool Unfocus() => Unfocus(this, true);

    public bool IsMouseOver()
    {
        Vector2 mouse = Global.GetMousePosition();
        Vector2 pos = GlobalPosition;
        return mouse.X >= pos.X && mouse.X <= pos.X + Size.X &&
               mouse.Y >= pos.Y && mouse.Y <= pos.Y + Size.Y;
    }

    public virtual bool HandleMouseInput()
    {
        return false;
    }
}
