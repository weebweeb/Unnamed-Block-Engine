using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;
using Shapes;

namespace WorldManager
{
    public class World
    {

        public OctreeNode Entities;
        public World(Vector3 Min, Vector3 Max) 
        {

            // Create a bounding box for the map
            BoundingBox mapBounds = new BoundingBox
            {
                Min = new Vector3(Min.X, Min.Y, Min.Z), // Minimum corner of the map
                Max = new Vector3(Max.X, Max.Y, Max.Z)     // Maximum corner of the map
            };

            // Create the root node of the octree
           Entities = new OctreeNode(mapBounds);
        }

        public void Insert(Vector3 ImportPosition, Vector3 ImportSize, VBO<Vector3> ImportGeometry, VBO<int> ImportElements)
        {
            Entities.Insert(new GameEntity
               {
                Position = ImportPosition,
                Geometry = ImportGeometry,
                Elements = ImportElements,
                Bounds = new BoundingBox
                {
                    Min = new Vector3(ImportPosition.X - (ImportSize.X / 2), ImportPosition.Y - (ImportSize.Y / 2), ImportPosition.Z - (ImportSize.Z / 2)),
                    Max = new Vector3(ImportPosition.X + (ImportSize.X / 2), ImportPosition.Y + (ImportSize.Y / 2), ImportPosition.Z + (ImportSize.Z / 2))
                }
               }
            );
        }
    }
}
