using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

class PlayerController
{

    public Vector3 playerPos { get; private set; }
    private Vector3 velocity = new Vector3();
    public Camera camera;
    const float cameraSpeed = 100.5f;
    const float sensitivity = 0.2f;

    private bool _firstMove = true;

    private Vector2 _lastPos;

    #region Collision size

    private Vector3 boxMin = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 boxMax = new Vector3(1.0f, 2.0f, 1.0f);

    #endregion

    public PlayerController(Vector3 playerPos, float width, float height)
    {
        this.playerPos = playerPos;
        camera = new Camera(Vector3.UnitZ * 3, width / height);
        camera.Position = playerPos + new Vector3(0f, 1f, 0);
    }




    public void Update(float deltaTime, KeyboardState input, MouseState mouse)
    {

        if (input.IsKeyDown(Keys.W))
        {
            camera.Position += camera.Front * cameraSpeed * deltaTime; // Forward
        }

        if (input.IsKeyDown(Keys.S))
        {
            camera.Position -= camera.Front * cameraSpeed * deltaTime; // Backwards
        }
        if (input.IsKeyDown(Keys.A))
        {
            camera.Position -= camera.Right * cameraSpeed * deltaTime; // Left
        }
        if (input.IsKeyDown(Keys.D))
        {
            camera.Position += camera.Right * cameraSpeed * deltaTime; // Right
        }
        if (input.IsKeyDown(Keys.Space))
        {
            camera.Position += camera.Up * cameraSpeed * deltaTime; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            camera.Position -= camera.Up * cameraSpeed * deltaTime; // Down
        }


        if (_firstMove) // This bool variable is initially set to true.
        {
            _lastPos = new Vector2(mouse.X, mouse.Y);
            _firstMove = false;
        }
        else
        {
            // Calculate the offset of the mouse position
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;
            _lastPos = new Vector2(mouse.X, mouse.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            camera.Yaw += deltaX * sensitivity;
            camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }
}