using System;
using System.Numerics;
using OpenGL;
using MathHelp;
namespace BlockGameRenderer
{
    public class Camera
    {
        private Vector3 position;
        private Quaternion orientation;
        private Matrix4 viewMatrix; // cached view matrix
        private bool dirty = true;  // true if the viewMatrix must be recalculated

        public Matrix4 ViewMatrix
        {
            get
            {
                if (dirty)
                {
                    viewMatrix = Matrix4.CreateTranslation(-position) * QuarternionHelper.QuaternionToMatrix(orientation);
                    dirty = false;
                }
                return viewMatrix;
            }
        }
    
        public Camera(Vector3 position, Quaternion orientation)
        {
            this.position = position;
            this.orientation = orientation;
        }

        //sets the camera to a certain vector3 direction
        public void SetDirection(Vector3 direction)
        {
            if (direction == Vector3.Zero) return;

            Vector3 zvec = -direction.Normalize();
            Vector3 xvec = Vector3.Cross(Vector3.UnitY, zvec).Normalize();
            Vector3 yvec = Vector3.Cross(zvec, xvec).Normalize();
            orientation = QuarternionHelper.FromCrossProducts(xvec, yvec, zvec);
        }

        
        // Moves the camera 
        public void Move(Vector3 by)
        {
            position += by;
        }

        
        //moves the camera relative to where the camera is facing
       
        public void MoveRelative(Vector3 by)
        {
            position += MathHelp.Vector3Helper.QuaternionToNormalizedVector3(orientation) * by;
        }

        
        public void Rotate(Quaternion rotation)
        {
            orientation = rotation * orientation;
        }

        
        public void Rotate(float angle, Vector3 axis)
        {
            Rotate(Quaternion.CreateFromAxisAngle (axis, angle));
        }

        
        public void Roll(float angle)
        {
            Vector3 axis = Vector3Helper.QuaternionToNormalizedVector3(orientation) * Vector3.UnitZ;
            Rotate(angle, axis);
        }
       
        public void Yaw(float angle)
        {
            
            // for an unfixed yaw replace Vector3.Up with (orientation * Vector3.UnitY)
            Rotate(angle, Vector3.UnitY);
        }

        
        public void Pitch(float angle)
        {
            Vector3 axis = Vector3Helper.QuaternionToNormalizedVector3(orientation) * Vector3.UnitX;
            Rotate(angle, axis);
        }
    }
}
