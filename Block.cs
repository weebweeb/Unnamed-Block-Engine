using System;
using System.Collections.Generic;
using System.IO;
using Shapes;
using WorldManager;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;
using System.Drawing;


namespace Blocks
{
    public class Block
    {
        public int Hardness { get; set; }
        public Texture texture { get; set; }

        public Shape Geometry { get; set; }

        public VBO<Vector2> textureUVs {get; set;}

        public int ID { get; set; }
    }


   public class Grass : Block
    {

       VBO<Vector3> ExampleColor3D = new VBO<Vector3>(new Vector3[] 
            {  new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) });
        public Grass(Vector3 position, Vector3 size, Matrix4 Angle)
        {
            Hardness = 1;

            //string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            texture = new Texture("blocktexturetemplate.png");

            ID = 0;

            textureUVs = new VBO<Vector2>(new Vector2[] {
                // Side face //3
                new Vector2(2f / 10f, 1f), new Vector2(3f / 10f, 1f), new Vector2(3f / 10f, 0f), new Vector2(2f / 10f, 0f),

                // Back face //4
                new Vector2(4f / 10f, 1f), new Vector2(4f / 10f, 0f), new Vector2(3f / 10f, 0f), new Vector2(3f / 10f, 1f),



                // Side face  //5

                new Vector2(5f / 10f, 1f), new Vector2(5f / 10f, 0f), new Vector2(4f / 10f, 0f), new Vector2(4f / 10f, 1f),



                // Front face //1
                new Vector2(0f / 10f, 1f), new Vector2(1f / 10f, 1f), new Vector2(1f / 10f, 0f), new Vector2(0f / 10f, 0f),

                // Bottom face

               new Vector2(6f / 10f, 0f), new Vector2(5f / 10f, 0f), new Vector2(5f / 10f, 1f), new Vector2(6f / 10f, 1f), 




                // Top face
                new Vector2(1f / 10f, 1f), new Vector2(2f / 10f, 1f), new Vector2(2f / 10f, 0f), new Vector2(1f / 10f, 0f),


            });

            Geometry = new Cube(position, size, ExampleColor3D, Angle);

            }
        }
}
