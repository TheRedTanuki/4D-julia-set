using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Raylib_cs;
class Program {
    static void Main() {
        Raylib.SetWindowState(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(0, 0, "Julia");

        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();
        float aspectRatio = width / height;
        float fov = 1.5f;

        Shader shader = Raylib.LoadShader(null, "ressources/julia_shader.glsl");
        RenderTexture2D target = Raylib.LoadRenderTexture(width, height);

        Vector3 cameraPosition = new (5.0f, 0.0f, 0.0f);
        Vector2 cameraAngle = new (0f, 0f);

        Vector4 c = new (0.2f, 0.7f, 0.5f, 0.0f);

        int cameraPositionLocation = Raylib.GetShaderLocation(shader, "cameraPosition");
        int cameraForwardLocation = Raylib.GetShaderLocation(shader, "cameraForward");
        int cameraRightLocation = Raylib.GetShaderLocation(shader, "cameraRight");
        int cameraUpLocation = Raylib.GetShaderLocation(shader, "cameraUp");

        float rotationAngle = 0f;
        int colorGradientLocation = Raylib.GetShaderLocation(shader, "colorGradient");

        uint[] colorGradientHex = {
            0x1E183A,
            0x3E285C,
            0x6B4276,
            0x974972,
            0xC33C57,
            0xDA4450,
            0xE85245,
            0xF26A40,
            0xF98F4D,
            0xFDA649
        };

        Vector3[] colorGradient = colorGradientHex.Select((hexColor) => new Vector3(
            (float)(hexColor >> 16) / 255f,
            (float)((hexColor & 0xFF00) >> 8) / 255f,
            (float)(hexColor & 0xFF) / 255f
            )).ToArray();

        Raylib.SetShaderValueV(shader, colorGradientLocation, colorGradient, ShaderUniformDataType.Vec3, colorGradient.Length);

        int cLocation = Raylib.GetShaderLocation(shader, "c");

        Raylib.SetTargetFPS(60);
        Raylib.HideCursor();
        Raylib.SetMousePosition((int)MathF.Round(width / 2f), (int)MathF.Round(height / 2f));

        while (!Raylib.WindowShouldClose()) {
            if (Raylib.IsWindowResized()) {
                Raylib.UnloadRenderTexture(target);
                width = Raylib.GetScreenWidth();
                height = Raylib.GetScreenHeight();
                target = Raylib.LoadRenderTexture(width, height);
                aspectRatio = width / height;
            }

            var deltaTime = Raylib.GetFrameTime();

            //-- Camera direction -- //

            cameraAngle[0] += Raylib.GetMouseDelta()[0] * deltaTime / 2;
            cameraAngle[1] -= Raylib.GetMouseDelta()[1] * deltaTime / 2;
            Raylib.SetMousePosition((int)MathF.Round(width / 2f), (int)MathF.Round(height / 2f));

            // Angles
            float yaw = cameraAngle[0];
            float pitch = cameraAngle[1];
            /*float yaw = rotationAngle;
            float pitch = 0f;*/

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

            //cameraPosition = new(5 * MathF.Cos(rotationAngle), 0f, 5 * MathF.Sin(rotationAngle));
            float scale = MathF.Tan(fov * 0.5f);

            Raylib.SetShaderValue(shader, cameraPositionLocation, cameraPosition, ShaderUniformDataType.Vec3);

            Raylib.SetShaderValue(shader, cameraForwardLocation, cameraForward, ShaderUniformDataType.Vec3);
            Raylib.SetShaderValue(shader, cameraRightLocation, Vector3.Multiply(scale * aspectRatio, cameraRight), ShaderUniformDataType.Vec3);
            Raylib.SetShaderValue(shader, cameraUpLocation, Vector3.Multiply(scale, cameraUp), ShaderUniformDataType.Vec3);

            if (Raylib.IsKeyDown(KeyboardKey.Y)) c[0] += 1f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.T)) c[0] -= 1f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.I)) c[1] += 1f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.U)) c[1] -= 1f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.P)) c[2] += 1f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.O)) c[2] -= 1f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.J)) c[3] += 1f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.H)) c[3] -= 1f * deltaTime;

            /*c[0] = -0.2f + 0.2f * MathF.Sin(2f * rotationAngle);
            c[1] = 0.7f + 0.1f * MathF.Sin(rotationAngle);
            c[2] = 0.3f * MathF.Sin(0.5f * rotationAngle);*/

            Raylib.SetShaderValue(shader, cLocation, c, ShaderUniformDataType.Vec4);


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
            Console.WriteLine(Raylib.GetFPS());
            //rotationAngle += 0.01f;
        }

        Raylib.UnloadShader(shader);
        Raylib.CloseWindow();
    }
}