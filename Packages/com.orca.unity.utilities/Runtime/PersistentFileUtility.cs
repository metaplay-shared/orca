using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orca.Unity.Utilities {
	public class PersistentFileUtility {
		public static string LoadFile(string relativePath) {
			var filePath = GetFilePath(relativePath);
			var result = File.ReadAllText(filePath);
			return result;
		}

		public static async Task<string> LoadFileAsync(string relativePath) {
			var filePath = GetFilePath(relativePath);
			byte[] result;
			using (FileStream stream = File.Open(filePath, FileMode.Open)) {
				result = new byte[stream.Length];
				await stream.ReadAsync(result, 0, (int) stream.Length);
			}
			return Encoding.ASCII.GetString(result);
		}

		public static async Task SaveFileAsync(byte[] content, string filePath) {
			var path = GetFilePath(filePath);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using (FileStream stream = File.OpenWrite(path)) {
				// Make sure the file is empty
				stream.SetLength(0);
				await stream.WriteAsync(content, 0, content.Length);
			}
		}

		public static async Task SaveFileAsync(string content, string filePath) {
			var path = GetFilePath(filePath);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using (FileStream stream = File.OpenWrite(path)) {
				// Make sure the file is empty
				stream.SetLength(0);
				var bytes = Encoding.ASCII.GetBytes(content);
				await stream.WriteAsync(bytes, 0, bytes.Length);
			}
		}

		public static bool ContainsFile(string filePath) {
			var path = GetFilePath(filePath);
			return File.Exists(path);
		}
		
		public static string GetFilePath(string relativePath) {
			return Path.Combine(Application.persistentDataPath, relativePath);
		}

		public static string GetDirectory() {
			return Application.persistentDataPath;
		}
	}
}
