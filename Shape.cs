using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;

namespace Shapes
{
   public class Shape
    {
        public static VBO<Vector3> Vertices;
        public static VBO<int> Elements;
        public static Vector3 Position;
        public static Vector3 Size;
    }

   public class Triangle : Shape
    {
        

        public Triangle(Vector3 origin, Vector3 size)
        {
            // 0 , 1, 0
            // -1, -1, 0
            //1, -1, 0
            Vertices = new VBO<Vector3>(new Vector3[] { new Vector3(origin.X, origin.Y +size.Y, origin.Z), new Vector3(origin.X-size.X, origin.Y - size.Y, origin.Z), new Vector3(origin.X + size.X, origin.Y - size.Y, origin.Z) });
            Elements = new VBO<int>(new int[] {0,1,2}, BufferTarget.ElementArrayBuffer);
            Position = origin;
            Size = size;
        }
    }

    public class Square : Shape
    {
        public Square(Vector3 origin, Vector3 size)
        { //-1 , 1, 0
          //1, 1, 0
          //1, -1, 0
          //-1, -1, 0
            Elements = new VBO<int>(new int[] { 0, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);
            Vertices = new VBO<Vector3>(new Vector3[] { new Vector3(origin.X - size.X, origin.Y- size.Y, origin.Z), new Vector3(origin.X + size.X, origin.Y + size.Y, origin.Z), new Vector3(origin.X + size.X, origin.Y - size.Y, origin.Z), new Vector3(origin.X - size.X, origin.Y - size.Y, origin.Z) });
            Position = origin;
            Size = size;
        }
    }
}
