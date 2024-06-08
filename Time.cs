using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using WorldManager;
using BlockGameRenderer;

namespace Tick
{
    public delegate void GenericFunction();
    class Time
    {
        
        private Stopwatch stopwatch;
        private long previousTicks;
        private Thread timerThread;
        private bool running;
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
        private void Update()
        {
            stopwatch.Start();
            while (running)
            {
                
                foreach (var RunTimeElement in RunTimeFunctions)
                {
                    Thread.Sleep(1);
                    RunTimeElement();
                }
                Thread.Sleep(1);

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

        public float GetTicksPerSecond()
        {
            stopwatch.Stop();
            float TPS = (float)stopwatch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            stopwatch.Restart();
            return TPS;
        }

        // Method to get the number of ticks between the last and current call to this method.
        public long GetTicksSinceLastCall()
        {
            long currentTicks = stopwatch.ElapsedTicks;
            long deltaTicks = currentTicks - previousTicks;
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
