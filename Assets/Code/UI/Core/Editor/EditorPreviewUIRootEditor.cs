using UnityEditor;

namespace Code.UI.Core.Editor {
	[CustomEditor(typeof(EditorPreviewUIRoot))]
	public class EditorPreviewUIRootEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			EditorGUILayout.HelpBox(
				"Replace this component with a proper UI Root Behaviour before shipping it with the game.",
				MessageType.Warning
			);
		}
	}
}
