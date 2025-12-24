using System;
using System.Numerics;

namespace Codixia;

public static class Noise
{
    public static float Hash(int x, int y)
    {
        int h = x * 374761393 + y * 668265263;
        h = (h ^ h >> 13) * 1274126177;
        return (h & 0xFFFFFF) / 16777216f;
    }

    public static float Smooth(float n) => n * n * (3 - 2 * n);

    public static float Noise2D(float x, float y)
    {
        int xi = (int)MathF.Floor(x);
        int yi = (int)MathF.Floor(y);

        float xf = x - xi;
        float yf = y - yi;

        float a = Hash(xi, yi);
        float b = Hash(xi + 1, yi);
        float c = Hash(xi, yi + 1);
        float d = Hash(xi + 1, yi + 1);

        float u = Smooth(xf);
        float v = Smooth(yf);

        return Lerp(
            Lerp(a, b, u),
            Lerp(c, d, u),
            v
        );
    }

    public static float Fbm2D(float x, float y, int octaves, float gain)
    {
        float amp = 1f;
        float freq = 1f;
        float sum = 0;

        for (int i = 0; i < octaves; i++)
        {
            sum += Noise2D(x * freq, y * freq) * amp;
            amp *= gain;
            freq *= 2f;
        }

        return sum / 1.5f;
    }

    public static float Fbm2D2(float x, float y, int octaves, float persistence)
    {
        float total = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            total += PerlinNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2f;
        }

        return total / maxValue;
    }

    private static float PerlinNoise(float x, float y)
    {
        // Simple perlin-like noise implementation
        int xi = (int)Math.Floor(x);
        int yi = (int)Math.Floor(y);

        float xf = x - xi;
        float yf = y - yi;

        float n00 = DotGridGradient(xi, yi, x, y);
        float n10 = DotGridGradient(xi + 1, yi, x, y);
        float n01 = DotGridGradient(xi, yi + 1, x, y);
        float n11 = DotGridGradient(xi + 1, yi + 1, x, y);

        float u = Fade(xf);
        float v = Fade(yf);

        float nx0 = Lerp(n00, n10, u);
        float nx1 = Lerp(n01, n11, u);

        return Lerp(nx0, nx1, v);
    }

    private static float DotGridGradient(int ix, int iy, float x, float y)
    {
        Vector2 gradient = RandomGradient(ix, iy);
        float dx = x - ix;
        float dy = y - iy;
        return dx * gradient.X + dy * gradient.Y;
    }

    private static Vector2 RandomGradient(int ix, int iy)
    {
        uint w = 32;
        uint s = w / 2;
        uint a = (uint)ix, b = (uint)iy;
        a *= 3284157443;
        b ^= a << (int)s | a >> (int)(w - s);
        b *= 1911520717;
        a ^= b << (int)s | b >> (int)(w - s);
        a *= 2048419325;
        float random = a * (3.14159265f / ~(~0u >> 1));

        return new Vector2(MathF.Cos(random), MathF.Sin(random));
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
}