using System;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

class Program
{

    /// <summary>
    /// Obligatory name for your first OpenGL example program.
    /// </summary>

    private static float r(int x)
    {
        return (float)x / 255f;
    }

    static void Main(string[] args)
    {

        // Create a window and shader program
        // Window configs
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(1200, 720),
            Title = "SharpCraft",
            // This is needed to run on macos
            Flags = ContextFlags.ForwardCompatible,
        };

        using (var app = new Application(GameWindowSettings.Default, nativeWindowSettings))
        {
            app.Run();
        }
    }

}