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

            texture = new Texture("grass.png");

            ID = 0;

            textureUVs = new VBO<Vector2>(new Vector2[] { 
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),new Vector2(0,1),
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),new Vector2(0,1),
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),new Vector2(0,1),
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),new Vector2(0,1),
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),new Vector2(0,1),
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),new Vector2(0,1),
            });

            Geometry = new Cube(position, size, ExampleColor3D, Angle);

            }
        }
}
