using System;
using Tao.FreeGlut;
using OpenGL;
using System.Collections.Generic;
using System.Numerics;
using Shapes;

namespace WorldManager
{

// Represents a single game entity with a position and a bounding box.
public class GameEntity //Abstract a instance into a generic object
{
    public Vector3 Position { get; set; }
    public VBO<int> Elements { get; set; } // Geometry Element data
    public VBO<Vector3> Geometry { get; set; } //Geometry Vertex data
    public BoundingBox Bounds { get; set; } // Assume BoundingBox is a class defining the entity's bounds.
}

// Represents a bounding box with minimum and maximum corners.
public class BoundingBox
{
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }

    // Check if this bounding box intersects with another bounding box.
    public bool Intersects(BoundingBox other)
    {
        return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
               (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y) &&
               (Min.Z <= other.Max.Z && Max.Z >= other.Min.Z);
    }

    // Check if this bounding box contains another bounding box.
    public bool Contains(BoundingBox other)
    {
        return (Min.X <= other.Min.X && Max.X >= other.Max.X) &&
               (Min.Y <= other.Min.Y && Max.Y >= other.Max.Y) &&
               (Min.Z <= other.Min.Z && Max.Z >= other.Max.Z);
    }
}

// Represents a node in the octree.
public class OctreeNode
{
    private const int MAX_ENTITIES = 8; // Max entities before subdivision
    private const int MAX_DEPTH = 8; // Max depth of the octree

    public BoundingBox Bounds { get; private set; }
    private List<GameEntity> entities;
    private OctreeNode[] children;
    private int depth;

    // Constructor for creating a new octree node.
    public OctreeNode(BoundingBox bounds, int depth = 0)
    {
        this.Bounds = bounds;
        this.depth = depth;
        entities = new List<GameEntity>();
        children = null;
    }

    // Subdivides the current node into 8 child octants.
    private void Subdivide()
    {
        Vector3 size = (Bounds.Max - Bounds.Min) / 2;
        Vector3 mid = Bounds.Min + size;

        children = new OctreeNode[8];
        children[0] = new OctreeNode(new BoundingBox { Min = Bounds.Min, Max = mid }, depth + 1);
        children[1] = new OctreeNode(new BoundingBox { Min = new Vector3(mid.X, Bounds.Min.Y, Bounds.Min.Z), Max = new Vector3(Bounds.Max.X, mid.Y, mid.Z) }, depth + 1);
        children[2] = new OctreeNode(new BoundingBox { Min = new Vector3(Bounds.Min.X, mid.Y, Bounds.Min.Z), Max = new Vector3(mid.X, Bounds.Max.Y, mid.Z) }, depth + 1);
        children[3] = new OctreeNode(new BoundingBox { Min = new Vector3(mid.X, mid.Y, Bounds.Min.Z), Max = new Vector3(Bounds.Max.X, Bounds.Max.Y, mid.Z) }, depth + 1);
        children[4] = new OctreeNode(new BoundingBox { Min = new Vector3(Bounds.Min.X, Bounds.Min.Y, mid.Z), Max = new Vector3(mid.X, mid.Y, Bounds.Max.Z) }, depth + 1);
        children[5] = new OctreeNode(new BoundingBox { Min = new Vector3(mid.X, Bounds.Min.Y, mid.Z), Max = new Vector3(Bounds.Max.X, mid.Y, Bounds.Max.Z) }, depth + 1);
        children[6] = new OctreeNode(new BoundingBox { Min = new Vector3(Bounds.Min.X, mid.Y, mid.Z), Max = new Vector3(mid.X, Bounds.Max.Y, Bounds.Max.Z) }, depth + 1);
        children[7] = new OctreeNode(new BoundingBox { Min = mid, Max = Bounds.Max }, depth + 1);
    }

    // Inserts an entity into the octree node.
    public void Insert(GameEntity entity)
    {
        // If this node is not a leaf node, attempt to insert into a child node.
        if (children != null)
        {
            int index = GetChildIndex(entity.Bounds);
            if (index != -1)
            {
                children[index].Insert(entity);
                return;
            }
        }

        // Add the entity to the current node.
        entities.Add(entity);

        // If the node exceeds capacity and depth limit is not reached, subdivide.
        if (entities.Count > MAX_ENTITIES && depth < MAX_DEPTH)
        {
            if (children == null)
            {
                Subdivide();
            }

            // Re-distribute entities among children.
            int i = 0;
            while (i < entities.Count)
            {
                int index = GetChildIndex(entities[i].Bounds);
                if (index != -1)
                {
                    GameEntity entityToMove = entities[i];
                    entities.RemoveAt(i);
                    children[index].Insert(entityToMove);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    // Determines which child node an entity should go to based on its bounds.
    private int GetChildIndex(BoundingBox bounds)
    {
        for (int i = 0; i < 8; i++)
        {
            if (children[i].Bounds.Contains(bounds))
            {
                return i;
            }
        }
        return -1; // Entity does not completely fit in any child node
    }

    // Retrieves a list of potential collision candidates for the given bounding box.
    public List<GameEntity> Retrieve(BoundingBox bounds)
    {
        List<GameEntity> candidates = new List<GameEntity>();

        // If this node has children, check which child nodes intersect with the bounds.
        if (children != null)
        {
            foreach (var child in children)
            {
                if (child.Bounds.Intersects(bounds))
                {
                    candidates.AddRange(child.Retrieve(bounds));
                }
            }
        }

        // Add entities from the current node.
        candidates.AddRange(entities);

        return candidates;
    }
}
}
