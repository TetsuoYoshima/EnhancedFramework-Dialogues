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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="Dialogues.Dialogue"/> is being played.
    /// </summary>
    [Tooltip("Sends an Event when a Dialogue starts being played")]
    [ActionCategory(CategoryName)]
    public sealed class DialoguePlayedEvent : BaseDialogueFSM {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Dialogue used by the event.")]
        [RequiredField, ObjectType(typeof(Dialogue))]
        public FsmObject Dialogue = null;

        [Tooltip("Event to send when the Dialogue starts being played.")]
        public FsmEvent PlayedEvent;
        #endregion

        #region Behaviour
        private Action<Dialogue, DialoguePlayer> onPlayedCallback = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            Dialogue = null;
            PlayedEvent  = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Dialogue.Value is Dialogue _dialogue) {

                onPlayedCallback ??= OnPlayed;
                _dialogue.OnPlayed += onPlayedCallback;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Dialogue.Value is Dialogue _dialogue) {
                _dialogue.OnPlayed -= onPlayedCallback;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnPlayed(Dialogue _dialogue, DialoguePlayer _player) {
            Fsm.Event(PlayedEvent);
        }
        #endregion
    }
}
