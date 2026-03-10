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
    /// Plays a <see cref="DialogueNode"/> during this clip.
    /// </summary>
    [DisplayName(NamePrefix + "Play Dialogue Node")]
    public sealed class PlayDialogueNodeClip : DialoguePlayableAsset<PlayDialogueNodeBehaviour> {
        #region Utility
        public override string ClipDefaultName {
            get { return "Play Dialogue Node"; }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="PlayDialogueNodeClip"/>-related <see cref="PlayableBehaviour"/>.
    /// </summary>
    [Serializable]
    public sealed class PlayDialogueNodeBehaviour : EnhancedPlayableBehaviour<Dialogue> {
        #region Global Members
        [Tooltip("If true, plays the next node from the active dialogue")]
        public bool PlayNext = true;

        [Tooltip("ID of the node to play")]
        [Enhanced, ShowIf(nameof(PlayNext), ConditionType.False)] public int NodeGUID = 0;

        // -----------------------

        protected override bool CanExecuteInEditMode {
            get { return false; }
        }
        #endregion

        #region Behaviour
        protected override void OnPlay(Playable _playable, FrameData _info) {
            base.OnPlay(_playable, _info);

            if (bindingObject.IsNull()) {
                return;
            }

            // Play node.
            if (bindingObject.GetPlayer(out DialoguePlayer _player)) {

                if (PlayNext) {
                    _player.PlayNextNode();
                    return;

                } else if (bindingObject.FindNode(NodeGUID, out DialogueNode _node)) {
                    _player.PlayNode(_node);
                    return;
                }
            }

            // Failed.
            bindingObject.LogWarningMessage("Node could not be played");
        }
        #endregion
    }
}
