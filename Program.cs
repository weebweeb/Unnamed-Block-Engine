using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;
using Shapes;
using WorldManager;
using System.Collections.Generic;
using Tick;
using Animations;
using Blocks;
using LowLevelInput.Hooks;
using LowLevelInput.Converters;

 
namespace BlockGameRenderer
{



    class Program
    {
        public static int width = 1280, height = 720;

        public static string[] FragmentShaders =
        {
            @"#version 130
in vec2 uv;
uniform float opacity;
uniform int num_lights = 3;
uniform vec3 lighting_direction[200];
uniform vec3 lighting_colors[200];
uniform sampler2D texture;
in vec3 normals;
out vec4 fragment;

vec4 tex = texture2D(texture, uv);


void main(void)
{
    float diffuse = 0;
    vec3 ambient = vec3(0.3, 0.3, 0.3);
    vec3 diffusecolor = vec3(0.0);
     for (int i = 0; i < num_lights; i++) {
        diffuse = max(dot(normalize(normals), normalize(lighting_direction[i])), 0.0);
        
        diffusecolor += diffuse * lighting_colors[i] * vec3(4, 4, 4);
    }
    

    vec3 finalColor = tex.rgb * (ambient + diffusecolor);

    fragment = vec4(finalColor, opacity*tex.a);
    
}
",@"#version 130

out vec4 fragment;

void main(void)
{
    fragment = vec4(1, 1, 1, 1);
}
",
@"#version 130
in vec2 uv;
uniform float opacity;
uniform int num_lights = 3;
uniform vec3 lighting_direction[200];
uniform sampler2D texture;
in vec3 normals;
out vec4 fragment;

vec4 tex = texture2D(texture, uv);


void main(void)
{
    float diffuse = 0;
     for (int i = 0; i < num_lights; i++) {
        diffuse += max(dot(normals, lighting_direction[i]), 0.0);
    }
    float ambient = 0.3;
    float lighting = max(diffuse, ambient);
    fragment = vec4(tex.xyz * lighting, opacity*tex.a);
    
}
"};
        public static string[] vertexShaders = {
@"
#version 130

in vec3 vertexPosition;
in vec2 vertexUV;
in vec3 vertexNormals;


out vec2 uv;
out vec3 normals;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    uv = vertexUV;
    normals = normalize((model_matrix * vec4(floor(vertexNormals),0)).xyz);
    
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
", @"
#version 130

in vec3 vertexPosition;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
"};


        public static float fov = 0.7f;
        public static int SelectedVertexShader = 0; //0 - full render, //1 - whiteworld 
        public static int SelectedFragmentShader = 2; //0 - full render, //1 - whiteworld //2 b/w shadows
        public static float MaxRenderDistance = 1000f;
        public static float MinRenderDistance = 0.1f;
        public static int numLights = 100; // max number of lights
        public static int Worldsize = 2000;
        public static bool Fullscreen = false;

        public static Camera camera;
        public static List<Vector3> Lights;
        public static List<Vector3> Colors;
        private static ShaderProgram shaderProgram;
        private static World Map;
        private static Block ExampleSquare;
        private static Block ExampleSquare2;
        private static Vector3 ExamplePosition;
        private static Time GameTime;
        private static InputManager inputManager;
        public static UpdateService updateService;
        




        private static void ExampleAnimationFunction(Shape Subject)
        {
            
            foreach(var Polygon in Subject.ConstitutentGeometry)
            {
                //Vector3 oldposition = new Vector3(Polygon.Position.X, Polygon.Position.Y, Polygon.Position.Z);
                //Polygon.Position = new Vector3(0, 0, 0);
           
                Polygon.Orientation = new Vector3(Polygon.Orientation.X + 0.01f, Polygon.Orientation.Y + 0.01f, Polygon.Orientation.Z);
        
                Polygon.Rotation = Shape.CreateRotationMatrix(new Vector3(1, 0, 0), Polygon.Orientation.X) * Shape.CreateRotationMatrix(new Vector3(0, 1, 0), Polygon.Orientation.Y);
                //Polygon.Position = oldposition;
            }
        }
        public static AnimationHandler Animator;
        public static Animation AnimationObject;

  

        public static List<GameEntity> VisibleEntities;

        static void Main(string[] args)
        {
            Glut.glutInit();

            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_RGB | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("Game");
            inputManager = new InputManager();



            ExamplePosition = new Vector3(1.5f, 0, 0);
            Map = new World(new Vector3(-(Worldsize / 2), -(Worldsize / 2), -(Worldsize / 2)), new Vector3((Worldsize / 2), (Worldsize / 2), (Worldsize / 2)));
            GameTime = new Time();  // generic thread for generic game jobs

            ExampleSquare = new OakLog(ExamplePosition, new Vector3(1, 1, 1), Shape.CreateRotationMatrix(Vector3.UnitY, 0.1f));
            ExampleSquare2 = new Grass(new Vector3(-8f, 0, 0), new Vector3(1, 1, 1), Shape.CreateRotationMatrix(Vector3.UnitY, 0.1f));

            AnimationObject = new Animation
            {
                AnimationFunctions = new GenericAnimationFunction[] { new GenericAnimationFunction(ExampleAnimationFunction) },
                Loopable = true,
                Subject = ExampleSquare.Geometry
            };

            camera = new Camera(new Vector3(0, 0, 10), 0f, 30f);


            Map.Insert(ExampleSquare.Geometry.Position, ExampleSquare.Geometry.Size, ExampleSquare);
            Map.Insert(ExampleSquare2.Geometry.Position, ExampleSquare2.Geometry.Size, ExampleSquare2);

            Animator = new AnimationHandler();
            Animator.QueueAnimation(AnimationObject);
            int RoundedRenderDistance = (int)MaxRenderDistance;
            updateService = new UpdateService(Map, RoundedRenderDistance, 5000, new Vector3(0, 0, 0));
            GameTime.addRunTimeFunction(new GenericFunction(UpdateCamera));



            Gl.DepthFunc(DepthFunction.Less);
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);

            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            Glut.glutReshapeFunc(OnReshape);
            inputManager.OnKeyboardEvent += Input.OnKeyboardDown;
            Glut.glutPassiveMotionFunc(camera.Interpolate2D);
            //Glut.glutKeyboardFunc(Input.OnKeyboardDown);
            Glut.glutIdleFunc(onRenderFrame);
            Glut.glutDisplayFunc(onDisplay);
            Glut.glutCloseFunc(onClose);
            Lights = new List<Vector3>() { };
            Colors = new List<Vector3>() { };
            shaderProgram = new ShaderProgram(vertexShaders[SelectedVertexShader], FragmentShaders[SelectedFragmentShader]);
            shaderProgram.Use(); 
            shaderProgram["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(fov, (float)width / height, MinRenderDistance, MaxRenderDistance));
            inputManager.Initialize();

            Glut.glutMainLoop(); // Rendering using x86 version of Glut, might become a bottleneck in the future
        }



        private static void UpdateCamera()
        {

            // basic movement tied to the main game thread
            // ill fuckin change it later please god im so tired of working on this camera
            if (Input.BackwardMomentum) camera.MoveRelative(false, true, false ,false, false, false);
            if (Input.ForwardMomentum) camera.MoveRelative(true, false, false, false, false, false);
            if (Input.LeftMomentum) camera.MoveRelative(false, false, true, false, false, false);
            if (Input.RightMomentum) camera.MoveRelative(false, false, false, true, false ,false);
            if (Input.UpMomentum) camera.MoveRelative(false, false, false, false, true, false);
            if (Input.DownMomentum) camera.MoveRelative(false, false, false, false, false, true);
        }

        
        private static void OnReshape(int width, int height) {
            Program.width = width;
            Program.height = height;

            shaderProgram.Use();
            shaderProgram["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(fov, (float)width / height, MinRenderDistance, MaxRenderDistance));

        }
        private static void UpdateLighting(int num, List<Vector3> lightingdirections, List<Vector3> Lightcolor)
        {
            int lightingDirectionsLocation = Gl.GetUniformLocation(shaderProgram.ProgramID, "lighting_direction");
            int lightingColorLocation = Gl.GetUniformLocation(shaderProgram.ProgramID, "lighting_colors");
            // Convert List<Vector3> to float array
            float[] lightDirArray = new float[lightingdirections.Count * 3];
            for (int i = 0; i < lightingdirections.Count; i++)
            {
                lightDirArray[i * 3] = lightingdirections[i].X;
                lightDirArray[i * 3 + 1] = lightingdirections[i].Y;
                lightDirArray[i * 3 + 2] = lightingdirections[i].Z;
            }

            float[] lightcolorArray = new float[Lightcolor.Count * 3];
            for (int i = 0; i < lightingdirections.Count; i++)
            {
                lightcolorArray[i * 3] = Lightcolor[i].X;
                lightcolorArray[i * 3 + 1] = Lightcolor[i].Y;
                lightcolorArray[i * 3 + 2] = Lightcolor[i].Z;
            }
            shaderProgram["num_lights"].SetValue(num);

         Gl.Uniform3fv(lightingDirectionsLocation, num, lightDirArray);
         Gl.Uniform3fv(lightingColorLocation, num, lightcolorArray);


        }

        private static void Deload(Shape Target)
        {
            Target.Color.Dispose();
            Target.Elements.Dispose();
            Target.Vertices.Dispose();
            Target.Normals.Dispose();
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

                Ent.Block.texture.Dispose();
                Ent.Block.textureUVs.Dispose();
                foreach(var toClose in Ent.Block.Geometry.ConstitutentGeometry)
                {
                    Deload(toClose);
                }
            }
            shaderProgram.DisposeChildren = true;
            shaderProgram.Dispose();
            inputManager.Dispose();
        }

        private static void onDisplay()
        {

        }

        private static void onRenderFrame()
        {
            
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            UpdateLighting(Lights.Count, Lights, Colors);
            
            shaderProgram.Use();
            Lights.Clear();
            Colors.Clear();
            uint vertexPositionIndex = (uint)Gl.GetAttribLocation(shaderProgram.ProgramID, "vertexPosition");
            Gl.EnableVertexAttribArray(vertexPositionIndex);
            shaderProgram["view_matrix"].SetValue(camera.ViewMatrix);




            foreach (var Ent in updateService.ReturnUpdateList())
            {

                Gl.BindTexture(Ent.Block.texture);
                if (Ent.Block.Light > 0)
                {
                    Lights.Add(Vector3.Normalize(Ent.Position));
                    Colors.Add(Vector3.Normalize(Ent.Block.LightColor));
                }
                
                foreach (var GeometryElements in Ent.Block.Geometry.ConstitutentGeometry)
                {
                    Gl.BindBufferToShaderAttribute(GeometryElements.Normals, shaderProgram, "vertexNormals");
                    Gl.BindBufferToShaderAttribute(GeometryElements.Vertices, shaderProgram, "vertexPosition");
                    Gl.BindBufferToShaderAttribute(Ent.Block.textureUVs, shaderProgram, "vertexUV");

                    int location = Gl.GetUniformLocation(shaderProgram.ProgramID, "opacity");
                    Gl.Uniform1f(location, Ent.Block.Transparency);
                    

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
