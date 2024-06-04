
using System;
using System.Collections.Generic;
using System.Text;
using Tick;

namespace Animations
{

    class AnimationHandler
    {
        private Time AnimationThread = new Time();
        public static List<Animation> QueuedAnimations = new List<Animation> { };

       public void renderAnimations() 
        {
            foreach (Animation animation in QueuedAnimations) {
                animation.NextFrame();
            };
        }
       public AnimationHandler() 
        {
            AnimationThread.addRunTimeFunction(new GenericFunction(renderAnimations));
        }
        
        public void QueueAnimation(Animation QueuedAnimation)
        {
            QueuedAnimation.Index = QueuedAnimations.Count;
            QueuedAnimations.Add(QueuedAnimation);
        }

        public static void RemoveAnimation(int index)
        {
            QueuedAnimations.RemoveAt(index);
        }

    }
    class Animation
    {
        public int CurrentFrame = 0; // keeps track of the current frame in the AnimationFunctions array
        public GenericFunction[] AnimationFunctions { get; set; }
        public bool Loopable { get; set; } // determines whether the animation loops after reaching the last index

        public int Index = 0;


        public void NextFrame() 
        {
            CurrentFrame += 1;
            if (CurrentFrame > AnimationFunctions.Length)
            {
                if (Loopable == true)
                {
                    CurrentFrame = 0;
                    AnimationFunctions[CurrentFrame]();
                }
                else { AnimationHandler.RemoveAnimation(Index); }
            }
            else
            {

                AnimationFunctions[CurrentFrame]();
            }

        }
    }
}
