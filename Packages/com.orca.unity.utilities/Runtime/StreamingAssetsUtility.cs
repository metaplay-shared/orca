#if UNITY_ANDROID && !UNITY_EDITOR
#define USE_WEBREQUEST
#endif

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#if USE_WEBREQUEST
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
#endif

namespace Orca.Unity.Utilities {
	public class StreamingAssetsUtility {
		public static string LoadTextFile(string relativePath) {
			#if USE_WEBREQUEST
			var request = CreateLoadFileRequest(relativePath);
			request.SendWebRequest();
			while (!request.isDone) { }
			return request.downloadHandler.text;
			#else
			var filePath = GetFilePath(relativePath);
			var result = File.ReadAllText(filePath);
			return result;
			#endif
		}

		public static Task<string> LoadTextFileAsync(string relativePath) {
			var tcs = new TaskCompletionSource<string>();

			try {
				#if USE_WEBREQUEST
				var request = CreateLoadFileRequest(relativePath);
				var sendRequest = request.SendWebRequest();
				var task = sendRequest.ToUniTask();
				task.ContinueWith(t => tcs.SetResult(t.downloadHandler.text));
				#else
				var filePath = GetFilePath(relativePath);
				var result = File.ReadAllText(filePath);
				tcs.SetResult(result);
				#endif
			} catch (Exception e) {
				tcs.SetException(e);
			}

			return tcs.Task;
		}

		private static UnityWebRequest CreateLoadFileRequest(string relativePath) {
			var filePath = GetFilePath(relativePath);
			var request = UnityWebRequest.Get(filePath);
			return request;
		}

		private static string GetFilePath(string relativePath) {
			return Path.Combine(Application.streamingAssetsPath, relativePath);
		}
	}
}
