using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utilities
{
	public static class TextureUtilities
	{
		private delegate void GetWidthAndHeight(TextureImporter importer, ref int width, ref int height);

		private static GetWidthAndHeight getWidthAndHeightDelegate;

		public struct Size2DInt
		{
			public int width;
			public int height;
		}

		public static Size2DInt GetOriginalTextureSize(Texture texture) //changed form Texture2D
		{
			if (texture == null)
				throw new NullReferenceException();

			string path = AssetDatabase.GetAssetPath(texture);
			if (string.IsNullOrEmpty(path))
				throw new Exception("Texture is not an asset texture.");

			TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
			if (importer == null)
				throw new Exception("Failed to get Texture importer for " + path);

			return GetOriginalTextureSize(importer);
		}

		public static Size2DInt GetOriginalTextureSize(TextureImporter importer)
		{
			if (getWidthAndHeightDelegate == null)
			{
				MethodInfo method = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
					BindingFlags.NonPublic | BindingFlags.Instance);
				getWidthAndHeightDelegate =
					Delegate.CreateDelegate(typeof(GetWidthAndHeight), null, method) as GetWidthAndHeight;
			}

			var size = new Size2DInt();
			getWidthAndHeightDelegate(importer, ref size.width, ref size.height);

			return size;
		}
	}
}
