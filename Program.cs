using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;
using Shapes;
using WorldManager;
using System.Collections.Generic;
using Tick;
using Animations;

namespace BlockGameRenderer
{



    class Program
    {
        private static int width = 1280, height = 720;

        public static string[] FragmentShaders =
        {
            @"#version 130

in vec3 color;

void main(void)
{
    gl_FragColor = vec4(color,1);
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

void main(void)
{
    color = vertexColor;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
", };
        

        public static float fov = 0.45f;
        public static int SelectedVertexShader = 0;
        public static int SelectedFragmentShader = 0;
        public static float MaxRenderDistance = 1000f;
        public static float MinRenderDistance = 0.1f;


        private static ShaderProgram shaderProgram;
        private static World Map;
        private static AbstractTriangle ExampleTriangle;
        private static Square ExampleSquare;
        private static VBO<Vector3> ExampleColor;
        private static Vector3 ExamplePosition;
        private static Time GameTime;
        private static long ElapsedTime;

        public static List<GameEntity> VisibleEntities;
        
        static void Main(string[] args)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Game");
            ExamplePosition = new Vector3(1.5f, 0, 0);
            Map = new World(new Vector3(-1000,-1000, -1000), new Vector3(1000,1000,1000));
            GameTime = new Time();
            ExampleColor = new VBO<Vector3>(new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0) });
            ExampleTriangle = new AbstractTriangle(new Vector3(-1f, 0, 0), new Vector3(1,1,1), ExampleColor, Shape.CreateRotationMatrix(Vector3.UnitY, 0f));
            ExampleSquare = new Square(ExamplePosition, new Vector3(1, 1, 1), ExampleColor, Shape.CreateRotationMatrix(Vector3.UnitY, 0.1f));
            Map.Insert(ExampleTriangle.Position, ExampleTriangle.Size, ExampleTriangle.ConstitutentGeometry);
            Map.Insert(ExampleSquare.Position, ExampleSquare.Size, ExampleSquare.ConstitutentGeometry);
            VisibleEntities = Map.Entities.Retrieve(new BoundingBox{ 
               Min = new Vector3(-10,-10,-10),
               Max = new Vector3(10,10,10)
            });
            


            Glut.glutIdleFunc(onRenderFrame);
            Glut.glutDisplayFunc(onDisplay);
            shaderProgram = new ShaderProgram(vertexShaders[SelectedVertexShader], FragmentShaders[SelectedFragmentShader]);
            shaderProgram.Use(); 
            shaderProgram["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(fov, (float)width / height, MinRenderDistance, MaxRenderDistance)); 
            shaderProgram["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, new Vector3(0,1,0))); //basic camera control 10 units away from origin
            shaderProgram["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(0, 0, 0)));
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
            ElapsedTime = GameTime.GetTicksSinceLastCall();
            uint vertexPositionIndex = (uint)Gl.GetAttribLocation(shaderProgram.ProgramID, "vertexPosition");
            Gl.EnableVertexAttribArray(vertexPositionIndex);
            foreach (var Ent in VisibleEntities)
            {
                
                foreach (var GeometryElements in Ent.Geometry)
                {
                    shaderProgram["model_matrix"].SetValue(GeometryElements.Rotation * Matrix4.CreateTranslation(GeometryElements.Position));
                    Gl.BindBufferToShaderAttribute(GeometryElements.Vertices, shaderProgram, "vertexPosition");
                    Gl.BindBufferToShaderAttribute(GeometryElements.Color, shaderProgram, "vertexColor");
                    Gl.BindBuffer(GeometryElements.Elements);
                    Gl.DrawElements(GeometryElements.Beginmode, GeometryElements.Elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
                }
            }

            Glut.glutSwapBuffers();
        }
    }
}
