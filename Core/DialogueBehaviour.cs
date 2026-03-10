// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
// 
// ============================================================================================= //

#if LOCALIZATION_PACKAGE
#define LOCALIZATION_ENABLED
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
#endif

namespace EnhancedFramework.Dialogues {
    /// <summary>
    /// <see cref="Component"/> wrapper for a <see cref="Dialogues.Dialogue"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Dialogue/Dialogue")]
    public sealed class DialogueBehaviour : EnhancedBehaviour
                                          #if LOCALIZATION_ENABLED
                                          , IResourceBehaviour<LocalizationResourceLoader>
                                          #endif
    {
        #region Global Members
        [Section("Dialogue")]

        [Tooltip("The dialogue associated with this component")]
        [SerializeField, Enhanced, Required] private Dialogue dialogue = null;

        // -----------------------

        /// <summary>
        /// The <see cref="Dialogues.Dialogue"/> associated with this component.
        /// </summary>
        public Dialogue Dialogue {
            get { return dialogue; }
        }
        #endregion

        #region Operator
        public static implicit operator Dialogue(DialogueBehaviour _behaviour) {
            return _behaviour.Dialogue;
        }
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        void IResourceBehaviour<LocalizationResourceLoader>.FillResource(LocalizationResourceLoader _resource) {
            // Fill dialogue tables for preload.
            _resource.FillTables(dialogue);
        }
        #endif
        #endregion
    }
}
