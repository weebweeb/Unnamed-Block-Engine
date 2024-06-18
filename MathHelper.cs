using System;
using Tao.FreeGlut;
using System.Numerics;
using OpenGL;

namespace MathHelp
{
    
    public class QuarternionHelper
    {
        // Function to create a quaternion from three cross product vectors

        public static float DegreesToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180.0f);
        }

        public static Quaternion FromCrossProducts(Vector3 u, Vector3 v, Vector3 w)
        {
            u = Vector3.Normalize(u);
            v = Vector3.Normalize(v);
            w = Vector3.Normalize(w);


            Matrix4 rotationMatrix = new Matrix4(new float[] {
                u.X, u.Y, u.Z, 0,
                v.X, v.Y, v.Z, 0,
                w.X, w.Y, w.Z, 0,
                0, 0, 0, 1 }
            );

            // Convert the rotation matrix to a quaternion
            Quaternion rotationQuaternion = MatrixToQuaternion(rotationMatrix);

            return rotationQuaternion;
        }

        // Function to convert a rotation matrix to a quaternion
        public static Quaternion MatrixToQuaternion(Matrix4 m)
        {
            Quaternion q = new Quaternion();
            float trace = m[0, 0] + m[1, 1] + m[2, 2];
            if (trace > 0)
            {
                float s = 0.5f / MathF.Sqrt(trace + 1.0f);
                q.W = 0.25f / s;
                q.X = (m[1, 2] - m[2, 1]) * s;
                q.Y = (m[2, 0] - m[0, 2]) * s;
                q.Z = (m[0, 1] - m[1, 0]) * s;
            }
            else
            {
                if (m[0, 0] > m[1, 1] && m[0, 0] > m[2, 2])
                {
                    float s = 2.0f * MathF.Sqrt(1.0f + m[0, 0] - m[1, 1] - m[2, 2]);
                    q.W = (m[1, 2] - m[2, 1]) / s;
                    q.X = 0.25f * s;
                    q.Y = (m[0, 1] + m[1, 0]) / s;
                    q.Z = (m[0, 2] + m[2, 0]) / s;
                }
                else if (m[1, 1] > m[2, 2])
                {
                    float s = 2.0f * MathF.Sqrt(1.0f + m[1, 1] - m[0, 0] - m[2, 2]);
                    q.W = (m[2, 0] - m[0, 2]) / s;
                    q.X = (m[0, 1] + m[1, 0]) / s;
                    q.Y = 0.25f * s;
                    q.Z = (m[1, 2] + m[2, 1]) / s;
                }
                else
                {
                    float s = 2.0f * MathF.Sqrt(1.0f + m[2, 2] - m[0, 0] - m[1, 1]);
                    q.W = (m[0, 1] - m[1, 0]) / s;
                    q.X = (m[0, 2] + m[2, 0]) / s;
                    q.Y = (m[1, 2] + m[2, 1]) / s;
                    q.Z = 0.25f * s;
                }
            }
            return q;
        }

        // Function to convert a quaternion to a rotation matrix
        public static Matrix4 QuaternionToMatrix(Quaternion q)
        {
            float xx = q.X * q.X;
            float yy = q.Y * q.Y;
            float zz = q.Z * q.Z;
            float xy = q.X * q.Y;
            float xz = q.X * q.Z;
            float yz = q.Y * q.Z;
            float wx = q.W * q.X;
            float wy = q.W * q.Y;
            float wz = q.W * q.Z;

            Matrix4 m = new Matrix4(new float[] {
                1.0f - 2.0f * (yy + zz), 2.0f * (xy - wz), 2.0f * (xz + wy), 0.0f,
                2.0f * (xy + wz), 1.0f - 2.0f * (xx + zz), 2.0f * (yz - wx), 0.0f,
                2.0f * (xz - wy), 2.0f * (yz + wx), 1.0f - 2.0f * (xx + yy), 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f }
            );

            return m;
        }

    }
    
    public class Vector3Helper
    {


        public static Vector3 ConvertAngleToNormalizedVector(float angleInDegrees)
        {
            // Convert angle from degrees to radians
            float angleInRadians = MathF.PI / 180.0f * angleInDegrees;

            // Calculate the x and y components of the vector
            float x = MathF.Cos(angleInRadians);
            float y = MathF.Sin(angleInRadians);

            // The z component is 0 since the angle is in the XY plane
            float z = 0.0f;

            // Create the vector
            Vector3 vector = new Vector3(x, y, z);

            // Normalize the vector
            vector = Vector3.Normalize(vector);

            return vector;
        }
        public static Vector3 QuaternionToNormalizedVector3(Quaternion q, Vector3 forward)
        {

            //q = Quaternion.Normalize(q);


            // Rotate our forward direction by the quaternion
            Vector3 rotatedForward = RotateVectorByQuaternion(forward, q);
            
            rotatedForward = Vector3.Normalize(rotatedForward);

            return rotatedForward;
        }

        private static Vector3 RotateVectorByQuaternion(Vector3 v, Quaternion q)
        {
            // Quaternion multiplication: q * v * q^-1
            Quaternion qv = new Quaternion(v.X, v.Y, v.Z, 0);
            Quaternion qConjugate = Quaternion.Conjugate(q);

            Quaternion result = q * qv * qConjugate;
            



            return new Vector3(result.X, result.Y, result.Z);
        }

    }
}
