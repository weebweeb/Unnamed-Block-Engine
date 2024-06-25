using System;
using System.Numerics;
using OpenGL;
using MathHelp;
using Tao.FreeGlut;
using System.Threading.Tasks;

namespace BlockGameRenderer
{
    public class Camera
    {
        private Vector3 position;
        private Vector3 lookAt = new Vector3(1,0,1);
        private Matrix4 viewMatrix; // cached view matrix
        private Matrix4 simplifiedviewMatrix;
        private bool dirty = true;  // true if the viewMatrix must be recalculated
        private bool simplifiedDirty = true;
        public bool LockCamera = false;
        public float pitch, yaw = 0;
        public float velocity = 2;
        public float Sensitivity = 0.01f;

        public Matrix4 ViewMatrix
        {
            get
            {
                if (dirty)
                {
                    Vector3 direction = GetDirection();
                    Vector3 target = position + direction;

                    viewMatrix = CreateLookAt(position, target, Vector3.UnitY);
                }
                return viewMatrix;
            }
        }

        public Matrix4 SimplifiedViewMatrix
        {
            get
            {
                if (simplifiedDirty | dirty == true)
                {
                    Vector3 direction = GetDirection();
                    Vector3 target = position + direction;

                    simplifiedviewMatrix = CreateLookAt(position, target, Vector3.UnitY);
                }
                return simplifiedviewMatrix;
            }
        }

        public Camera(Vector3 position, float pitch, float yaw)
        {
            this.position = position;
            this.pitch = pitch;
            this.yaw = yaw;

        }

        private Matrix4 CreateLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zAxis = Vector3.Normalize(eye - target); // Forward
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis)); // Right
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis); // Up

            return new Matrix4(new float[] {
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3.Dot(xAxis, eye), -Vector3.Dot(yAxis, eye), -Vector3.Dot(zAxis, eye), 1 }
            );
        }

        private Matrix4 CreateSimplifiedLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zAxis = Vector3.Normalize(eye - target); // Forward
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis)); // Right
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis); // Up

            return new Matrix4(new float[] {
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                      0,       0,      0, 0 }
            );
        }

        private Vector3 GetDirection()
        {
            Vector3 direction;
            direction.X = MathF.Cos(QuarternionHelper.DegreesToRadians(yaw)) * MathF.Cos(QuarternionHelper.DegreesToRadians(pitch));
            direction.Y = MathF.Sin(QuarternionHelper.DegreesToRadians(pitch));
            direction.Z = MathF.Sin(QuarternionHelper.DegreesToRadians(yaw)) * MathF.Cos(QuarternionHelper.DegreesToRadians(pitch));
            return Vector3.Normalize(direction);
        }


        // Moves the camera 
        public void Move(Vector3 by)
        {
            position += by;
            dirty = true;
        }

        
        //moves the camera relative to where the camera is facing
       
        public void MoveRelative(bool forward, bool backward, bool left, bool right, bool up, bool down)
        {
            if (forward)
            { position += GetDirection() * velocity; }
            if (backward)
            { position -= GetDirection() * velocity; }
            if (left)
            { position -= Vector3.Normalize(Vector3.Cross(GetDirection(), Vector3.UnitY)) * velocity; }
            if (right)
            { position += Vector3.Normalize(Vector3.Cross(GetDirection(), Vector3.UnitY)) * velocity; }
            if (up) { position += Vector3.UnitY * velocity; }
            if (down) { position -= Vector3.UnitY * velocity; }
            dirty = true;
        }

      

        public async void Interpolate2D(int x, int y) //interpolate the camera to rotate towards a set of 2D screen coordinates
        {
            
                   
            
            if (!LockCamera)
            {
                x = (Program.width / 2) - x;
                y = (Program.height / 2) - y;

                yaw -= (x * Sensitivity);
                pitch += (y * Sensitivity);

                if (pitch > 89.0f) pitch = 89.0f;
                if (pitch < -89.0f) pitch = -89.0f;
                dirty = true;
            }

            //if (x < 40) { Glut.glutWarpPointer(Program.width / 2, Program.height / 2);}
            //else if (x > Program.width - 60) { Glut.glutWarpPointer(Program.width / 2, Program.height / 2); };

            //if (y < 40) { Glut.glutWarpPointer(Program.width / 2, Program.height / 2);  }
            //else if (y > Program.height - 60) { Glut.glutWarpPointer(Program.width / 2, Program.height / 2);  };
            await Task.Delay(1);

            Glut.glutWarpPointer(Program.width / 2, Program.height / 2);

        }
    }
}
