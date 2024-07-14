using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;

namespace Shapes
{
    public class Shape
    {
        public VBO<Vector3> Vertices { get; set; }
        public VBO<int> Elements { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
        public BeginMode Beginmode { get; set; }
        public Shape[] ConstitutentGeometry { get; set; }

        public VBO<Vector3> Normals { get; set; }
        public VBO<Vector3> Color { get; set; }
        public Matrix4 Rotation { get; set; }
        public Vector3 Orientation = new Vector3(0, 0, 0); // whichever way the object is facing when spawned in will be considered 0,0,0 on its orientation. this field should only be really used to get an object's angle quickly and is only changed manually

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
            Vertices = new VBO<Vector3>(new Vector3[] { new Vector3(origin.X, origin.Y + size.Y, origin.Z), new Vector3(origin.X - size.X, origin.Y - size.Y, origin.Z), new Vector3(origin.X + size.X, origin.Y - size.Y, origin.Z) }, BufferTarget.ArrayBuffer);
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
            Vertices = new VBO<Vector3>(new Vector3[] { new Vector3(origin.X + (size.X*flip), origin.Y + (size.Y*flip), origin.Z), new Vector3(origin.X - (size.X*flip), origin.Y - (size.Y*flip), origin.Z), new Vector3(origin.X - (size.X*flip), origin.Y + (size.Y*flip), origin.Z), new Vector3(origin.X + (size.X*flip), origin.Y + (size.Y*flip), origin.Z) }, BufferTarget.ArrayBuffer);
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

            Position = origin;
            Size = size;
            Rotation = Angle;
            Side1 = new RightTriangle(origin, size, 1, TrigColor, Angle);
            Side2 = new RightTriangle(origin, size, -1, TrigColor, Angle);
            ConstitutentGeometry = new Shape[] {Side1, Side2};
        }
    }


    public class Cube : Shape
    {

        public Cube(Vector3 origin, Vector3 size, VBO<Vector3> TrigColor, Matrix4 Angle)
        {

            Position = origin;
            Size = size;
            Rotation = Angle;

            ConstitutentGeometry = new Shape[] { new Shape { 
                Size = size,
                Position = origin,
                Vertices = new VBO<Vector3>(new Vector3[]
                {
                    new Vector3(size.X, size.Y, size.Z), new Vector3(size.X, size.Y, -size.Z), new Vector3(size.X, -size.Y, -size.Z), new Vector3(size.X, -size.Y, size.Z), // Right face
                    new Vector3(-size.X, size.Y, size.Z), new Vector3(-size.X, -size.Y, size.Z), new Vector3(-size.X, -size.Y, -size.Z), new Vector3(-size.X, size.Y, -size.Z), // Left face
                    new Vector3(size.X, size.Y, size.Z), new Vector3(size.X, -size.Y, size.Z), new Vector3(-size.X, -size.Y, size.Z), new Vector3(-size.X, size.Y, size.Z), // Front face
                    new Vector3(size.X, size.Y, -size.Z), new Vector3(-size.X, size.Y, -size.Z), new Vector3(-size.X, -size.Y, -size.Z), new Vector3(size.X, -size.Y, -size.Z), // Back face
                    new Vector3(size.X, size.Y, size.Z), new Vector3(-size.X, size.Y, size.Z), new Vector3(-size.X, size.Y, -size.Z), new Vector3(size.X, size.Y, -size.Z), // Top face
                    new Vector3(size.X, -size.Y, size.Z), new Vector3(size.X, -size.Y, -size.Z), new Vector3(-size.X, -size.Y, -size.Z), new Vector3(-size.X, -size.Y, size.Z)  // Bottom face
                }, BufferTarget.ArrayBuffer),
                Color = TrigColor,
                Elements = new VBO<int>(new int[]
                    { // Right face
                0, 1, 2, 0, 2, 3,
                // Left face
                4, 5, 6, 4, 6, 7,
                // Front face
                8, 9, 10, 8, 10, 11,
                // Back face
                12, 13, 14, 12, 14, 15,
                // Top face
                16, 17, 18, 16, 18, 19,
                // Bottom face
                20, 21, 22, 20, 22, 23}, 
                BufferTarget.ElementArrayBuffer),
                Beginmode = BeginMode.Triangles,
                Rotation = Angle,
                Normals = new VBO<Vector3>(new Vector3[]
                {
                    new Vector3(1,0,0),new Vector3(1,0,0),new Vector3(1,0,0),new Vector3(1,0,0), //Right Face
                    new Vector3(-1,0,0),new Vector3(-1,0,0),new Vector3(-1,0,0),new Vector3(-1,0,0), // Left Face
                    new Vector3(0,0,1),new Vector3(0,0,1),new Vector3(0,0,1),new Vector3(0,0,1), //Front Face
                    new Vector3(0,0,-1),new Vector3(0,0,-1),new Vector3(0,0,-1),new Vector3(0,0,-1), //Back Face
                    new Vector3(0,1,0),new Vector3(0,1,0),new Vector3(0,1,0),new Vector3(0,1,0), // Top Face
                    new Vector3(0,-1,0),new Vector3(0,-1,0),new Vector3(0,-1,0),new Vector3(0,-1,0), //Bottom Face
                }, BufferTarget.ArrayBuffer)


                } 
            
            };

        }
    }


    public class Pyramid : Shape
    {
        public Pyramid(Vector3 origin, Vector3 size, VBO<Vector3> TrigColor, Matrix4 Angle)
        {
            ConstitutentGeometry = new Shape[] { };
        }
    }

}
