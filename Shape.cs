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
        public VBO<Vector3> Color;
        public Matrix4 Rotation;
        public Vector3 Orientation = new Vector3(0, 0, 0);

        public static Matrix4 CreateRotationMatrix(Vector3 axis, float angle)
        {
            return Matrix4.CreateFromAxisAngle(axis, angle);
        }
    }

   public class Triangle : Shape
    {


        public Triangle(Vector3 origin, Vector3 size, VBO<Vector3> TrigColor, Matrix4 Angle)
        {
            // 0 , 1, 0
            // -1, -1, 0
            //1, -1, 0
            Vertices = new VBO<Vector3>(new Vector3[] { new Vector3(origin.X, origin.Y + size.Y, origin.Z), new Vector3(origin.X - size.X, origin.Y - size.Y, origin.Z), new Vector3(origin.X + size.X, origin.Y - size.Y, origin.Z) });
            Elements = new VBO<int>(new int[] { 0, 1, 2 }, BufferTarget.ElementArrayBuffer);
            Position = origin;
            Size = size;
            Beginmode = BeginMode.Triangles;
            Color = TrigColor;
            Rotation = Angle;
        }
    }

    public class AbstractTriangle : Shape
    {
        private Triangle InstanceTriangle;
        public AbstractTriangle(Vector3 origin, Vector3 size, VBO<Vector3> TrigColor, Matrix4 Angle) {
            InstanceTriangle = new Triangle(origin, size, TrigColor, Angle);
            ConstitutentGeometry = new Shape[] {InstanceTriangle};
        }

    }


    public class RightTriangle : Shape
    {
       public RightTriangle(Vector3 origin, Vector3 size, int flip, VBO<Vector3> TrigColor, Matrix4 Angle)
        {
            Elements = new VBO<int>(new int[] { 0, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);
            Vertices = new VBO<Vector3>(new Vector3[] { new Vector3(origin.X + (size.X*flip), origin.Y + (size.Y*flip), origin.Z), new Vector3(origin.X - (size.X*flip), origin.Y - (size.Y*flip), origin.Z), new Vector3(origin.X - (size.X*flip), origin.Y + (size.Y*flip), origin.Z), new Vector3(origin.X + (size.X*flip), origin.Y + (size.Y*flip), origin.Z) });
            Position = origin;
            Size = size;
            Beginmode = BeginMode.TriangleFan;
            Color = TrigColor;
            Rotation = Angle;
        }
    }



    public class Square : Shape
    {
        public RightTriangle Side1;
        public RightTriangle Side2;
        
        public Square(Vector3 origin, Vector3 size, VBO<Vector3> TrigColor, Matrix4 Angle)
        { //-1 , 1, 0
          //1, 1, 0
          //1, -1, 0
          //-1, -1, 0


            Side1 = new RightTriangle(origin, size, 1, TrigColor, Angle);
            Side2 = new RightTriangle(origin, size, -1, TrigColor, Angle);
            ConstitutentGeometry = new Shape[] {Side1, Side2};
        }
    }

}
