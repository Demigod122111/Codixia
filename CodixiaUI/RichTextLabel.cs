using Raylib_cs;
using System.Numerics;

namespace Codixia.UI;

public class RichTextLabel : UIElement
{
    public Font Font;
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                _needsReparse = true;
            }
        }
    }
    public float TextSpacing = 1.0f;
    public int FontSize = 20;
    public Color DefaultColor = Color.White;

    private string _text = "";
    private List<TextSegment> _segments = new();
    private bool _needsReparse = true;

    public RichTextLabel()
    {
        Font = Raylib.GetFontDefault();
        MouseFilter = MouseFilter.Ignore;
        AutoSize = true;
    }

    private class TextSegment
    {
        public string Text;
        public Color Color;
        public bool Bold;
        public bool Italic;
        public Vector2 Position;
        public Vector2 Size;

        public TextSegment(string text, Color color, bool bold, bool italic)
        {
            Text = text;
            Color = color;
            Bold = bold;
            Italic = italic;
        }
    }

    private void ParseText()
    {
        _segments.Clear();
        if (string.IsNullOrEmpty(Text))
        {
            _needsReparse = false;
            return;
        }

        var colorStack = new Stack<Color>();
        colorStack.Push(DefaultColor);
        bool bold = false;
        bool italic = false;

        int pos = 0;
        string currentText = "";

        while (pos < Text.Length)
        {
            if (Text[pos] == '[')
            {
                // Save current text segment before processing tag
                if (currentText.Length > 0)
                {
                    _segments.Add(new TextSegment(currentText, colorStack.Peek(), bold, italic));
                    currentText = "";
                }

                int closePos = Text.IndexOf(']', pos);
                if (closePos == -1) break;

                string tag = Text.Substring(pos + 1, closePos - pos - 1);

                if (tag == "b")
                {
                    bold = true;
                }
                else if (tag == "/b")
                {
                    bold = false;
                }
                else if (tag == "i")
                {
                    italic = true;
                }
                else if (tag == "/i")
                {
                    italic = false;
                }
                else if (tag.StartsWith("color="))
                {
                    string colorStr = tag.Substring(6);
                    Color newColor = ParseColor(colorStr);
                    colorStack.Push(newColor);
                }
                else if (tag == "/color")
                {
                    if (colorStack.Count > 1)
                        colorStack.Pop();
                }

                pos = closePos + 1;
            }
            else
            {
                currentText += Text[pos];
                pos++;
            }
        }

        // Add remaining text
        if (currentText.Length > 0)
        {
            _segments.Add(new TextSegment(currentText, colorStack.Peek(), bold, italic));
        }

        _needsReparse = false;
    }

    private Color ParseColor(string colorStr)
    {
        if (colorStr.StartsWith("#"))
        {
            colorStr = colorStr.Substring(1);
            if (colorStr.Length == 6)
            {
                int r = Convert.ToInt32(colorStr.Substring(0, 2), 16);
                int g = Convert.ToInt32(colorStr.Substring(2, 2), 16);
                int b = Convert.ToInt32(colorStr.Substring(4, 2), 16);
                return new Color(r, g, b, 255);
            }
            else if (colorStr.Length == 8)
            {
                int r = Convert.ToInt32(colorStr.Substring(0, 2), 16);
                int g = Convert.ToInt32(colorStr.Substring(2, 2), 16);
                int b = Convert.ToInt32(colorStr.Substring(4, 2), 16);
                int a = Convert.ToInt32(colorStr.Substring(6, 2), 16);
                return new Color(r, g, b, a);
            }
        }

        // Named colors
        return colorStr.ToLower() switch
        {
            "red" => Color.Red,
            "green" => Color.Green,
            "blue" => Color.Blue,
            "yellow" => Color.Yellow,
            "white" => Color.White,
            "black" => Color.Black,
            "gray" => Color.Gray,
            "orange" => Color.Orange,
            "purple" => Color.Purple,
            _ => DefaultColor
        };
    }

    public override void ComputeLayout()
    {
        Text ??= "";

        if (_needsReparse)
            ParseText();

        if (AutoSize)
        {
            float totalWidth = 0;
            float maxHeight = 0;
            Vector2 currentPos = Vector2.Zero;

            foreach (var segment in _segments)
            {
                var textSize = Raylib.MeasureTextEx(Font, segment.Text, FontSize, TextSpacing);
                segment.Position = currentPos;
                segment.Size = textSize;

                currentPos.X += textSize.X;
                totalWidth = Math.Max(totalWidth, currentPos.X);
                maxHeight = Math.Max(maxHeight, textSize.Y);
            }

            Size = new Vector2(totalWidth + Padding.X * 2, maxHeight + Padding.Y * 2);
        }

        base.ComputeLayout();
    }

    public override void Render()
    {
        base.Render();
        if (!Visible) return;

        if (_needsReparse)
        {
            ParseText();
            ComputeLayout();
        }

        // Calculate centering offset
        float totalWidth = 0;
        float maxHeight = 0;
        foreach (var segment in _segments)
        {
            totalWidth += segment.Size.X;
            maxHeight = Math.Max(maxHeight, segment.Size.Y);
        }

        Vector2 offset = new Vector2(
            (Size.X - totalWidth) * 0.5f,
            (Size.Y - maxHeight) * 0.5f
        );

        foreach (var segment in _segments)
        {
            Vector2 drawPos = GlobalPosition + offset + segment.Position;

            // Simple bold effect: draw text multiple times with slight offsets
            if (segment.Bold)
            {
                Raylib.DrawTextEx(Font, segment.Text, drawPos + new Vector2(1, 0), FontSize, TextSpacing, segment.Color);
                Raylib.DrawTextEx(Font, segment.Text, drawPos + new Vector2(0, 1), FontSize, TextSpacing, segment.Color);
            }

            // Italic effect: use shear/skew (requires custom rendering)
            // For now, just draw normally
            Raylib.DrawTextEx(Font, segment.Text, drawPos, FontSize, TextSpacing, segment.Color);
        }
    }
}