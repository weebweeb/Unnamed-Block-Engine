
using System;
using System.Collections.Generic;
using System.Text;
using Tick;
using Shapes;
using WorldManager;
using BlockGameRenderer;

namespace Animations
{
    public delegate void GenericAnimationFunction(Shape subject);
    class AnimationHandler
    {
        private Time AnimationThread = new Time();
        public static List<Animation> QueuedAnimations = new List<Animation> { };

       public void renderAnimations() 
        {
            if (QueuedAnimations.Count > 0)
            {
                foreach (Animation animation in QueuedAnimations)
                {
                    animation.NextFrame();
                };
            }
        }
       public AnimationHandler() 
        {
            AnimationThread.addRunTimeFunction(new GenericFunction(renderAnimations));
        }
        
        public void QueueAnimation(Animation QueuedAnimation)
        {
            QueuedAnimation.Index = QueuedAnimations.Count -1;
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
        public GenericAnimationFunction[] AnimationFunctions { get; set; }
        public bool Loopable { get; set; } // determines whether the animation loops after reaching the last index

        public int Index = 0; // the current index in the public array of all animations

        public Shape Subject { get; set; }


        public void NextFrame() 
        {
            CurrentFrame += 1;
            if (CurrentFrame > AnimationFunctions.Length -1)
            {
                if (Loopable == true)
                {
                    CurrentFrame = 0;
                    AnimationFunctions[CurrentFrame](Subject);
                }
                else { AnimationHandler.RemoveAnimation(Index); }
            }
            else
            {

                AnimationFunctions[CurrentFrame](Subject);
            }

        }
    }
}
