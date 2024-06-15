using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using WorldManager;
using BlockGameRenderer;
using System.Threading.Tasks;

namespace Tick
{
    public delegate void GenericFunction();
    class Time
    {
        
        private Stopwatch stopwatch;
        private double previousTicks;
        private Thread timerThread;
        private bool running;
        
        private double currentTime = 0;
        public double ElapsedThreadTicks = 0;
        public double TPS = 0;
        public double minuteCounter = 0;
        private const double FixedTimestep = 1.0 / 60.0; // 60 FPS
        private static double previousTime = 0.0;
        private static double lag = 0.0;
        private static double lastTickTime = 0.0;


        private List<GenericFunction> RunTimeFunctions = new List<GenericFunction>();

        public void addRunTimeFunction(GenericFunction PassedFunction)
        {
            RunTimeFunctions.Add(PassedFunction);
        }
        public Time()
        {
            stopwatch = new Stopwatch();
            previousTicks = 0;
            running = true;
            timerThread = new Thread(Update);
            timerThread.Start();
        }

        // Update method runs in a separate thread and continuously updates the stopwatch.
        private async void Update()
        {
            stopwatch.Start();


            while (running)
            {
                double currentTime = stopwatch.Elapsed.TotalMilliseconds;
                double elapsedTime = currentTime - previousTime;
                previousTime = currentTime;
                lag += elapsedTime;
                await Task.Delay((int)(FixedTimestep * 1000));
               
                minuteCounter += 1;

                if (lag >= FixedTimestep)
                {
                    
                    lag -= FixedTimestep;
                    ElapsedThreadTicks += 1;


                    foreach (var RunTimeElement in RunTimeFunctions)
                    {

                        RunTimeElement();

                    }

                    if (currentTime - lastTickTime >= 1.0)
                    {
                        TPS = (ElapsedThreadTicks *1000) / (currentTime - lastTickTime);
                        ElapsedThreadTicks = 0;
                        lastTickTime = currentTime;
                    }
                }
                
            }
            stopwatch.Stop();
        }

        // Method to get the total elapsed time in ticks.
        public long GetElapsedTicks()
        {
            return stopwatch.ElapsedTicks;
        }

        public long GetElapsedMilliseconds()
        {
            return stopwatch.ElapsedMilliseconds;
        }

        public double GetElapsedSeconds()
        {
            return stopwatch.Elapsed.TotalSeconds;
        }

        public double GetTicksPerSecond()
        {
            return TPS;
        }

        // Method to get the number of ticks between the last and current call to this method.
        public double GetTicksSinceLastCall()
        {
            double currentTicks = ElapsedThreadTicks;
            double deltaTicks = currentTicks - previousTicks;
            previousTicks = currentTicks;
            return deltaTicks;
        }

        public void Reset()
        {
            stopwatch.Reset();
            previousTicks = 0;
        }

        public void Stop()
        {
            running = false;
            if (timerThread.IsAlive)
            {
                timerThread.Join();
            }
        }
    }
}
