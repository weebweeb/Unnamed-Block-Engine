using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;
using System.Drawing;
using Shapes;
using WorldManager;
using System.Collections.Generic;
using Tick;
using Animations;
using Blocks;
using LowLevelInput.Hooks;
using LowLevelInput.Converters;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using Skybox;

namespace BlockGameRenderer
{

   

    class Program
    {
        public static int width = 1280, height = 720;

        public static float fov = 0.7f;
        public static string SelectedVertexShader = "Primary.vert";
        public static string SelectedFragmentShader = "Primary.frag";
        public static string SkyboxVertexShader = "Skybox.vert";
        public static string SkyboxFragmentShader = "Skybox.frag";

        public static float MaxRenderDistance = 1000f;
        public static float MinRenderDistance = 0.1f;
        public static int numLights = 100; // max number of lights
        public static int Worldsize = 2000;
        public static bool Fullscreen = false;
        public static Vector3 ambient_light = new Vector3(0.5f,0.5f,0.5f);

        public static Camera camera;
        public static List<Vector4> Lights;
        public static List<Vector4> Colors;
        public static List<Vector4> Brightness;
        private static uint ubo;
        public static uint CubeMapTexture;
        private static ShaderProgram shaderProgram;
        private static ShaderProgram skyboxShader;
        private static World Map;
        private static Block ExampleSquare;
        private static Block ExampleSquare2;
        private static Vector3 ExamplePosition;
        private static Time GameTime;
        private static InputManager inputManager;
        public static UpdateService updateService;
        public static Skybox.Skybox LocalSkybox;
        public static AnimationHandler Animator;
        public static Animation AnimationObject;
        public static List<GameEntity> VisibleEntities;

        public static String LoadShader(string path)
        {
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }


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
        


        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct LightData
        {
            public int num_lights; // 4 bytes
            private Vector3 padding; //12 bytes

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public Vector4[] lighting_directions; // Using Vector4 to ensure alignment

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public Vector4[] lighting_colors; // Using Vector4 to ensure alignment

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public Vector4[] lighting_brightness; // Using Vector4 to ensure alignment

            public Vector3 ambient; // 12 bytes
            private float padding3;  // 4 bytes to align the struct size to a multiple of 16 bytes

        }

        private static void UpdateLightDataUBO()
        {

            Vector4[] VectorLights = Lights.ToArray();

            LightData data = new LightData
            {
                num_lights = VectorLights.Length,
                lighting_directions = new Vector4[100],
                lighting_colors = new Vector4[100],
                lighting_brightness = new Vector4[100],
                ambient = ambient_light,
            };

            

            Array.Copy(VectorLights, data.lighting_directions, data.num_lights);
            Array.Copy(Colors.ToArray(), data.lighting_colors, data.num_lights);
            Array.Copy(Brightness.ToArray(), data.lighting_brightness, data.num_lights);
            


            Gl.BindBuffer(BufferTarget.UniformBuffer, ubo);


            int uboSize = Marshal.SizeOf(typeof(LightData));
            IntPtr ptr = Marshal.AllocHGlobal(uboSize);
            Marshal.StructureToPtr(data, ptr, false);
            Gl.BufferData(BufferTarget.UniformBuffer,(IntPtr)uboSize, ptr, BufferUsageHint.DynamicDraw);

            Marshal.FreeHGlobal(ptr);
            Gl.BindBuffer(BufferTarget.UniformBuffer, 0);

        }


        private static void InitializeUBO(uint uboLocation, string Index)
        {
            ubo = Gl.GenBuffer();

            
            Gl.BindBuffer(BufferTarget.UniformBuffer, ubo);
            uint blockIndex = Gl.GetUniformBlockIndex(shaderProgram.ProgramID, Index);

            //Gl.UniformBlockBinding(shaderProgram.ProgramID, blockIndex, (uint)11);

            int uboSize = Marshal.SizeOf(typeof(LightData));
            Gl.BufferData(BufferTarget.UniformBuffer, (IntPtr)uboSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            Gl.BindBufferBase(BufferTarget.UniformBuffer, 11, ubo);

            UpdateLightDataUBO();

            Gl.BindBuffer(BufferTarget.UniformBuffer, 0);

        }

        public static void InitSkybox()
        {
            CubeMapTexture = Gl.GenTexture();

            LocalSkybox = new Skybox.Skybox
            {
                SkyboxTexturePaths = new string[]{
                    "BlueSkyDayRight.jpg",
                    "BlueSkyDayLeft.jpg",
                    "BlueSkyDayTop.jpg",
                    "BlueSkyDayBottom.jpg",
                    "BlueSkyDayFront.jpg",
                    "BlueSkyDayBack.jpg"
                }

            };

            Gl.BindTexture(TextureTarget.TextureCubeMap, CubeMapTexture);
            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, TextureParameter.Linear);
            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, TextureParameter.Linear);

            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, TextureParameter.ClampToEdge);
            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, TextureParameter.ClampToEdge);
            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, TextureParameter.ClampToEdge);

            for (int i = 0; i < LocalSkybox.SkyboxTexturePaths.Length; i++)
            {
                Bitmap bitmap = new Bitmap(LocalSkybox.SkyboxTexturePaths[i]);
                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                if (data != null)
                {

                    Gl.TexImage2D(
                        TextureTarget.TextureCubeMapPositiveX + i,
                        0,
                        PixelInternalFormat.Rgb,
                        bitmap.Width,
                        bitmap.Height,
                        0,
                        OpenGL.PixelFormat.Rgb,
                        PixelType.UnsignedByte,
                        data.Scan0);

                    bitmap.UnlockBits(data);
                    bitmap.Dispose();
                }
                else { Console.WriteLine("Failed to load skybox texture!"); };
            }
        }

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

            camera = new Camera(new Vector3(0, 0, 10), 0f, 20f);


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
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Front);
            Gl.Enable(EnableCap.Texture2D);
            Gl.Enable(EnableCap.TextureCubeMap);
           


            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            Glut.glutReshapeFunc(OnReshape);
            inputManager.OnKeyboardEvent += Input.OnKeyboardDown;
            Glut.glutPassiveMotionFunc(camera.Interpolate2D);
            //Glut.glutKeyboardFunc(Input.OnKeyboardDown);
            Glut.glutIdleFunc(onRenderFrame);
            Glut.glutDisplayFunc(onDisplay);
            Glut.glutCloseFunc(onClose);
            InitSkybox();
            Lights = new List<Vector4>(100) { };
            Colors = new List<Vector4>(100) { };
            Brightness = new List<Vector4>(100) { };
            shaderProgram = new ShaderProgram(LoadShader(SelectedVertexShader), LoadShader(SelectedFragmentShader));
            skyboxShader = new ShaderProgram(LoadShader(SkyboxVertexShader), LoadShader(SkyboxVertexShader));
            InitializeUBO(shaderProgram.ProgramID, "lightdata");
            shaderProgram.Use(); 
            shaderProgram["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(fov, (float)width / height, MinRenderDistance, MaxRenderDistance));
            skyboxShader.Use();
            Gl.Uniform1i(Gl.GetUniformLocation(skyboxShader.ProgramID, "skybox"), 0);
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
            shaderProgram.Use();
            UpdateLightDataUBO();
            Lights.Clear();
            Colors.Clear();
            Brightness.Clear();



            uint vertexPositionIndex = (uint)Gl.GetAttribLocation(shaderProgram.ProgramID, "vertexPosition");
            Gl.EnableVertexAttribArray(vertexPositionIndex);
            shaderProgram["view_matrix"].SetValue(camera.ViewMatrix);


            




            foreach (var Ent in updateService.ReturnUpdateList())
            {

                Gl.BindTexture(Ent.Block.texture);
                if (Ent.Block.Light > 0)
                {
                    Lights.Add(new Vector4(Ent.Position.X, Ent.Position.Y, Ent.Position.Z, 0));
                    Colors.Add(new Vector4(Ent.Block.LightColor.X, Ent.Block.LightColor.Y, Ent.Block.LightColor.Z, 0));
                    Brightness.Add(new Vector4(Ent.Block.Light, 0 ,0, 0));
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
            Gl.BindVertexArray(0);
            Gl.DepthFunc(DepthFunction.Equal);
            skyboxShader.Use();
            // redefining some of the matrices so we can render a skybox without issues
            // quick and dirty
            skyboxShader["view_matrix"].SetValue(camera.SimplifiedViewMatrix);
            skyboxShader["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.2f, (float)width / height, 0.1f, 100f));
            Gl.BindBufferToShaderAttribute(LocalSkybox.Vertices, skyboxShader, "aPos");
            Gl.ActiveTexture(0);
            Gl.BindTexture(TextureTarget.TextureCubeMap, CubeMapTexture);
            Gl.DrawElements(BeginMode.Triangles, 36, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindVertexArray(0);

            Gl.DepthFunc(DepthFunction.Less);


            Glut.glutSwapBuffers();

        }
    }
}
