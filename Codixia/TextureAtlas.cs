using Raylib_cs;
using System.Numerics;

namespace Codixia;

public static class TextureAtlas
{
    private static Dictionary<string, AtlasEntry> _textureMap = new();
    private static Texture2D _atlasTexture;
    private static int _atlasSize = 16; // Grid size (e.g., 16x16 = 256 textures max)
    private static int _textureResolution = 16; // Default texture resolution in pixels
    private static bool _debug = false;
    public static Texture2D AtlasTexture => _atlasTexture;

    public struct AtlasEntry
    {
        /// <summary>
        /// Position and size in the atlas (in pixels)
        /// </summary>
        public Rectangle SourceRect;
        /// <summary>
        /// UV offset (normalized 0-1)
        /// </summary>
        public Vector2 UV;
        /// <summary>
        /// UV size (normalized 0-1)
        /// </summary>
        public Vector2 UVSize;
        /// <summary>
        /// Original texture width
        /// </summary>
        public int Width;
        /// <summary>
        /// Original texture height
        /// </summary>
        public int Height;  
    }

    /// <summary>
    /// Gets the atlas entry for a texture by its relative path
    /// </summary>
    public static AtlasEntry? GetTexture(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;

        // Normalize path separators
        string normalizedPath = relativePath.Replace('\\', '/').ToLowerInvariant().Trim();

        if (_textureMap.TryGetValue(normalizedPath, out AtlasEntry entry))
            return entry;

        return null;
    }

    /// <summary>
    /// Gets the source rectangle for a texture (useful for drawing)
    /// </summary>
    public static Rectangle? GetSourceRect(string relativePath)
    {
        var entry = GetTexture(relativePath);
        return entry?.SourceRect;
    }

    /// <summary>
    /// Loads all image files from a folder recursively and generates a texture atlas
    /// </summary>
    /// <param name="folderPath">Root folder to scan for images</param>
    /// <param name="textureResolution">Size (in pixels) to normalize each texture to (0 = keep original sizes)</param>
    public static unsafe void GenerateAtlas(string folderPath, int textureResolution = 16, bool debug = false)
    {
        _debug = debug;

        if (!Directory.Exists(folderPath))
        {
            if (_debug) Console.WriteLine($"Error: Folder '{folderPath}' does not exist");
            return;
        }

        _textureResolution = textureResolution;

        // Find all image files recursively
        string[] imageExtensions = { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" };
        List<string> imageFiles = new();

        foreach (var extension in imageExtensions)
        {
            imageFiles.AddRange(Directory.GetFiles(folderPath, extension, SearchOption.AllDirectories));
        }

        if (imageFiles.Count == 0)
        {
            if (_debug) Console.WriteLine($"No image files found in '{folderPath}'");
            return;
        }

        if (_debug) Console.WriteLine($"Found {imageFiles.Count} image files");

        // Calculate atlas size based on texture count
        _atlasSize = (int)Math.Ceiling(Math.Sqrt(imageFiles.Count));
        _atlasSize = Math.Max(2, _atlasSize); // Minimum 2x2

        int atlasPixelSize = _atlasSize * _textureResolution;

        if (_debug) Console.WriteLine($"Creating {_atlasSize}x{_atlasSize} atlas ({atlasPixelSize}x{atlasPixelSize} pixels)");

        // Create atlas image
        Image atlasImage = Raylib.GenImageColor(atlasPixelSize, atlasPixelSize, new Color(0, 0, 0, 0));

        // Process each image file
        int currentIndex = 0;
        foreach (var filePath in imageFiles)
        {
            int gridX = currentIndex % _atlasSize;
            int gridY = currentIndex / _atlasSize;

            // Get relative path from root folder
            string relativePath = Path.GetRelativePath(folderPath, filePath).Replace('\\', '/');

            try
            {
                Image textureImage = Raylib.LoadImage(filePath);

                int originalWidth = textureImage.Width;
                int originalHeight = textureImage.Height;

                // Resize if needed and resolution is specified
                if (_textureResolution > 0 &&
                    (textureImage.Width != _textureResolution || textureImage.Height != _textureResolution))
                {
                    Raylib.ImageResize(ref textureImage, _textureResolution, _textureResolution);
                }

                int texWidth = textureImage.Width;
                int texHeight = textureImage.Height;

                // Calculate position in atlas
                Rectangle sourceRect = new Rectangle(0, 0, texWidth, texHeight);
                Rectangle destRect = new Rectangle(
                    gridX * _textureResolution,
                    gridY * _textureResolution,
                    texWidth,
                    texHeight
                );

                // Draw texture onto atlas
                Raylib.ImageDraw(ref atlasImage, textureImage, sourceRect, destRect, Color.White);

                // Store atlas entry
                AtlasEntry entry = new AtlasEntry
                {
                    SourceRect = destRect,
                    UV = new Vector2(gridX / (float)_atlasSize, gridY / (float)_atlasSize),
                    UVSize = new Vector2(texWidth / (float)atlasPixelSize, texHeight / (float)atlasPixelSize),
                    Width = originalWidth,
                    Height = originalHeight
                };

                _textureMap[relativePath.ToLowerInvariant().Trim()] = entry;

                Raylib.UnloadImage(textureImage);

                if (_debug) Console.WriteLine($"  Added '{relativePath}' at grid ({gridX}, {gridY})");
            }
            catch (Exception ex)
            {
                if (_debug) Console.WriteLine($"  Error loading texture '{relativePath}': {ex.Message}");

                // Draw error pattern
                DrawErrorPattern(ref atlasImage, gridX * _textureResolution, gridY * _textureResolution);
            }

            currentIndex++;
        }

        // Load as texture
        _atlasTexture = Raylib.LoadTextureFromImage(atlasImage);

        // Set texture filter to nearest neighbor for pixel-perfect rendering
        Raylib.SetTextureFilter(_atlasTexture, TextureFilter.Point);

        Raylib.UnloadImage(atlasImage);

        if (_debug) Console.WriteLine("Texture atlas generation complete!");
    }

    /// <summary>
    /// Draws a texture from the atlas at the specified position
    /// </summary>
    public static void DrawTexture(string relativePath, Vector2 position, Color tint)
    {
        var entry = GetTexture(relativePath);
        if (entry == null)
        {
            if (_debug) Console.WriteLine($"Texture '{relativePath}' not found in atlas");
            return;
        }

        Raylib.DrawTextureRec(_atlasTexture, entry.Value.SourceRect, position, tint);
    }

    /// <summary>
    /// Draws a texture from the atlas with extended options
    /// </summary>
    public static void DrawTexturePro(string relativePath, Rectangle destRect, Vector2 origin, float rotation, Color tint)
    {
        var entry = GetTexture(relativePath);
        if (entry == null)
        {
            if (_debug) Console.WriteLine($"Texture '{relativePath}' not found in atlas");
            return;
        }

        Raylib.DrawTexturePro(_atlasTexture, entry.Value.SourceRect, destRect, origin, rotation, tint);
    }

    private static unsafe void DrawErrorPattern(ref Image image, int x, int y)
    {
        // Red/black checkerboard for errors
        for (int py = 0; py < _textureResolution; py++)
        {
            for (int px = 0; px < _textureResolution; px++)
            {
                bool isRed = (px / 4 + py / 4) % 2 == 0;
                Color color = isRed ? Color.Red : Color.Black;
                Raylib.ImageDrawPixel(ref image, x + px, y + py, color);
            }
        }
    }

    /// <summary>
    /// Unloads the texture atlas
    /// </summary>
    public static void UnloadAtlas()
    {
        if (_atlasTexture.Id != 0)
        {
            Raylib.UnloadTexture(_atlasTexture);
            _atlasTexture = default;
        }

        _textureMap.Clear();
    }

    /// <summary>
    /// Gets all loaded texture paths
    /// </summary>
    public static IEnumerable<string> GetAllTexturePaths()
    {
        return _textureMap.Keys;
    }

    /// <summary>
    /// Checks if a texture exists in the atlas
    /// </summary>
    public static bool HasTexture(string relativePath)
    {
        string normalizedPath = relativePath.Replace('\\', '/');
        return _textureMap.ContainsKey(normalizedPath);
    }

    /// <summary>
    /// Gets debug information about the atlas
    /// </summary>
    public static string GetAtlasInfo()
    {
        return $"Atlas: {_atlasSize}x{_atlasSize} grid, " +
               $"{_atlasSize * _textureResolution}x{_atlasSize * _textureResolution} pixels, " +
               $"{_textureMap.Count} textures loaded";
    }
}