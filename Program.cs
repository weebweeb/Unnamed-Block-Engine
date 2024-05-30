using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;
using Shapes;

namespace BlockGameRenderer
{
    
    class Program
    {
        private static int width = 1280, height = 720;

        public static string[] FragmentShaders =
        {
            @"#version 130

uniform sampler2D texture;

in vec3 color;

out vec4 fragment;

void main(void)
{
    fragment = vec4(color * texture2D(texture, gl_PointCoord).xyz, 1);
}
",
        };
        public static string[] vertexShaders = { 
@"
#version 130

in vec3 vertexPosition;
in vec3 vertexColor;

out vec3 color;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;
uniform bool static_colors;

void main(void)
{
    color = (static_colors ? vertexColor : mix(vec3(0, 0, 1), vec3(0.7, 0, 1), clamp(vertexPosition.y / 2, 0, 1)));

    gl_PointSize = clamp(10 + vertexPosition.y * 5, 0, 10);

    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition.xyz, 1);
}
", };

        public static float fov = 0.45f;
        public static int SelectedVertexShader = 0;
        public static int SelectedFragmentShader = 0;
        public static float MaxRenderDistance = 1000f;
        public static float MinRenderDistance = 0.1f;


        private static ShaderProgram shaderProgram;
        static void Main(string[] args)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Game");

            Glut.glutIdleFunc(onRenderFrame);
            Glut.glutDisplayFunc(onDisplay);
            shaderProgram = new ShaderProgram(vertexShaders[SelectedVertexShader], FragmentShaders[SelectedFragmentShader]);
            shaderProgram.Use(); // init shader program
            shaderProgram["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(fov, (float)width / height, MinRenderDistance, MaxRenderDistance)); // basic world space
            shaderProgram["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY)); //basic camera control 10 units away from origin
            Glut.glutMainLoop(); // Rendering using x86 version of Glut, might become a bottleneck in the future
        }

        private static void onDisplay()
        {

        }

        private static void onRenderFrame()
        {
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shaderProgram.Use();

            uint vertexPositionIndex = (uint)Gl.GetAttribLocation(shaderProgram.ProgramID, "vertexPosition");
            Gl.EnableVertexAttribArray(vertexPositionIndex);


            Glut.glutSwapBuffers();
        }
    }
}
