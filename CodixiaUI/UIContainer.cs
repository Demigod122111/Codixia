using System.Numerics;

namespace Codixia.UI;

public class UIContainer : UIElement
{
    internal List<UIElement> Children = new List<UIElement>();
    public LayoutDirection Direction = LayoutDirection.Vertical;
    public float Spacing = 5f;

    // Grid-specific
    public int GridColumns = 2; // default columns for grid
    public Vector2 CellSize = Vector2.Zero; // optional fixed cell size

    public UIContainer()
    {
        MouseFilter = MouseFilter.Ignore;
    }
    public void AddChild(UIElement child)
    {
        if (child == this)
            throw new ArgumentException("UIElement cannot be a child of itself.");

        if (child is UIContainer c && c.HasDescendant(this))
            throw new ArgumentException("UIElement cannot be a child of its descendant.");

        child.Parent = this;
        Children.Add(child);
    }

    public bool HasChild(UIElement child)
    {
        return Children.Contains(child);
    }

    public bool HasDescendant(UIElement child)
    {
        if (Children.Contains(child)) return true;

        foreach (var c in Children.ToArray())
        {
            if (c is UIContainer container)
            {
                if (container.HasDescendant(child))
                    return true;
            }
        }

        return false;
    }

    public void RemoveChild(UIElement child)
    {
        if (Children.Remove(child))
            child.Parent = null;
    }

    public void RemoveAllChildren()
    {
        foreach (var child in Children.ToArray())
            RemoveChild(child);
    }

    public UIElement GetChild(int index)
    {
        if (index < 0 || index >= Children.Count)
            throw new IndexOutOfRangeException("Child index out of range.");

        return Children[index];
    }

    public T GetChild<T>(int index) where T: UIElement
    {
        if (index < 0 || index >= Children.Count)
            throw new IndexOutOfRangeException("Child index out of range.");

        return (T)Children[index];
    }

    public override void ComputeLayout()
    {
        if (Direction == LayoutDirection.Grid)
        {
            Vector2 offset = Padding;
            int col = 0;
            int row = 0;

            // Determine cell width/height
            Vector2 computedCellSize = CellSize;
            if (CellSize == Vector2.Zero)
            {
                // auto-compute max size among children
                float maxWidth = 0, maxHeight = 0;
                foreach (var child in Children)
                {
                    child.ComputeLayout();
                    maxWidth = Math.Max(maxWidth, child.Size.X + child.Margin.X * 2);
                    maxHeight = Math.Max(maxHeight, child.Size.Y + child.Margin.Y * 2);
                }
                computedCellSize = new Vector2(maxWidth, maxHeight);
            }

            foreach (var child in Children)
            {
                child.ComputeLayout();

                if ((!child.Visible && child.IgnoreLayoutWhenInvisible) || child.LayoutPosition == LayoutPosition.Absolute)
                    continue; // skip ignored invisible and absolute positioned elements

                // Compute position in grid
                float x = Padding.X + col * (computedCellSize.X + Spacing) + child.Margin.X;
                float y = Padding.Y + row * (computedCellSize.Y + Spacing) + child.Margin.Y;

                // Alignment inside cell
                switch (child.Align)
                {
                    case Alignment.Center:
                        x += (computedCellSize.X - child.Size.X) / 2;
                        y += (computedCellSize.Y - child.Size.Y) / 2;
                        break;
                    case Alignment.End:
                        x += (computedCellSize.X - child.Size.X);
                        y += (computedCellSize.Y - child.Size.Y);
                        break;
                    case Alignment.Stretch:
                        child.Size = new Vector2(computedCellSize.X - child.Margin.X * 2,
                                                 computedCellSize.Y - child.Margin.Y * 2);
                        break;
                }

                child.Position = new Vector2(x, y);

                col++;
                if (col >= GridColumns)
                {
                    col = 0;
                    row++;
                }
            }

            // Compute container size automatically if AutoSize
            if (AutoSize)
            {
                int totalRows = (Children.Count + GridColumns - 1) / GridColumns;
                float width = GridColumns * computedCellSize.X + (GridColumns - 1) * Spacing + Padding.X * 2;
                float height = totalRows * computedCellSize.Y + (totalRows - 1) * Spacing + Padding.Y * 2;
                Size = new Vector2(width, height);
            }
        }
        else
        {
            // Vertical or Horizontal layout (existing logic)
            Vector2 offset = Padding;

            foreach (var child in Children)
            {
                child.ComputeLayout();

                if ((!child.Visible && child.IgnoreLayoutWhenInvisible) || child.LayoutPosition == LayoutPosition.Absolute)
                    continue; // skip ignored invisible and absolute positioned elements


                if (Direction == LayoutDirection.Vertical)
                {
                    float x = offset.X + child.Margin.X;

                    switch (child.Align)
                    {
                        case Alignment.Center:
                            x += (Size.X - Padding.X * 2 - child.Size.X) / 2;
                            break;
                        case Alignment.End:
                            x += (Size.X - Padding.X * 2 - child.Size.X);
                            break;
                        case Alignment.Stretch:
                            child.Size = new Vector2(Size.X - Padding.X * 2 - child.Margin.X * 2, child.Size.Y);
                            break;
                    }

                    child.Position = new Vector2(x, offset.Y + child.Margin.Y);
                    offset.Y += child.Size.Y + Spacing + child.Margin.Y;
                }
                else // Horizontal
                {
                    float y = offset.Y + child.Margin.Y;

                    switch (child.Align)
                    {
                        case Alignment.Center:
                            y += (Size.Y - Padding.Y * 2 - child.Size.Y) / 2;
                            break;
                        case Alignment.End:
                            y += (Size.Y - Padding.Y * 2 - child.Size.Y);
                            break;
                        case Alignment.Stretch:
                            child.Size = new Vector2(child.Size.X, Size.Y - Padding.Y * 2 - child.Margin.Y * 2);
                            break;
                    }

                    child.Position = new Vector2(offset.X + child.Margin.X, y);
                    offset.X += child.Size.X + Spacing + child.Margin.X;
                }
            }

            if (AutoSize)
            {
                if (Direction == LayoutDirection.Vertical)
                {
                    float width = 0f;
                    foreach (var c in Children) width = Math.Max(width, c.Size.X + c.Margin.X);
                    Size = new Vector2(width + Padding.X * 2, offset.Y + Padding.Y);
                }
                else
                {
                    float height = 0f;
                    foreach (var c in Children) height = Math.Max(height, c.Size.Y + c.Margin.Y);
                    Size = new Vector2(offset.X + Padding.X, height + Padding.Y * 2);
                }
            }
        }

        base.ComputeLayout();
    }

    public override void Update()
    {
        base.Update();
        if (!Visible || !Enabled)
            return;

        if (IsRoot)
            Input();

        foreach (var child in Children)
            if (child.Visible) child.Update();
    }

    private static bool mouseProcessed(UIElement element, HashSet<UIElement> alreadyInput)
    {
        if (!element.Visible || !element.Enabled)
            return false;

        // Input propagation: top-most first
        if (element is UIContainer container)
        {
            for (int i = container.Children.Count - 1; i >= 0; i--)
            {
                if (mouseProcessed(container.Children[i], alreadyInput))
                    return true;
            }
        }
        else
        {
            if (element.MouseFilter == MouseFilter.Ignore)
                return false;

            if (!element.IsMouseOver())
                return false;

            // ALWAYS deliver input
            element.Input();

            bool handled = element.HandleMouseInput();

            // Only stop traversal if Stop + handled
            return handled && element.MouseFilter == MouseFilter.Stop;
        }

        return false;
    }

    private static void callInput(UIElement element, HashSet<UIElement> alreadyInput)
    {
        if (!element.Visible || !element.Enabled || alreadyInput.Contains(element))
            return;

        // Input propagation: top-most first
        if (element is UIContainer container)
        {
            for (int i = container.Children.Count - 1; i >= 0; i--)
            {
                callInput(container.Children[i], alreadyInput);
            }

            return;
        }

        element.Input();
    }

    public override void Input()
    {
        base.Input();
        if (!Visible || !Enabled || !IsRoot)
            return;

        HashSet<UIElement> alreadyInput = new();

        if (mouseProcessed(this, alreadyInput))
            return;

        callInput(this, alreadyInput);
    }

    public override void Render()
    {
        base.Render();
        if (!Visible) return;

        foreach (var child in Children)
            if (child.Visible) child.Render();
    }
}
