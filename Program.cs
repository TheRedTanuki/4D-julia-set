using System.Numerics;
using Raylib_cs;
class Program
{
    static void Main()
    {
        Raylib.SetWindowState(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(0, 0, "Julia");
        
        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();
        float aspectRatio = width / height;

        Shader shader = Raylib.LoadShader(null, "ressources/julia_shader.glsl");
        RenderTexture2D target = Raylib.LoadRenderTexture(width, height);

        Vector3 cameraPosition = new (5.0f, 0.0f, 0.0f);
        Vector2 cameraAngle = new (0f, 0f);

        int cameraPositionLocation = Raylib.GetShaderLocation(shader, "cameraPosition");
        int cameraForwardLocation = Raylib.GetShaderLocation(shader, "cameraForward");
        int cameraRightLocation = Raylib.GetShaderLocation(shader, "cameraRight");
        int cameraUpLocation = Raylib.GetShaderLocation(shader, "cameraUp");
        int aspectRatioLocation = Raylib.GetShaderLocation(shader, "aspectRatio");

        Raylib.SetTargetFPS(60);
        Raylib.SetShaderValue(shader, aspectRatioLocation, aspectRatio, ShaderUniformDataType.Float);
        Raylib.HideCursor();

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsWindowResized())
            {
                Raylib.UnloadRenderTexture(target);
                target = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                Raylib.SetShaderValue(shader, aspectRatioLocation, aspectRatio, ShaderUniformDataType.Float);
            }

            var deltaTime = Raylib.GetFrameTime();

            //-- Camera direction -- //

            cameraAngle[0] += Raylib.GetMouseDelta()[0] * deltaTime / 2;
            cameraAngle[1] -= Raylib.GetMouseDelta()[1] * deltaTime / 2;
            Raylib.SetMousePosition((int)MathF.Round(width / 2f), (int)MathF.Round(height / 2f));

            // Angles
            float yaw = cameraAngle[0];
            float pitch = cameraAngle[1];

            // Forward
            Vector3 cameraForward = Vector3.Normalize(new(
                MathF.Cos(pitch) * MathF.Cos(yaw),
                MathF.Sin(pitch),
                MathF.Cos(pitch) * MathF.Sin(yaw)));

            // Right
            Vector3 cameraRight = new(MathF.Sin(yaw), 0, -MathF.Cos(yaw));

            // Up
            Vector3 cameraUp = Vector3.Cross(cameraForward, cameraRight);

            if (Raylib.IsKeyDown(KeyboardKey.S)) cameraPosition += 5f * deltaTime * cameraForward;
            if (Raylib.IsKeyDown(KeyboardKey.W)) cameraPosition -= 5f * deltaTime * cameraForward;
            if (Raylib.IsKeyDown(KeyboardKey.D)) cameraPosition += 5f * deltaTime * cameraRight;
            if (Raylib.IsKeyDown(KeyboardKey.A)) cameraPosition -= 5f * deltaTime * cameraRight;

            Raylib.SetShaderValue(shader, cameraPositionLocation, cameraPosition, ShaderUniformDataType.Vec3);

            Raylib.SetShaderValue(shader, cameraForwardLocation, cameraForward, ShaderUniformDataType.Vec3);
            Raylib.SetShaderValue(shader, cameraRightLocation, cameraRight, ShaderUniformDataType.Vec3);
            Raylib.SetShaderValue(shader, cameraUpLocation, cameraUp, ShaderUniformDataType.Vec3);

            //-- Drawing textures --//

            Raylib.BeginTextureMode(target);
            Raylib.ClearBackground(Color.Black);
            Raylib.EndTextureMode();

            Raylib.BeginDrawing();
            Raylib.BeginShaderMode(shader);
            Raylib.DrawTexture(target.Texture, 0, 0, Color.White);
            Raylib.EndShaderMode();
            Raylib.EndDrawing();

            // Optional : print fps
            //Console.WriteLine(Raylib.GetFPS());
        }

        Raylib.UnloadShader(shader);
        Raylib.CloseWindow();
    }
}