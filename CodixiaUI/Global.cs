using Raylib_cs;
using System.Numerics;

namespace Codixia.UI;

public static class Global
{
    public static Func<float> GetDeltaTime
    {
        get
        {
            if (_getDeltaTime == null)
                throw new NotImplementedException("Codixia.UI.Global.GetDeltaTime is unassigned.");

            return _getDeltaTime;
        }

        set => _getDeltaTime = value;
    }

    static Func<float>? _getDeltaTime = Raylib.GetFrameTime;

    public static Func<Vector2> GetMousePosition
    {
        get
        {
            if (_getMousePosition == null)
                throw new NotImplementedException("Codixia.UI.Global.GetMousePosition is unassigned.");

            return _getMousePosition;
        }

        set => _getMousePosition = value;
    }

    static Func<Vector2>? _getMousePosition = Raylib.GetMousePosition;
}
