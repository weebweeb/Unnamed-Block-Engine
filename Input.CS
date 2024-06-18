using System;
using Tao.FreeGlut;
using OpenGL;
using System.Numerics;
using LowLevelInput.Hooks;
using LowLevelInput.Converters;


namespace BlockGameRenderer
{
   static class Input
    {
        static VirtualKeyCode MoveForward = VirtualKeyCode.W;
        static VirtualKeyCode MoveBackward = VirtualKeyCode.S;
        static VirtualKeyCode MoveLeft = VirtualKeyCode.A;
        static VirtualKeyCode MoveRight = VirtualKeyCode.D;
        static VirtualKeyCode Jump = VirtualKeyCode.Space;
        static VirtualKeyCode Crouch = VirtualKeyCode.Lshift;
        static VirtualKeyCode Pause = VirtualKeyCode.Escape;
        static VirtualKeyCode FullScreen = VirtualKeyCode.F11;

        public static bool ForwardMomentum = false;
        public static bool BackwardMomentum = false;
        public static bool LeftMomentum = false;
        public static bool RightMomentum = false;
        public static bool UpMomentum = false;
        public static bool DownMomentum = false;

       



        public static void OnKeyboardDown(VirtualKeyCode Key, KeyState state) {
            if (state == KeyState.Down)
            {
                if (Key == Pause) { Glut.glutLeaveMainLoop(); }
                if (Key == FullScreen)
                {
                    Program.Fullscreen = !Program.Fullscreen;
                    if (Program.Fullscreen) { Glut.glutFullScreen(); }
                    else
                    {
                        Glut.glutPositionWindow(0, 0);
                        Glut.glutReshapeWindow(Program.width, Program.height);
                    }
                }
                if (Key == MoveForward) { ForwardMomentum = true; }
                if (Key == MoveBackward) { BackwardMomentum = true; }
                if (Key == MoveLeft) { LeftMomentum = true; }
                if (Key == MoveRight) { RightMomentum = true; }
                if (Key == Jump) { UpMomentum = true; }
                if (Key == Crouch) { DownMomentum = true; }
            }
            else if (state == KeyState.Up)
            {
                if (Key == MoveForward) { ForwardMomentum = false; }
                if (Key == MoveBackward) { BackwardMomentum = false; }
                if (Key == MoveLeft) { LeftMomentum = false; }
                if (Key == MoveRight) { RightMomentum = false; }
                if (Key == Jump) { UpMomentum = false; }
                if (Key == Crouch) { DownMomentum = false; }
            }

        }
    }
}
