using System;
using UnityEngine;

namespace Utilities
{
    public class BitConverterUtilities
    {
        public static byte[] VectorToByte(Vector3 vect)
        {
            byte[] buff = new byte[sizeof(float) * 3];
            Buffer.BlockCopy(BitConverter.GetBytes(vect.x), 0, buff, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(vect.y), 0, buff, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(vect.z), 0, buff, 2 * sizeof(float), sizeof(float));
            return buff;
        }

        public static Vector3 ByteToVector(byte[] bytes)
        {
            Vector3 vect = Vector3.zero;
            vect.x = BitConverter.ToSingle(bytes, 0 * sizeof(float));
            vect.y = BitConverter.ToSingle(bytes, 1 * sizeof(float));
            vect.z = BitConverter.ToSingle(bytes, 2 * sizeof(float));

            return vect;
        }

        public static byte[] QuaternionToByte(Quaternion quat)
        {
            byte[] buff = new byte[sizeof(float) * 4];
            Buffer.BlockCopy(BitConverter.GetBytes(quat.x), 0, buff, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(quat.y), 0, buff, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(quat.z), 0, buff, 2 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(quat.w), 0, buff, 3 * sizeof(float), sizeof(float));
            return buff;
        }

        public static Quaternion ByteToQuaternion(byte[] bytes)
        {
            Quaternion quat = new Quaternion();
            quat.x = BitConverter.ToSingle(bytes, 0 * sizeof(float));
            quat.y = BitConverter.ToSingle(bytes, 1 * sizeof(float));
            quat.z = BitConverter.ToSingle(bytes, 2 * sizeof(float));
            quat.w = BitConverter.ToSingle(bytes, 3 * sizeof(float));

            return quat;
        }
    }
}