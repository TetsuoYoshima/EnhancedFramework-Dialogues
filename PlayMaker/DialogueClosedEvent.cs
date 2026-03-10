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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="Dialogues.Dialogue"/> is being closed.
    /// </summary>
    [Tooltip("Sends an Event when a Dialogue is being closed")]
    [ActionCategory(CategoryName)]
    public sealed class DialogueClosedEvent : BaseDialogueFSM {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Dialogue used by the event")]
        [RequiredField, ObjectType(typeof(Dialogue))]
        public FsmObject Dialogue = null;

        [Tooltip("Event to send when the Dialogue is being closed")]
        public FsmEvent ClosedEvent;
        #endregion

        #region Behaviour
        private Action<Dialogue, DialoguePlayer> onClosedCallback = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            Dialogue = null;
            ClosedEvent  = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Dialogue.Value is Dialogue _dialogue) {

                onClosedCallback ??= OnClosed;
                _dialogue.OnClosed += onClosedCallback;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Dialogue.Value is Dialogue _dialogue) {
                _dialogue.OnClosed -= onClosedCallback;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnClosed(Dialogue _dialogue, DialoguePlayer _player) {
            Fsm.Event(ClosedEvent);
        }
        #endregion
    }
}
