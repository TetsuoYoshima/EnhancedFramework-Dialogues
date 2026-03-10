// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("EnhancedFramework.Dialogues.Editor")]
namespace EnhancedFramework.Dialogues {
    /// <summary>
    /// Contains multiple <see cref="DialogueNode"/>-related utilties,
    /// making connections between the editor and the runtime system.
    /// </summary>
    public static class DialogueNodeUtility {
        #region Content
        #if UNITY_EDITOR
        [SerializeReference] internal static DialogueNode copyBuffer = null;
        #endif

        /// <summary>
        /// Clipboard buffer for a <see cref="DialogueNode"/> reference (editor only).
        /// </summary>
        public static DialogueNode CopyBuffer {
            get {
                #if UNITY_EDITOR
                return copyBuffer;
                #else
                return null;
                #endif
            }
        }
        #endregion
    }
}
