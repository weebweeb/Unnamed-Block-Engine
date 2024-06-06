using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;
using Shapes;
using WorldManager;
using System.Collections.Generic;
using Tick;
using Animations;
using System.Threading;

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
        private static int Worldsize = 2000;

        private static void ExampleAnimationFunction(Shape Subject)
        {
            
            foreach(var Polygon in Subject.ConstitutentGeometry)
            {
                //Vector3 oldposition = new Vector3(Polygon.Position.X, Polygon.Position.Y, Polygon.Position.Z);
                //Polygon.Position = new Vector3(0, 0, 0);
                Polygon.Orientation = new Vector3(Polygon.Orientation.X, Polygon.Orientation.Y + 0.001f, Polygon.Orientation.Z);
                Polygon.Rotation = Shape.CreateRotationMatrix(new Vector3(0, 1, 0), Polygon.Orientation.Y);
                //Polygon.Position = oldposition;
            }
        }
        public static AnimationHandler Animator;
        public static Animation AnimationObject;

        public static Animation AnimationObjectTwo;

        public static List<GameEntity> VisibleEntities;
        
        static void Main(string[] args)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Game");
            
            ExamplePosition = new Vector3(1.5f, 0, 0);
            Map = new World(new Vector3(-(Worldsize/2),-(Worldsize / 2), -(Worldsize / 2)), new Vector3((Worldsize / 2), (Worldsize / 2), (Worldsize / 2)));
            GameTime = new Time();
            ExampleColor = new VBO<Vector3>(new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0) });
            ExampleTriangle = new AbstractTriangle(new Vector3(-1f, 0, 0), new Vector3(1,1,1), ExampleColor, Shape.CreateRotationMatrix(Vector3.UnitY, 0f));
            ExampleSquare = new Square(ExamplePosition, new Vector3(1, 1, 1), ExampleColor, Shape.CreateRotationMatrix(Vector3.UnitY, 0.1f));
            AnimationObject = new Animation
            {
                AnimationFunctions = new GenericAnimationFunction[] { new GenericAnimationFunction(ExampleAnimationFunction) },
                Loopable = true,
                Subject = ExampleSquare
            };

            AnimationObjectTwo = new Animation
            {
                AnimationFunctions = new GenericAnimationFunction[] { new GenericAnimationFunction(ExampleAnimationFunction) },
                Loopable = true,
                Subject = ExampleTriangle
            };
            Map.Insert(ExampleTriangle.Position, ExampleTriangle.Size, ExampleTriangle.ConstitutentGeometry);
            Map.Insert(ExampleSquare.Position, ExampleSquare.Size, ExampleSquare.ConstitutentGeometry);
            Animator = new AnimationHandler();
            Animator.QueueAnimation(AnimationObject);
            Animator.QueueAnimation(AnimationObjectTwo);
            VisibleEntities = Map.Entities.Retrieve(new BoundingBox{ 
               Min = new Vector3(-10,-10,-10),
               Max = new Vector3(10,10,10)
            });
            


            Glut.glutIdleFunc(onRenderFrame);
            Glut.glutDisplayFunc(onDisplay);
            Glut.glutCloseFunc(onClose);
            shaderProgram = new ShaderProgram(vertexShaders[SelectedVertexShader], FragmentShaders[SelectedFragmentShader]);
            shaderProgram.Use(); 
            shaderProgram["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(fov, (float)width / height, MinRenderDistance, MaxRenderDistance)); 
            shaderProgram["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, new Vector3(0,1,0))); //basic camera control 10 units away from origin
           // shaderProgram["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(0, 0, 0)));
            Glut.glutMainLoop(); // Rendering using x86 version of Glut, might become a bottleneck in the future
        }

        private static void Deload(Shape Target)
        {
            Target.Color.Dispose();
            Target.Elements.Dispose();
            Target.Vertices.Dispose();
        }

        private static void onClose()
        {
            VisibleEntities = Map.Entities.Retrieve(new BoundingBox
            {
                Min = new Vector3(-(Worldsize / 2), -(Worldsize / 2), -(Worldsize / 2)),
                Max = new Vector3((Worldsize / 2), (Worldsize / 2), (Worldsize / 2))
            });

            foreach (var Ent in VisibleEntities)
            {
                foreach(var toClose in Ent.Geometry)
                {
                    Deload(toClose);
                }
            }
            shaderProgram.DisposeChildren = true;
            shaderProgram.Dispose();
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
            foreach (var Ent in VisibleEntities)
            {
                
                foreach (var GeometryElements in Ent.Geometry)
                {

                    Gl.BindBufferToShaderAttribute(GeometryElements.Vertices, shaderProgram, "vertexPosition");
                    Gl.BindBufferToShaderAttribute(GeometryElements.Color, shaderProgram, "vertexColor");
                    Gl.BindBuffer(GeometryElements.Elements);

                    Matrix4 model = Matrix4.Identity;

                    model = model* Matrix4.CreateTranslation(-GeometryElements.Position);

                    model = model * GeometryElements.Rotation;
                    model = model * Matrix4.CreateTranslation(GeometryElements.Position);

                    shaderProgram["model_matrix"].SetValue(model);


                    Gl.DrawElements(GeometryElements.Beginmode, GeometryElements.Elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
                    
                }
            }

            Glut.glutSwapBuffers();
        }
    }
}
