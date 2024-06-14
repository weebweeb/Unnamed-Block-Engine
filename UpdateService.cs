using System;
using System.Collections.Generic;
using System.Text;
using WorldManager;
using Tick;
using System.Numerics;
using System.Threading.Tasks;


namespace BlockGameRenderer
{
    class UpdateService
    {
        public static World world;

        public static int RenderDistance;

        public static int TimeBetweenUpdates;

        public List<GameEntity> VisibleEntities;

        public static Vector3 Position;

        Time UpdateThread;

       public async void Update()
        {
            
            VisibleEntities = world.Entities.Retrieve(new BoundingBox
            {
                Min = new Vector3(Position.X - (RenderDistance / 2), Position.Y - (RenderDistance / 2), Position.Z - (RenderDistance / 2)),
                Max = new Vector3(Position.X + (RenderDistance / 2), Position.Y + (RenderDistance / 2), Position.Z + (RenderDistance / 2))
            });
            await Task.Delay(TimeBetweenUpdates);
        }

        public UpdateService(World map, int Renderdistance, int TimeBetweenupdates, Vector3 pos)
        {
            VisibleEntities = new List<GameEntity>() { };
            Position = pos;
            TimeBetweenUpdates = TimeBetweenupdates;
            RenderDistance = Renderdistance;
            world = map;
            UpdateThread = new Time();
            

            UpdateThread.addRunTimeFunction(new GenericFunction(Update));
        }

        public GameEntity[] ReturnUpdateList()
        {
            return VisibleEntities.ToArray(); ;
        }
       

        

        
    }

        
 }

