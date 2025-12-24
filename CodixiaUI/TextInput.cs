using Raylib_cs;
using System.Numerics;
using System.Text;

namespace Codixia.UI;

public class TextInput : UIElement
{
    public Font Font;
    public string Text = "";
    public string PlaceholderText = "";
    public float TextSpacing = 1.0f;
    public int FontSize = 20;
    public int MaxLength = 256;

    // Colors
    public Color BackgroundColor = new Color(40, 40, 40, 255);
    public Color FocusedBackgroundColor = new Color(50, 50, 50, 255);
    public Color BorderColor = Color.Gray;
    public Color FocusedBorderColor = Color.White;
    public Color TextColor = Color.White;
    public Color PlaceholderColor = Color.Gray;
    public Color SelectionColor = new Color(100, 150, 200, 150);
    public Color CursorColor = Color.White;

    public int BorderThickness = 2;

    // State
    private bool _isFocused = false;
    private bool _isHovered = false;
    private int _cursorPosition = 0;
    private float _cursorBlinkTimer = 0f;
    private const float CursorBlinkInterval = 0.5f;

    // Selection
    private int _selectionStart = -1;
    private int _selectionEnd = -1;

    // Scrolling for long text
    private float _textScrollOffset = 0f;

    // Key repeat
    private KeyboardKey _lastKey = KeyboardKey.Null;
    private float _keyRepeatTimer = 0f;
    private const float KeyRepeatDelay = 0.5f;
    private const float KeyRepeatRate = 0.05f;
    private bool _keyInitialPress = false;

    public TextInput()
    {
        Padding = new(10, 5);
        Font = Raylib.GetFontDefault();
        MinSize = new Vector2(200, 40);
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

        // Handle focus
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (IsMouseOver())
            {
                if (!_isFocused)
                {
                    _isFocused = true;
                    _cursorBlinkTimer = 0f;
                }

                // Calculate cursor position from mouse click
                UpdateCursorFromMouse();
            }
            else
            {
                _isFocused = false;
                _selectionStart = -1;
                _selectionEnd = -1;
            }
        }

        if (!_isFocused) return;

        // Handle text selection with shift + arrow keys or mouse drag
        if (Raylib.IsMouseButtonDown(MouseButton.Left) && IsMouseOver())
        {
            int newPos = GetCursorPositionFromMouse();
            if (_selectionStart == -1)
            {
                _selectionStart = _cursorPosition;
            }
            _cursorPosition = newPos;
            _selectionEnd = _cursorPosition;
        }

        // Handle text input
        int key = Raylib.GetCharPressed();
        while (key > 0)
        {
            if (key >= 32 && key <= 126 && Text.Length < MaxLength)
            {
                DeleteSelection();
                Text = Text.Insert(_cursorPosition, ((char)key).ToString());
                _cursorPosition++;
                _cursorBlinkTimer = 0f;
            }
            key = Raylib.GetCharPressed();
        }

        // Handle special keys
        HandleSpecialKeys();
    }

    private void HandleSpecialKeys()
    {
        bool shift = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        bool ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);

        // Ctrl commands should NOT repeat - use IsKeyPressed only
        // Ctrl+A - Select all
        if (ctrl && Raylib.IsKeyPressed(KeyboardKey.A))
        {
            _selectionStart = 0;
            _selectionEnd = Text.Length;
            _cursorPosition = Text.Length;
            return; // Early return to prevent other key handling
        }

        // Ctrl+C - Copy
        if (ctrl && Raylib.IsKeyPressed(KeyboardKey.C) && HasSelection())
        {
            int start = Math.Min(_selectionStart, _selectionEnd);
            int end = Math.Max(_selectionStart, _selectionEnd);
            string selectedText = Text.Substring(start, end - start);
            Raylib.SetClipboardText(selectedText);
            return;
        }

        // Ctrl+X - Cut
        if (ctrl && Raylib.IsKeyPressed(KeyboardKey.X) && HasSelection())
        {
            int start = Math.Min(_selectionStart, _selectionEnd);
            int end = Math.Max(_selectionStart, _selectionEnd);
            string selectedText = Text.Substring(start, end - start);
            Raylib.SetClipboardText(selectedText);
            DeleteSelection();
            return;
        }

        // Ctrl+V - Paste
        if (ctrl && Raylib.IsKeyPressed(KeyboardKey.V))
        {
            string clipboard = Raylib.GetClipboardText_();
            if (!string.IsNullOrEmpty(clipboard))
            {
                DeleteSelection();

                // Filter out invalid characters and respect max length
                StringBuilder validText = new StringBuilder();
                foreach (char c in clipboard)
                {
                    if (c >= 32 && c <= 126 && Text.Length + validText.Length < MaxLength)
                    {
                        validText.Append(c);
                    }
                }

                if (validText.Length > 0)
                {
                    Text = Text.Insert(_cursorPosition, validText.ToString());
                    _cursorPosition += validText.Length;
                    _cursorBlinkTimer = 0f;
                }
            }
            return;
        }

        // Navigation keys (these use repeat)
        // Backspace
        if (HandleKeyWithRepeat(KeyboardKey.Backspace))
        {
            if (HasSelection())
            {
                DeleteSelection();
            }
            else if (_cursorPosition > 0)
            {
                Text = Text.Remove(_cursorPosition - 1, 1);
                _cursorPosition--;
            }
            _cursorBlinkTimer = 0f;
        }

        // Delete
        if (HandleKeyWithRepeat(KeyboardKey.Delete))
        {
            if (HasSelection())
            {
                DeleteSelection();
            }
            else if (_cursorPosition < Text.Length)
            {
                Text = Text.Remove(_cursorPosition, 1);
            }
            _cursorBlinkTimer = 0f;
        }

        // Left arrow
        if (HandleKeyWithRepeat(KeyboardKey.Left))
        {
            if (shift)
            {
                if (_selectionStart == -1)
                    _selectionStart = _cursorPosition;

                if (_cursorPosition > 0)
                    _cursorPosition--;

                _selectionEnd = _cursorPosition;
            }
            else
            {
                if (HasSelection())
                {
                    _cursorPosition = Math.Min(_selectionStart, _selectionEnd);
                    ClearSelection();
                }
                else if (_cursorPosition > 0)
                {
                    _cursorPosition--;
                }
            }
            _cursorBlinkTimer = 0f;
        }

        // Right arrow
        if (HandleKeyWithRepeat(KeyboardKey.Right))
        {
            if (shift)
            {
                if (_selectionStart == -1)
                    _selectionStart = _cursorPosition;

                if (_cursorPosition < Text.Length)
                    _cursorPosition++;

                _selectionEnd = _cursorPosition;
            }
            else
            {
                if (HasSelection())
                {
                    _cursorPosition = Math.Max(_selectionStart, _selectionEnd);
                    ClearSelection();
                }
                else if (_cursorPosition < Text.Length)
                {
                    _cursorPosition++;
                }
            }
            _cursorBlinkTimer = 0f;
        }

        // Home - use IsKeyPressed only (no repeat needed)
        if (Raylib.IsKeyPressed(KeyboardKey.Home))
        {
            if (shift)
            {
                if (_selectionStart == -1)
                    _selectionStart = _cursorPosition;
                _cursorPosition = 0;
                _selectionEnd = _cursorPosition;
            }
            else
            {
                _cursorPosition = 0;
                ClearSelection();
            }
            _cursorBlinkTimer = 0f;
        }

        // End - use IsKeyPressed only (no repeat needed)
        if (Raylib.IsKeyPressed(KeyboardKey.End))
        {
            if (shift)
            {
                if (_selectionStart == -1)
                    _selectionStart = _cursorPosition;
                _cursorPosition = Text.Length;
                _selectionEnd = _cursorPosition;
            }
            else
            {
                _cursorPosition = Text.Length;
                ClearSelection();
            }
            _cursorBlinkTimer = 0f;
        }
    }

    private bool HandleKeyWithRepeat(KeyboardKey key)
    {
        // Initial key press
        if (Raylib.IsKeyPressed(key))
        {
            _lastKey = key;
            _keyRepeatTimer = 0f;
            _keyInitialPress = true;
            return true;
        }

        // Key is being held down - but ONLY process if this is the key we're tracking
        if (_lastKey == key && Raylib.IsKeyDown(key))
        {
            _keyRepeatTimer += Raylib.GetFrameTime();

            // Wait for initial delay before starting repeat
            if (_keyInitialPress)
            {
                if (_keyRepeatTimer >= KeyRepeatDelay)
                {
                    _keyInitialPress = false;
                    _keyRepeatTimer = 0f;
                    return true;
                }
            }
            // After initial delay, repeat at the specified rate
            else
            {
                if (_keyRepeatTimer >= KeyRepeatRate)
                {
                    _keyRepeatTimer = 0f;
                    return true;
                }
            }
        }

        // Key was released - reset state
        if (Raylib.IsKeyReleased(key) && _lastKey == key)
        {
            _lastKey = KeyboardKey.Null;
            _keyRepeatTimer = 0f;
            _keyInitialPress = false;
        }

        return false;
    }

    public override void Update()
    {
        base.Update();

        if (!Visible) return;

        if (_isFocused)
        {
            _cursorBlinkTimer += Global.GetDeltaTime();
            if (_cursorBlinkTimer >= CursorBlinkInterval * 2)
            {
                _cursorBlinkTimer = 0f;
            }
        }

        // Update text scroll offset to keep cursor visible
        UpdateTextScroll();
    }

    public override void Render()
    {
        base.Render();

        if (!Visible) return;

        Color bgColor = _isFocused ? FocusedBackgroundColor : BackgroundColor;
        Color borderCol = _isFocused ? FocusedBorderColor : BorderColor;

        // Draw background
        Raylib.DrawRectangle(
            (int)GlobalPosition.X,
            (int)GlobalPosition.Y,
            (int)Size.X,
            (int)Size.Y,
            bgColor
        );

        // Draw border
        Raylib.DrawRectangleLines(
            (int)GlobalPosition.X,
            (int)GlobalPosition.Y,
            (int)Size.X,
            (int)Size.Y,
            borderCol
        );

        if (BorderThickness > 1)
        {
            for (int i = 1; i < BorderThickness; i++)
            {
                Raylib.DrawRectangleLines(
                    (int)GlobalPosition.X + i,
                    (int)GlobalPosition.Y + i,
                    (int)Size.X - i * 2,
                    (int)Size.Y - i * 2,
                    borderCol
                );
            }
        }

        // Set up scissor mode for text clipping
        int textAreaX = (int)(GlobalPosition.X + Padding.X);
        int textAreaY = (int)(GlobalPosition.Y + Padding.Y);
        int textAreaW = (int)(Size.X - Padding.X * 2);
        int textAreaH = (int)(Size.Y - Padding.Y * 2);

        Raylib.BeginScissorMode(textAreaX, textAreaY, textAreaW, textAreaH);

        string displayText = string.IsNullOrEmpty(Text) ? PlaceholderText : Text;
        Color displayColor = string.IsNullOrEmpty(Text) ? PlaceholderColor : TextColor;

        Vector2 textPos = new Vector2(
            textAreaX - _textScrollOffset,
            GlobalPosition.Y + (Size.Y - FontSize) * 0.5f
        );

        // Draw selection
        if (_isFocused && HasSelection())
        {
            DrawSelection(textPos);
        }

        // Draw text
        Raylib.DrawTextEx(Font, displayText, textPos, FontSize, TextSpacing, displayColor);

        // Draw cursor
        if (_isFocused && _cursorBlinkTimer < CursorBlinkInterval && !string.IsNullOrEmpty(displayText))
        {
            DrawCursor(textPos);
        }

        Raylib.EndScissorMode();

        _isHovered = false;
    }

    private void DrawSelection(Vector2 textPos)
    {
        int start = Math.Min(_selectionStart, _selectionEnd);
        int end = Math.Max(_selectionStart, _selectionEnd);

        if (start < 0 || end > Text.Length || start >= end) return;

        string beforeSelection = Text.Substring(0, start);
        string selection = Text.Substring(start, end - start);

        Vector2 beforeSize = Raylib.MeasureTextEx(Font, beforeSelection, FontSize, TextSpacing);
        Vector2 selectionSize = Raylib.MeasureTextEx(Font, selection, FontSize, TextSpacing);

        float selectionX = textPos.X + beforeSize.X;
        float selectionY = GlobalPosition.Y + Padding.Y;
        float selectionH = Size.Y - Padding.Y * 2;

        Raylib.DrawRectangle(
            (int)selectionX,
            (int)selectionY,
            (int)selectionSize.X,
            (int)selectionH,
            SelectionColor
        );
    }

    private void DrawCursor(Vector2 textPos)
    {
        string beforeCursor = Text.Substring(0, _cursorPosition);
        Vector2 beforeCursorSize = Raylib.MeasureTextEx(Font, beforeCursor, FontSize, TextSpacing);

        float cursorX = textPos.X + beforeCursorSize.X;
        float cursorY = GlobalPosition.Y + Padding.Y;
        float cursorH = Size.Y - Padding.Y * 2;

        Raylib.DrawRectangle(
            (int)cursorX,
            (int)cursorY,
            2,
            (int)cursorH,
            CursorColor
        );
    }

    private void UpdateTextScroll()
    {
        if (!_isFocused) return;

        string beforeCursor = Text.Substring(0, _cursorPosition);
        float cursorPosX = Raylib.MeasureTextEx(Font, beforeCursor, FontSize, TextSpacing).X;

        float visibleWidth = Size.X - Padding.X * 2;
        float cursorScreenX = cursorPosX - _textScrollOffset;

        // Scroll right if cursor is past the right edge
        if (cursorScreenX > visibleWidth - 10)
        {
            _textScrollOffset = cursorPosX - visibleWidth + 10;
        }
        // Scroll left if cursor is past the left edge
        else if (cursorScreenX < 10)
        {
            _textScrollOffset = Math.Max(0, cursorPosX - 10);
        }

        // Reset scroll if text is shorter than visible area
        float totalTextWidth = Raylib.MeasureTextEx(Font, Text, FontSize, TextSpacing).X;
        if (totalTextWidth <= visibleWidth)
        {
            _textScrollOffset = 0;
        }
    }

    private void UpdateCursorFromMouse()
    {
        _cursorPosition = GetCursorPositionFromMouse();
        ClearSelection();
    }

    private int GetCursorPositionFromMouse()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        float relativeX = mousePos.X - GlobalPosition.X - Padding.X + _textScrollOffset;

        for (int i = 0; i <= Text.Length; i++)
        {
            string substr = Text.Substring(0, i);
            float width = Raylib.MeasureTextEx(Font, substr, FontSize, TextSpacing).X;

            if (relativeX < width)
            {
                // Check if closer to previous or current position
                if (i > 0)
                {
                    string prevSubstr = Text.Substring(0, i - 1);
                    float prevWidth = Raylib.MeasureTextEx(Font, prevSubstr, FontSize, TextSpacing).X;
                    float midPoint = (prevWidth + width) / 2;

                    return relativeX < midPoint ? i - 1 : i;
                }
                return i;
            }
        }

        return Text.Length;
    }

    private bool HasSelection()
    {
        return _selectionStart >= 0 && _selectionEnd >= 0 && _selectionStart != _selectionEnd;
    }

    private void ClearSelection()
    {
        _selectionStart = -1;
        _selectionEnd = -1;
    }

    private void DeleteSelection()
    {
        if (!HasSelection()) return;

        int start = Math.Min(_selectionStart, _selectionEnd);
        int end = Math.Max(_selectionStart, _selectionEnd);

        Text = Text.Remove(start, end - start);
        _cursorPosition = start;
        ClearSelection();
    }

    public override void ComputeLayout()
    {
        if (AutoSize)
        {
            var textSize = Raylib.MeasureTextEx(Font, Text.Length > 0 ? Text : PlaceholderText, FontSize, TextSpacing);
            Size = new Vector2(
                Math.Max(MinSize.X, textSize.X + Padding.X * 2),
                Math.Max(MinSize.Y, textSize.Y + Padding.Y * 2)
            );
        }
        base.ComputeLayout();
    }
}