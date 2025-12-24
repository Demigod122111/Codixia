using Raylib_cs;
using System.Numerics;

namespace Codixia.UI;

public class Panel : UIElement
{
    // Background options
    public Color BackgroundColor = Color.DarkGray;
    public Texture2D? BackgroundTexture = null;

    // Optional tiling for texture
    public bool TileTexture = true;

    public Panel()
    {
        MouseFilter = MouseFilter.Stop;
    }

    public override bool HandleMouseInput()
    {
        return true;
    }

    public override void Render()
    {
        if (!Visible) return;

        // Draw background
        if (BackgroundTexture != null)
        {
            if (TileTexture)
            {
                // Simple tiling based on Size
                int tilesX = (int)MathF.Ceiling(Size.X / BackgroundTexture.Value.Width);
                int tilesY = (int)MathF.Ceiling(Size.Y / BackgroundTexture.Value.Height);

                for (int x = 0; x < tilesX; x++)
                {
                    for (int y = 0; y < tilesY; y++)
                    {
                        Rectangle source = new Rectangle(0, 0, BackgroundTexture.Value.Width, BackgroundTexture.Value.Height);
                        Rectangle dest = new Rectangle(GlobalPosition.X + x * BackgroundTexture.Value.Width,
                                                       GlobalPosition.Y + y * BackgroundTexture.Value.Height,
                                                       BackgroundTexture.Value.Width,
                                                       BackgroundTexture.Value.Height);
                        Raylib.DrawTexturePro(BackgroundTexture.Value, source, dest, Vector2.Zero, 0f, Color.White);
                    }
                }
            }
            else
            {
                // Stretch to panel size
                Rectangle source = new Rectangle(0, 0, BackgroundTexture.Value.Width, BackgroundTexture.Value.Height);
                Rectangle dest = new Rectangle(GlobalPosition.X, GlobalPosition.Y, Size.X, Size.Y);
                Raylib.DrawTexturePro(BackgroundTexture.Value, source, dest, Vector2.Zero, 0f, Color.White);
            }
        }
        else
        {
            Raylib.DrawRectangle((int)GlobalPosition.X, (int)GlobalPosition.Y, (int)Size.X, (int)Size.Y, BackgroundColor);
        }
    }
}
