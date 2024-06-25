﻿using System;
using System.Collections.Generic;
using System.Text;
using Shapes;
using System.Numerics;
using OpenGL;


namespace Skybox
{
    class Skybox
    {
        public VBO<Vector3> Vertices = new VBO<Vector3>(new Vector3[]
                 {
                   new Vector3(-1.0f, -1.0f,  1.0f),
                   new Vector3( 1.0f, -1.0f,  1.0f),
                   new Vector3(1.0f, -1.0f, -1.0f),
                   new Vector3(-1.0f, -1.0f, -1.0f),
                   new Vector3(-1.0f,  1.0f,  1.0f),
                   new Vector3( 1.0f,  1.0f,  1.0f),
                   new Vector3( 1.0f,  1.0f, -1.0f),
                   new Vector3(-1.0f,  1.0f, -1.0f)
             });

       public VBO<int> Elements = new VBO<int>(new int[]{
                // Right
	            1, 2, 6,
                6, 5, 1,
	            // Left
	            0, 4, 7,
                7, 3, 0,
	            // Top
	            4, 5, 6,
                6, 7, 4,
	            // Bottom
	            0, 3, 2,
                2, 1, 0,
	            // Back
	            0, 1, 5,
                5, 4, 0,
	            // Front
	            3, 7, 6,
                6, 2, 3
            });


        public string[] SkyboxTexturePaths { get; set; }

    }
}
