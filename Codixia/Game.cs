using Raylib_cs;
using System.Numerics;

namespace Codixia;

public abstract class Game
{
    // Target canvas resolution
    private int _canvasWidth = 960;
    private int _canvasHeight = 540;
    private bool _running;

    /// <summary>
    /// Virtual canvas width.
    /// </summary>
    public int CanvasWidth => _canvasWidth;
    /// <summary>
    /// Virtual canvas height.
    /// </summary>
    public int CanvasHeight => _canvasHeight;

    /// <summary>
    /// The currently active scene.
    /// </summary>
    public Scene? CurrentScene { get; set; }

    /// <summary>
    /// Determines the trace log level for Raylib. (Has no effect after <see cref="Run"/> is called)
    /// </summary>
    public TraceLogLevel TraceLogLevel { get; set; } = TraceLogLevel.Warning | TraceLogLevel.Error | TraceLogLevel.Fatal;
    /// <summary>
    /// Sets the configuration flags for Raylib. (Has no effect after <see cref="Run"/> is called)
    /// </summary>
    public ConfigFlags ConfigFlags = ConfigFlags.ResizableWindow;
    /// <summary>
    /// Sets the scale of the game window relative to the virtual size. (Has no effect after <see cref="Run"/> is called)
    /// </summary>
    public float WindowScale { get; set; } = 1.0f;
    /// <summary>
    /// Sets the title of the game window. (Has no effect after <see cref="Run"/> is called)
    /// </summary>
    public string Title { get; set; } = "Codixa Game";

    /// <summary>
    /// Determines whether to draw the FPS counter.
    /// </summary>
    public bool DrawFPS { get; set; } = true;

    /// <summary>
    /// Called before the game loop starts.
    /// </summary>
    public Action Initialize { get; set; } = () => { };
    /// <summary>
    /// Called once at the end of the current scene update
    /// </summary>
    public Action<float> Update { get; set; } = (dt) => { };

    protected Game(int virtual_width = 960, int virtual_height = 540)
    {
        _canvasWidth = virtual_width;
        _canvasHeight = virtual_height;
    }

    /// <summary>
    /// Starts running the game loop.
    /// </summary>
    public void Run()
    {
        Raylib.SetTraceLogLevel(TraceLogLevel);
        Raylib.SetConfigFlags(ConfigFlags);

        // Create the real window
        Raylib.InitWindow((int)(_canvasWidth * WindowScale), (int)(_canvasHeight * WindowScale), Title);
        Raylib.SetTargetFPS(60);

        Raylib.SetExitKey(KeyboardKey.Null);

        RenderTexture2D canvas = Raylib.LoadRenderTexture(_canvasWidth, _canvasHeight);

        Initialize?.Invoke();

        _running = true;
        while (!Raylib.WindowShouldClose() && _running)
        {
            var dt = Raylib.GetFrameTime();
            CurrentScene?.Update(dt);
            Update?.Invoke(dt);

            Raylib.BeginTextureMode(canvas);
            Raylib.ClearBackground(Color.SkyBlue);

            CurrentScene?.Render();

            Raylib.EndTextureMode();


            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            DrawLetterboxedCanvas(canvas);
            if (DrawFPS) Raylib.DrawFPS(10, 10);
            Raylib.EndDrawing();
        }

        Raylib.UnloadRenderTexture(canvas);
        Raylib.CloseWindow();
    }

    /// <summary>
    /// Ends the game loop.
    /// </summary>
    public void Quit()
    {
        _running = false;
    }

    // ---------------------------------------------------
    // Convert actual mouse position -> canvas coordinates
    // ---------------------------------------------------
    /// <summary>
    /// Returns the mouse position in canvas coordinates, accounting for letterboxing.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetMousePositionOnCanvas()
    {
        int winW = Raylib.GetScreenWidth();
        int winH = Raylib.GetScreenHeight();

        float targetAspect = (float)_canvasWidth / _canvasHeight;
        float windowAspect = (float)winW / winH;

        float scale;
        float offsetX = 0, offsetY = 0;

        if (windowAspect > targetAspect)
        {
            scale = (float)winH / _canvasHeight;
            offsetX = (winW - _canvasWidth * scale) * 0.5f;
        }
        else
        {
            scale = (float)winW / _canvasWidth;
            offsetY = (winH - _canvasHeight * scale) * 0.5f;
        }

        Vector2 mouse = Raylib.GetMousePosition();

        return new Vector2(
            (mouse.X - offsetX) / scale,
            (mouse.Y - offsetY) / scale
        );
    }

    // ---------------------------------------------------
    // Draws the canvas centered with letterboxing
    // ---------------------------------------------------
    void DrawLetterboxedCanvas(RenderTexture2D canvas)
    {
        int winW = Raylib.GetScreenWidth();
        int winH = Raylib.GetScreenHeight();

        float targetAspect = (float)_canvasWidth / _canvasHeight;
        float windowAspect = (float)winW / winH;

        float drawW, drawH;

        if (windowAspect > targetAspect)
        {
            drawH = winH;
            drawW = drawH * targetAspect;
        }
        else
        {
            drawW = winW;
            drawH = drawW / targetAspect;
        }

        float posX = (winW - drawW) * 0.5f;
        float posY = (winH - drawH) * 0.5f;

        // Flip vertical because RenderTexture is upside-down in Raylib
        Raylib.DrawTexturePro(
            canvas.Texture,
            new Rectangle(0, 0, _canvasWidth, -_canvasHeight),
            new Rectangle(posX, posY, drawW, drawH),
            Vector2.Zero,
            0f,
            Color.White
        );
    }
}
