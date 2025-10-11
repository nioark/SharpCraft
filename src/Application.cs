using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Drawing;
using System.Diagnostics;
class Application : GameWindow
{
    public int width { get; private set; } = 800;
    public int height { get; private set; } = 600;

    public string title { get; private set; }

    private World world;
    private Chunk chunk;

    private PlayerController player;


    public Application(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {

        this.width = width;
        this.height = height;
        this.title = title;
    }

    float[] vertices = new float[]{
        -0.5f, -0.5f, 0.0f,
        0.5f, -0.5f, 0.0f,
        0.0f,  0.5f, 0.0f
    };

    int VBO, VAO;
    int vertexShader;

    GlShader BasicShader;

    GlShader ChunkShader;
    protected override void OnLoad()
    {
        base.OnLoad();

        //Background Color
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Front);
        //GL.Enable(EnableCap.Blend);

        GL.ClearColor(135 / 255f, 206 / 255f, 235 / 255f, 1.0f);


        VAO = GL.GenVertexArray();
        GL.BindVertexArray(VAO);

        VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        BasicShader = new GlShader("res/Shaders/shader.vert", "res/Shaders/shader.frag");
        BasicShader.Use();

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        BasicShader.SetMatrix4("model", Matrix4.CreateTranslation(0, 100, 0));

        player = new PlayerController(new Vector3(0, 100, 0), width, height);

        Blocks.Initialize();

        //ChunkShader = new GlShader("res/Shaders/chunkGreedyVert.glsl","res/Shaders/chunkGreedyFrag.glsl");
        ChunkShader = new GlShader("res/Shaders/chunkVert.glsl", "res/Shaders/chunkFrag.glsl");

        world = new World(69696969, player.camera, ChunkShader);
        //world.StartUpdateThread();
        world.generateChunkSquare();


        CursorGrabbed = true;

    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        //World rendering
        ChunkShader.SetMatrix4("view", player.camera.GetViewMatrix());
        ChunkShader.SetMatrix4("projection", player.camera.GetProjectionMatrix());


        world.UpdateChunkBuffer();
        //world.UpdateChunkBuffer();// TWO CHUNKS PER FRAME

        world.Render();

        //Simple triangle
        GL.BindVertexArray(VAO);
        BasicShader.Use();
        BasicShader.SetMatrix4("view", player.camera.GetViewMatrix());
        BasicShader.SetMatrix4("projection", player.camera.GetProjectionMatrix());
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);


        SwapBuffers();

        stopwatch.Stop();
        //Console.WriteLine((int)stopwatch.ElapsedMilliseconds);
        //Thread.Sleep((int)(1000 / 200) - (int)stopwatch.ElapsedMilliseconds);
        //Thread.Sleep(1);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        var input = KeyboardState;

        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        var mouse = MouseState;

        player.Update((float)e.Time, input, mouse);

        //SizeF size = FontDrawing.Print(Font, "text2", (OpenTK.Vector3)new Vector3(3,3,-2), QFontAlignment.Left);


        if (!IsFocused) // Check to see if the window is focused
        {
            return;
        }



        //camera.Update(e.Time);


    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        //GL.Viewport(0, 0, Size.X, Size.Y);
        // We need to update the aspect ratio once the window has been resized.
        //_camera.AspectRatio = Size.X / (float)Size.Y;
    }

}