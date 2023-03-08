using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Utilities
{
    public static class Code
    {
        public static string GenerateGuid() => Guid.NewGuid().ToString().ToLowerInvariant();

        public static List<T> EmptyList<T>(int count) where T : class
        {
            return CreateList<T>(count, () => null);
        }

        public static List<T> CreateList<T>(int count) where T : class, new()
        {
            return CreateList(count, () => new T());
        }

        public static List<T> CreateList<T>(int count, T defaultValue) where T : struct
        {
            return CreateList(count, () => defaultValue);
        }

        public static List<T> CreateList<T>(int count, Func<T> constructor)
        {
            var list = new List<T>(count);
            for (int i = 0; i < count; ++i)
            {
                list.Add(constructor());
            }
            return list;
        }

        public static List<T> GetEnumValues<T>() => (Enum.GetValues(typeof(T)) as T[]).ToList();
        public static int GetEnumCount<T>() => GetEnumValues<T>().Count;

        public static Vector3 PointOnCircleDegrees(float degrees, float r)
        {
            return PointOnCircleRadians(degrees.ToRadians(), r);
        }

        public static Vector3 PointOnCircleRadians(float radians, float r)
        {
            return new Vector2(Mathf.Sin(radians) * r, Mathf.Cos(radians) * r);
        }

        public static int NextPowerOfTwo(int x)
        {
            x--; // comment out to always take the next biggest power of two, even if x is already a power of two
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (x + 1);
        }

        public static void Run(string path) => Process.Start(path);

        public static void RunCommand(string command, string args = null)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", $"/C {command} {args}")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }

        public static bool RunProcess(string path, string args = null)
        {
            if (false == File.Exists(path))
            {
                return false;
            }

            //Create a process to run the newly created batch file
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(path),
                FileName = string.Concat("\"", path, "\""),

                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                Verb = "runas"
            };

            if (args != null)
            {
                string escapedArgs = string.Format("\"{0}\"", args);
                startInfo.Arguments = escapedArgs;
            }

            var process = new Process { StartInfo = startInfo };

            try
            {
                process.Start();

                UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Process error: " + e.Message);
                return false;
            }

            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError("CMD ERROR: " + path);
                return false;
            }

            return true;
        }
    }
}
