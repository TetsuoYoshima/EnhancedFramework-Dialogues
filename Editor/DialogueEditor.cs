// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedEditor.Editor;
using UnityEditor;

namespace EnhancedFramework.Dialogues.Editor {
    /// <summary>
    /// Custom <see cref="Dialogue"/> editor, used to draw various parameters from the <see cref="DialogueEditorWindow"/>.
    /// </summary>
    [CustomEditor(typeof(Dialogue), true), CanEditMultipleObjects]
    public sealed class DialogueEditor : UnityObjectEditor {
        #region Editor Content
        public override void OnInspectorGUI() {
            serializedObject.UpdateIfRequiredOrScript();

            using (var _scope = new EditorGUI.ChangeCheckScope()) {

                if (!DialogueEditorWindow.DrawInspector(target as Dialogue)) {
                    base.OnInspectorGUI();
                }

                if (_scope.changed) {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
        #endregion
    }
}
