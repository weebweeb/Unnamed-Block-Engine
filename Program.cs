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
using StbImageSharp;

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
        private static uint CubeMapTexture = 0;
        private static uint Vao;
        private static uint BlockTextureIndex;
        private static List<Matrix4> ObjectMatrices;
        private static List<Vector4> ActiveBlockTextures;
        private static uint MatriceUBO;
        private static string[] BlockTexturePaths =
        {
            "grass.png",
            "stone.png",
            "water.png",
            "dirt.png",
            "oakleaves.png",
            "oaklog.png",
        };

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
        public static uint skyboxVAO, skyboxVBO, skyboxEBO;
        public static int InstanceCount = 0;


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
            private float padding3;  // 4 bytes to align the struct size

        }


        [StructLayout(LayoutKind.Sequential)]
        public struct MatricesBlock
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100000)]
            public Matrix4[] Matrices;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100000)]
            public Vector4[] TextureIDs;
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

        private static void UpdateMatrixDataUBO()
        {

            Matrix4[] MatriceArray = ObjectMatrices.ToArray();
            Vector4[] TextureIndexArray = ActiveBlockTextures.ToArray();

            MatricesBlock MatriceStructData = new MatricesBlock
            {
                Matrices = new Matrix4[100000],
                TextureIDs = new Vector4[100000]
            };

            Array.Copy(MatriceArray, MatriceStructData.Matrices, InstanceCount);
            Array.Copy(TextureIndexArray, MatriceStructData.TextureIDs, InstanceCount);


            Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, MatriceUBO);


            int uboSize = Marshal.SizeOf(MatriceStructData);
            IntPtr ptr = Marshal.AllocHGlobal(uboSize);
            Marshal.StructureToPtr(MatriceStructData, ptr, false);
            Gl.BufferData(BufferTarget.ShaderStorageBuffer, (IntPtr)uboSize, ptr, BufferUsageHint.DynamicDraw);

            Marshal.FreeHGlobal(ptr);
            Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        }

        private static byte[] Load2DTexture(string path)
        {
            byte[] imageData = File.ReadAllBytes(path);
            return imageData;
        }

        private static List<byte[]> EnumerateTexturePaths(string[] texturepaths)
        {
            List<byte[]> TextureList = new List<byte[]>();

            for(int i = 0; i < texturepaths.Length; i++)
            {
                byte[] data = Load2DTexture(texturepaths[i]);
                TextureList.Add(data);
            }

            return TextureList;
        }


        private static void InitializeBufferObject<T>(BufferTarget buffertarget, uint uboptr, uint layoutIndex)
        {


            Gl.BindBuffer(buffertarget, uboptr);

            int uboSize = Marshal.SizeOf(typeof(T));
            Gl.BufferData(buffertarget, (IntPtr)uboSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            Gl.BindBufferBase(buffertarget, layoutIndex, uboptr);



            Gl.BindBuffer(buffertarget, 0);

        }

        public static void InitSkybox()
        {


            LocalSkybox = new Skybox.Skybox
            {
                SkyboxTexturePaths = new string[]{
                    "BlueSkyDayRight.png",
                    "BlueSkyDayLeft.png",
                    "BlueSkyDayTop.png",
                    "BlueSkyDayBottom.png",
                    "BlueSkyDayFront.png",
                    "BlueSkyDayBack.png"
                }

            };
            // doing it the old fashioned way
            skyboxVAO = Gl.GenVertexArray();
            skyboxVBO = Gl.GenBuffer();
            skyboxEBO = Gl.GenBuffer();
            Gl.BindVertexArray(skyboxVAO);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, skyboxVBO);
            Gl.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * LocalSkybox.Vertices.Length, LocalSkybox.Vertices, BufferUsageHint.StaticDraw);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, skyboxEBO);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * LocalSkybox.Elements.Length, LocalSkybox.Elements, BufferUsageHint.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), IntPtr.Zero);
            Gl.EnableVertexAttribArray(0);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
            Gl.BindVertexArray(0);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, 0);



            Gl.ActiveTexture(3);

            CubeMapTexture = Gl.GenTexture();

           


            Gl.BindTexture(TextureTarget.TextureCubeMap, CubeMapTexture);

            

            for (int i = 0; i < LocalSkybox.SkyboxTexturePaths.Length; i++)
            {
                byte[] imageData = File.ReadAllBytes(LocalSkybox.SkyboxTexturePaths[i]);
                ImageResult data = ImageResult.FromMemory(imageData, ColorComponents.RedGreenBlueAlpha);


                if (data != null)
                {
                    unsafe
                    {
                        fixed (byte* dataPtr = data.Data)
                        {
                            IntPtr intPtr = new IntPtr(dataPtr);

                            Gl.TexImage2D(
                            TextureTarget.TextureCubeMapPositiveX + i,
                            0,
                            PixelInternalFormat.Rgba,
                            data.Width,
                            data.Height,
                            0,
                            OpenGL.PixelFormat.Rgba,
                            PixelType.UnsignedByte,
                            intPtr);

                        }
                    }
                   


                }
                else { Console.WriteLine("Failed to load skybox texture!"); };


            }

            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, TextureParameter.Linear);
            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, TextureParameter.Linear);
            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, TextureParameter.ClampToEdge);
            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, TextureParameter.ClampToEdge);
            Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, TextureParameter.ClampToEdge);
            Gl.BindTexture(TextureTarget.TextureCubeMap, 0);
            Gl.ActiveTexture(0);

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

            ExampleSquare = new OakLeaves(ExamplePosition, new Vector3(1, 1, 1), Shape.CreateRotationMatrix(Vector3.UnitY, 0.1f));
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
            Gl.FrontFace(FrontFaceDirection.Ccw);
            Gl.Enable(EnableCap.TextureCubeMapSeamless);

            Gl.Viewport(0, 0, width, height);




            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            Gl.ClearColor(1f, 1f, 1f, 1.0f);
            Glut.glutReshapeFunc(OnReshape);
            inputManager.OnKeyboardEvent += Input.OnKeyboardDown;
            Glut.glutPassiveMotionFunc(camera.Interpolate2D);
            //Glut.glutKeyboardFunc(Input.OnKeyboardDown);
            Glut.glutIdleFunc(onRenderFrame);
            Glut.glutDisplayFunc(onDisplay);
            Glut.glutCloseFunc(onClose);
            Lights = new List<Vector4>(100) { };
            Colors = new List<Vector4>(100) { };
            Brightness = new List<Vector4>(100) { };
            ObjectMatrices = new List<Matrix4>(100000) { };
            ActiveBlockTextures = new List<Vector4>(100000) { };
            shaderProgram = new ShaderProgram(LoadShader(SelectedVertexShader), LoadShader(SelectedFragmentShader));
            skyboxShader = new ShaderProgram(LoadShader(SkyboxVertexShader), LoadShader(SkyboxFragmentShader));
            ubo = Gl.GenBuffer();
            MatriceUBO = Gl.GenBuffer();
            Vao = Gl.GenVertexArray();
            BlockTextureIndex = CreateTextureArray(160, 16, EnumerateTexturePaths(BlockTexturePaths));
          
            InitializeBufferObject<LightData>(BufferTarget.UniformBuffer, ubo, 11);
            InitializeBufferObject<MatricesBlock>(BufferTarget.ShaderStorageBuffer, MatriceUBO, 4);
            UpdateLightDataUBO();
            UpdateMatrixDataUBO();
            shaderProgram.Use(); 
            shaderProgram["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(fov, (float)width / height, MinRenderDistance, MaxRenderDistance));
            
            

            skyboxShader.Use();
            Gl.Uniform1i(Gl.GetUniformLocation(skyboxShader.ProgramID, "skybox"), 3);
            InitSkybox();

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

     
        public static void DeloadMap()
        {
            VisibleEntities = Map.Entities.Retrieve(new BoundingBox
            {
                Min = new Vector3(-(Worldsize / 2), -(Worldsize / 2), -(Worldsize / 2)),
                Max = new Vector3((Worldsize / 2), (Worldsize / 2), (Worldsize / 2))
            });

            foreach (var Ent in VisibleEntities)
            {
                Ent.Block.textureUVs.Dispose();
                foreach (var toClose in Ent.Block.Geometry.ConstitutentGeometry)
                {
                    Deload(toClose);
                }
            }

        }
        private static uint CreateTextureArray(int textureWidth, int textureHeight, List<byte[]> textureDataList)
        {
            uint textureArray;
            textureArray = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2DArray, textureArray);
            Gl.TexStorage3D(TextureTarget.Texture2DArray, 1, SizedInternalFormat.Rgba8, textureWidth, textureHeight, textureDataList.Count);

            for (int i = 0; i < textureDataList.Count; i++)
            {

                byte[] textureData = textureDataList[i];
                ImageResult data = ImageResult.FromMemory(textureData, ColorComponents.RedGreenBlueAlpha);

                unsafe
                {
                    fixed (byte* dataPtr = data.Data)
                    {
                        IntPtr intPtr = new IntPtr(dataPtr);
                        Gl.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, data.Width, data.Height, 1, OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, intPtr);
                    }
                }
            }

            // Set texture parameters
            Gl.TexParameteri(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, TextureParameter.Nearest);
            Gl.TexParameteri(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, TextureParameter.Nearest);
            Gl.TexParameteri(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, TextureParameter.Repeat);
            Gl.TexParameteri(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, TextureParameter.Repeat);

            // Unbind the texture array
            Gl.BindTexture(TextureTarget.Texture2DArray, 0);

            // Return the texture array ID
            return textureArray;
        }



        private static void onClose()
        {
            DeloadMap();
            shaderProgram.DisposeChildren = true;
            shaderProgram.Dispose();
            inputManager.Dispose();
            skyboxShader.DisposeChildren = true;
            skyboxShader.Dispose();
        }

        private static void onDisplay()
        {

        }

        private static void onRenderFrame()
        {

            InstanceCount = 0;
            int VertCount = 0;
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shaderProgram.Use();
            UpdateLightDataUBO();
            



            //uint vertexPositionIndex = (uint)Gl.GetAttribLocation(shaderProgram.ProgramID, "vertexPosition");
            //Gl.EnableVertexAttribArray(vertexPositionIndex);
            shaderProgram["view_matrix"].SetValue(camera.ViewMatrix);

            Gl.ActiveTexture(10);

            Gl.BindTexture(TextureTarget.Texture2DArray, BlockTextureIndex);

            Gl.Uniform1i(Gl.GetUniformLocation(shaderProgram.ProgramID, "textureArray"), 10);







            Gl.BindVertexArray(Vao);

            foreach (var Ent in updateService.ReturnUpdateList())
            {
                //Gl.BindTexture(Ent.Block.texture);
                if (Ent.Block.Light > 0)
                {
                    Lights.Add(new Vector4(Ent.Position.X, Ent.Position.Y, Ent.Position.Z, 0));
                    Colors.Add(new Vector4(Ent.Block.LightColor.X, Ent.Block.LightColor.Y, Ent.Block.LightColor.Z, 0));
                    Brightness.Add(new Vector4(Ent.Block.Light, 0 ,0, 0));
                }
                
                foreach (var GeometryElements in Ent.Block.Geometry.ConstitutentGeometry)
                {
                    InstanceCount +=1;

                    VertCount += GeometryElements.Vertices.Count;
                  Gl.BindBufferToShaderAttribute(GeometryElements.Normals, shaderProgram, "vertexNormals");
                  Gl.BindBufferToShaderAttribute(GeometryElements.Vertices, shaderProgram, "vertexPosition"); // REALLY inefficient, update to use a single draw call
                  Gl.BindBufferToShaderAttribute(Ent.Block.textureUVs, shaderProgram, "vertexUV");

                    


                    int location = Gl.GetUniformLocation(shaderProgram.ProgramID, "opacity");
                    Gl.Uniform1f(location, Ent.Block.Transparency);
                    

                    Gl.BindBuffer(GeometryElements.Elements);

                    Matrix4 model = Matrix4.Identity;

                    model = model * Matrix4.CreateTranslation(GeometryElements.Position);

                    model = model* Matrix4.CreateTranslation(-GeometryElements.Position);

                    model = model * GeometryElements.Rotation;

                    model = model * Matrix4.CreateTranslation(GeometryElements.Position);


                    ObjectMatrices.Add(model);

                    ActiveBlockTextures.Add(new Vector4(Ent.Block.textureID, 0 , 0 , 0));

                    //shaderProgram["model_matrix"].SetValue(model);

                   


                   // Gl.DrawElements(GeometryElements.Beginmode, GeometryElements.Elements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
                    
                }

            }

            //Gl.DrawElements(BeginMode.Triangles, 36, DrawElementsType.UnsignedInt, IntPtr.Zero);
            UpdateMatrixDataUBO();
            Gl.DrawElementsInstanced(BeginMode.Triangles,VertCount, DrawElementsType.UnsignedInt, IntPtr.Zero, InstanceCount);           
            Gl.BindVertexArray(0);



            Gl.DepthMask(false);
            skyboxShader.Use();
            // redefining some of the matrices so we can render a skybox without issues
            // quick and dirty
            Gl.DepthFunc(DepthFunction.Lequal);
            Gl.CullFace(CullFaceMode.Back);

            Gl.BindTexture(TextureTarget.TextureCubeMap, CubeMapTexture);

            skyboxShader["view_matrix"].SetValue(camera.SimplifiedViewMatrix);
            skyboxShader["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 10000f));


            Gl.BindVertexArray(skyboxVAO);
            Gl.ActiveTexture(3);


            Gl.DrawElements(BeginMode.Triangles, 36, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindVertexArray(0);


            //Gl.BindBufferToShaderAttribute(LocalSkybox.Vertices, skyboxShader, "aPos");

            Gl.DepthFunc(DepthFunction.Less);

            Gl.CullFace(CullFaceMode.Front);

            Gl.ActiveTexture(0);
            Gl.BindTexture(TextureTarget.TextureCubeMap, 0);

            Gl.DepthMask(true);
            Lights.Clear();
            Colors.Clear();
            Brightness.Clear();
            ObjectMatrices.Clear();
            ActiveBlockTextures.Clear();

            Glut.glutSwapBuffers();

        }
    }
}
