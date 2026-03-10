// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.Dialogues.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to close a <see cref="Dialogues.Dialogue"/>.
    /// </summary>
    [Tooltip("Closes a Dialogue")]
    [ActionCategory(CategoryName)]
    public sealed class DialogueClose : BaseDialogueFSM {
        #region Global Members
        // -------------------------------------------
        // Variable
        // -------------------------------------------

        [Tooltip("The Dialogue to close")]
        [RequiredField, ObjectType(typeof(DialogueBehaviour))]
        public FsmObject Dialogue = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Dialogue = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Dialogue.Value is DialogueBehaviour _behaviour) {
                _behaviour.Dialogue.ClosePlayer();
            }

            Finish();
        }
        #endregion
    }
}
