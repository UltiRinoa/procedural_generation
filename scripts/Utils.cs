using System;
using Godot;

public static class Utils
{
    public static float BoxSdf(Vector2 position, Vector2 size)
    {
        var dst = new Vector2(Mathf.Abs(position.X), Mathf.Abs(position.Y)) - size;
        return new Vector2(Mathf.Max(dst.X, 0), Mathf.Max(dst.Y, 0)).Length() + Mathf.Min(Mathf.Max(dst.X, dst.Y), 0);
    }
}