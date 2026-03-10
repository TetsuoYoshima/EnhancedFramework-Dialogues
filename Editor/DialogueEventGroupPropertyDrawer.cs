// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using UnityEditor;
using UnityEngine;

namespace EnhancedFramework.Dialogues.Editor {
    /// <summary>
    /// Custom <see cref="DialogueEventGroup{T}"/> drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(DialogueEventGroup<>), true)]
    public sealed class DialogueEventGroupPropertyDrawer : DialogueNodePropertyDrawer {
        #region Drawer Content
        protected override float OnEnhancedGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            SerializedProperty _eventProperty = _property.FindPropertyRelative("events");
            float _height = EditorGUI.GetPropertyHeight(_eventProperty);
            _position.height = _height;

            EditorGUI.PropertyField(_position, _eventProperty, _label, true);
            return _height;
        }
        #endregion
    }
}
