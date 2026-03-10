// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using HutongGames.PlayMaker;
using System;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.Dialogues.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to play a <see cref="Dialogues.Dialogue"/>.
    /// </summary>
    [Tooltip("Plays a Dialogue")]
    [ActionCategory(CategoryName)]
    public sealed class DialoguePlay : BaseDialogueFSM {
        #region Global Members
        // -------------------------------------------
        // Variable - Closed
        // -------------------------------------------

        [Tooltip("The Dialogue to play.")]
        [RequiredField, ObjectType(typeof(DialogueBehaviour))]
        public FsmObject Dialogue = null;

        [Tooltip("Event to send when the Dialogue is closed.")]
        public FsmEvent ClosedEvent;
        #endregion

        #region Behaviour
        private Action<Dialogue, DialoguePlayer> onClosedCallback = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            Dialogue = null;
            ClosedEvent = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (GetDialogue(out Dialogue _dialogue)) {

                onClosedCallback ??= OnClosed;

                _dialogue.OnClosed += onClosedCallback;
                _dialogue.CreatePlayer();
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (GetDialogue(out Dialogue _dialogue)) {
                _dialogue.OnClosed -= onClosedCallback;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private bool GetDialogue(out Dialogue _dialogue) {

            if (Dialogue.Value is DialogueBehaviour _behaviour) {
                _dialogue = _behaviour.Dialogue;
                return true;
            }

            _dialogue = null;
            return false;
        }

        private void OnClosed(Dialogue _dialogue, DialoguePlayer _player) {
            Fsm.Event(ClosedEvent);
        }
        #endregion
    }
}
