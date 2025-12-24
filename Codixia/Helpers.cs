using Raylib_cs;
using System.Numerics;

namespace Codixia;

public static class Helpers
{
    /// <summary>
    /// Draws a grid centered at (0, 0) (useful for visualizing the camera)
    /// </summary>
    /// <param name="divisions"></param>
    /// <param name="spacing"></param>
    public static void DrawGrid2D(int divisions, float spacing)
    {
        int halfDivisions = divisions / 2;

        for (int i = -halfDivisions; i <= halfDivisions; i++)
        {
            Color lineColor = (i == 0) ? Color.Red : new Color(100, 100, 100, 100);

            // Vertical lines
            Raylib.DrawLineV(
                new Vector2(i * spacing, -halfDivisions * spacing),
                new Vector2(i * spacing, halfDivisions * spacing),
                lineColor
            );

            // Horizontal lines
            Raylib.DrawLineV(
                new Vector2(-halfDivisions * spacing, i * spacing),
                new Vector2(halfDivisions * spacing, i * spacing),
                lineColor
            );
        }

        // Draw origin marker
        Raylib.DrawCircleV(Vector2.Zero, 5, Color.Yellow);
    }
}
