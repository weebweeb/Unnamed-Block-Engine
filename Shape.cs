using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;

namespace Shapes
{
   public class Shape
    {
        public VBO<Vector3> Vertices;
        public VBO<int> Elements;
        public Vector3 Position;
        public Vector3 Size;
        public BeginMode Beginmode;
        public Shape[] ConstitutentGeometry;
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
            Beginmode = BeginMode.Triangles;
        }
    }

    public class AbstractTriangle : Shape
    {
        private Triangle InstanceTriangle;
        public AbstractTriangle(Vector3 origin, Vector3 size) {
            InstanceTriangle = new Triangle(origin, size);
            ConstitutentGeometry = new Shape[] {InstanceTriangle};
        }

    }


    public class RightTriangle : Shape
    {
       public RightTriangle(Vector3 origin, Vector3 size, int flip)
        {
            Elements = new VBO<int>(new int[] { 0, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);
            Vertices = new VBO<Vector3>(new Vector3[] { new Vector3(origin.X + (size.X*flip), origin.Y + (size.Y*flip), origin.Z), new Vector3(origin.X - (size.X*flip), origin.Y - (size.Y*flip), origin.Z), new Vector3(origin.X - (size.X*flip), origin.Y + (size.Y*flip), origin.Z), new Vector3(origin.X + (size.X*flip), origin.Y + (size.Y*flip), origin.Z) });
            Position = origin;
            Size = size;
            Beginmode = BeginMode.TriangleFan;
        }
    }


    public class Square : Shape
    {
        public RightTriangle Side1;
        public RightTriangle Side2;
        
        public Square(Vector3 origin, Vector3 size)
        { //-1 , 1, 0
          //1, 1, 0
          //1, -1, 0
          //-1, -1, 0


            Side1 = new RightTriangle(origin, size, 1);
            Side2 = new RightTriangle(origin, size, -1);
            ConstitutentGeometry = new Shape[] {Side1, Side2};
        }
    }

}
