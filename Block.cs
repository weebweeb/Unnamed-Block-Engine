﻿using System;
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
        public int textureID { get; set; }

        public Shape Geometry { get; set; }

        public VBO<Vector2> textureUVs {get; set;}

        public float Light = 0; // how much light the block gives off, 0 for none

        public Vector3 LightColor = new Vector3(1, 1, 1); // the color of the light the item gives off

        public int ID { get; set; }

        public float Transparency = 1;

        public bool ContainsTransparencies = false; // a flag for the rendering engine for whether an object should be considered "somewhat" opaque, or contains opaque elements
    }


   public class Grass : Block
    {

       VBO<Vector3> ExampleColor3D = new VBO<Vector3>(new Vector3[] 
            {  new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) }, BufferTarget.ArrayBuffer);

        public Grass(Vector3 position, Vector3 size, Matrix4 Angle)
        {
            Hardness = 1;

            textureID = 0;

            Light = 1f;

            LightColor = new Vector3(0, 1f, 0);

            ID = 0;

            textureUVs = new VBO<Vector2>(new Vector2[] {
                // Side face //
                new Vector2(2f / 10f, 1f), new Vector2(3f / 10f, 1f), new Vector2(3f / 10f, 0f), new Vector2(2f / 10f, 0f),

                // Back face //
                new Vector2(4f / 10f, 1f), new Vector2(4f / 10f, 0f), new Vector2(3f / 10f, 0f), new Vector2(3f / 10f, 1f),

                // Side face 
                new Vector2(5f / 10f, 1f), new Vector2(5f / 10f, 0f), new Vector2(4f / 10f, 0f), new Vector2(4f / 10f, 1f),

                // Front face //
                new Vector2(0f / 10f, 1f), new Vector2(1f / 10f, 1f), new Vector2(1f / 10f, 0f), new Vector2(0f / 10f, 0f),

                // Bottom face
               new Vector2(6f / 10f, 0f), new Vector2(5f / 10f, 0f), new Vector2(5f / 10f, 1f), new Vector2(6f / 10f, 1f), 

                // Top face
                new Vector2(1f / 10f, 1f), new Vector2(2f / 10f, 1f), new Vector2(2f / 10f, 0f), new Vector2(1f / 10f, 0f),


            }, BufferTarget.ArrayBuffer);

            Geometry = new Cube(position, size, ExampleColor3D, Angle);

            }
        }

    public class Dirt : Block
    {

        VBO<Vector3> ExampleColor3D = new VBO<Vector3>(new Vector3[]
             {  new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) }, BufferTarget.ArrayBuffer);

        public Dirt(Vector3 position, Vector3 size, Matrix4 Angle)
        {
            Hardness = 1;

            textureID = 3;

            ID = 0;

            textureUVs = new VBO<Vector2>(new Vector2[] {
                // Side face //
                new Vector2(2f / 10f, 1f), new Vector2(3f / 10f, 1f), new Vector2(3f / 10f, 0f), new Vector2(2f / 10f, 0f),

                // Back face //
                new Vector2(4f / 10f, 1f), new Vector2(4f / 10f, 0f), new Vector2(3f / 10f, 0f), new Vector2(3f / 10f, 1f),

                // Side face 
                new Vector2(5f / 10f, 1f), new Vector2(5f / 10f, 0f), new Vector2(4f / 10f, 0f), new Vector2(4f / 10f, 1f),

                // Front face //
                new Vector2(0f / 10f, 1f), new Vector2(1f / 10f, 1f), new Vector2(1f / 10f, 0f), new Vector2(0f / 10f, 0f),

                // Bottom face
               new Vector2(6f / 10f, 0f), new Vector2(5f / 10f, 0f), new Vector2(5f / 10f, 1f), new Vector2(6f / 10f, 1f), 

                // Top face
                new Vector2(1f / 10f, 1f), new Vector2(2f / 10f, 1f), new Vector2(2f / 10f, 0f), new Vector2(1f / 10f, 0f),


            }, BufferTarget.ArrayBuffer);

            Geometry = new Cube(position, size, ExampleColor3D, Angle);

        }
    }

    public class Stone : Block
    {

        VBO<Vector3> ExampleColor3D = new VBO<Vector3>(new Vector3[]
             {  new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) }, BufferTarget.ArrayBuffer);

        public Stone(Vector3 position, Vector3 size, Matrix4 Angle)
        {
            Hardness = 5;

            textureID = 1;

            ID = 0;

            textureUVs = new VBO<Vector2>(new Vector2[] {
                // Side face //
                new Vector2(2f / 10f, 1f), new Vector2(3f / 10f, 1f), new Vector2(3f / 10f, 0f), new Vector2(2f / 10f, 0f),

                // Back face //
                new Vector2(4f / 10f, 1f), new Vector2(4f / 10f, 0f), new Vector2(3f / 10f, 0f), new Vector2(3f / 10f, 1f),

                // Side face 
                new Vector2(5f / 10f, 1f), new Vector2(5f / 10f, 0f), new Vector2(4f / 10f, 0f), new Vector2(4f / 10f, 1f),

                // Front face //
                new Vector2(0f / 10f, 1f), new Vector2(1f / 10f, 1f), new Vector2(1f / 10f, 0f), new Vector2(0f / 10f, 0f),

                // Bottom face
               new Vector2(6f / 10f, 0f), new Vector2(5f / 10f, 0f), new Vector2(5f / 10f, 1f), new Vector2(6f / 10f, 1f), 

                // Top face
                new Vector2(1f / 10f, 1f), new Vector2(2f / 10f, 1f), new Vector2(2f / 10f, 0f), new Vector2(1f / 10f, 0f),


            }, BufferTarget.ArrayBuffer);

            Geometry = new Cube(position, size, ExampleColor3D, Angle);

        }
    }


    public class OakLog : Block
    {

        VBO<Vector3> ExampleColor3D = new VBO<Vector3>(new Vector3[]
             {  new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) }, BufferTarget.ArrayBuffer);

        public OakLog(Vector3 position, Vector3 size, Matrix4 Angle)
        {
            Hardness = 3;

            textureID = 5;

            ID = 0;

            textureUVs = new VBO<Vector2>(new Vector2[] {
                // Side face //
                new Vector2(2f / 10f, 1f), new Vector2(3f / 10f, 1f), new Vector2(3f / 10f, 0f), new Vector2(2f / 10f, 0f),

                // Back face //
                new Vector2(4f / 10f, 1f), new Vector2(4f / 10f, 0f), new Vector2(3f / 10f, 0f), new Vector2(3f / 10f, 1f),

                // Side face 
                new Vector2(5f / 10f, 1f), new Vector2(5f / 10f, 0f), new Vector2(4f / 10f, 0f), new Vector2(4f / 10f, 1f),

                // Front face //
                new Vector2(0f / 10f, 1f), new Vector2(1f / 10f, 1f), new Vector2(1f / 10f, 0f), new Vector2(0f / 10f, 0f),

                // Bottom face
               new Vector2(6f / 10f, 0f), new Vector2(5f / 10f, 0f), new Vector2(5f / 10f, 1f), new Vector2(6f / 10f, 1f), 

                // Top face
                new Vector2(1f / 10f, 1f), new Vector2(2f / 10f, 1f), new Vector2(2f / 10f, 0f), new Vector2(1f / 10f, 0f),


            }, BufferTarget.ArrayBuffer);

            Geometry = new Cube(position, size, ExampleColor3D, Angle);

        }
    }


    public class OakLeaves : Block
    {

        VBO<Vector3> ExampleColor3D = new VBO<Vector3>(new Vector3[]
             {  new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) }, BufferTarget.ArrayBuffer);

        public OakLeaves(Vector3 position, Vector3 size, Matrix4 Angle)
        {
            Hardness = 1;

            textureID = 4;

            ID = 0;

            Transparency = 1f;

            ContainsTransparencies = true;

            textureUVs = new VBO<Vector2>(new Vector2[] {
                // Side face //
                new Vector2(2f / 10f, 1f), new Vector2(3f / 10f, 1f), new Vector2(3f / 10f, 0f), new Vector2(2f / 10f, 0f),

                // Back face //
                new Vector2(4f / 10f, 1f), new Vector2(4f / 10f, 0f), new Vector2(3f / 10f, 0f), new Vector2(3f / 10f, 1f),

                // Side face 
                new Vector2(5f / 10f, 1f), new Vector2(5f / 10f, 0f), new Vector2(4f / 10f, 0f), new Vector2(4f / 10f, 1f),

                // Front face //
                new Vector2(0f / 10f, 1f), new Vector2(1f / 10f, 1f), new Vector2(1f / 10f, 0f), new Vector2(0f / 10f, 0f),

                // Bottom face
               new Vector2(6f / 10f, 0f), new Vector2(5f / 10f, 0f), new Vector2(5f / 10f, 1f), new Vector2(6f / 10f, 1f), 

                // Top face
                new Vector2(1f / 10f, 1f), new Vector2(2f / 10f, 1f), new Vector2(2f / 10f, 0f), new Vector2(1f / 10f, 0f),


            }, BufferTarget.ArrayBuffer);

            Geometry = new Cube(position, size, ExampleColor3D, Angle);

        }
    }




    public class Water : Block
    {

        VBO<Vector3> ExampleColor3D = new VBO<Vector3>(new Vector3[]
             {  new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 0.5f, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1) });

        public Water(Vector3 position, Vector3 size, Matrix4 Angle)
        {
            Hardness = 1;

            textureID = 2;

            Light = 0f;

            Transparency = 0.6f;

            ContainsTransparencies = true;

            LightColor = new Vector3(0, 1f, 0);

            ID = 0;

            textureUVs = new VBO<Vector2>(new Vector2[] {
                // Side face //
                new Vector2(2f / 10f, 1f), new Vector2(3f / 10f, 1f), new Vector2(3f / 10f, 0f), new Vector2(2f / 10f, 0f),

                // Back face //
                new Vector2(4f / 10f, 1f), new Vector2(4f / 10f, 0f), new Vector2(3f / 10f, 0f), new Vector2(3f / 10f, 1f),

                // Side face 
                new Vector2(5f / 10f, 1f), new Vector2(5f / 10f, 0f), new Vector2(4f / 10f, 0f), new Vector2(4f / 10f, 1f),

                // Front face //
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


