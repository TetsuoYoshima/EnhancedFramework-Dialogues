// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedEditor;
using EnhancedFramework.Timeline;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.Dialogues.Timeline {
    /// <summary>
    /// Plays a <see cref="Dialogue"/> for the duration of the clip.
    /// </summary>
    [DisplayName(NamePrefix + "Play Dialogue")]
    public sealed class PlayDialogueClip : DialoguePlayableAsset<PlayDialogueBehaviour> {
        #region Utility
        public override string ClipDefaultName {
            get { return "Dialogue"; }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="PlayDialogueClip"/>-related <see cref="PlayableBehaviour"/>.
    /// </summary>
    [Serializable]
    public sealed class PlayDialogueBehaviour : EnhancedPlayableBehaviour<Dialogue> {
        #region Global Members
        [Tooltip("If true, automatically closes the dialogue on exit")]
        public bool AutoClose = true;

        // -----------------------

        protected override bool CanExecuteInEditMode {
            get { return false; }
        }
        #endregion

        #region Behaviour
        private DialoguePlayer player = null;

        // -----------------------

        protected override void OnPlay(Playable _playable, FrameData _info) {
            base.OnPlay(_playable, _info);

            Dialogue _bindingObject = bindingObject;
            if (_bindingObject.IsNull()) {
                return;
            }

            // Play.
            player = _bindingObject.CreatePlayer();
        }

        protected override void OnStop(Playable _playable, FrameData _info, bool _completed) {
            base.OnStop(_playable, _info, _completed);

            // Close.
            if (AutoClose && (player != null)) {
                player.Close();
            }
        }
        #endregion
    }
}
