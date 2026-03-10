// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using System;
using System.Collections.Generic;

namespace EnhancedFramework.Dialogues {
    /// <summary>
    /// <see cref="DialogueEvent"/> with an already implemented delay.
    /// </summary>
    [Serializable]
    public abstract class DelayedDialogueEvent : DialogueEvent {
        #region Global Members
        /// <summary>
        /// Delay before playing this event, in second(s).
        /// </summary>
        public virtual float Delay {
            get { return 0f; }
        }

        public override bool IsPlaying {
            get { return playDelay > 0f; }
        }
        #endregion

        #region Behaviour
        [NonSerialized] private float playDelay = 0f;

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        protected override sealed bool OnPlay(DialoguePlayer _player, List<DialogueEvent> _playingEvents) {
            ref float _delay = ref playDelay;
            _delay = Delay;

            // Instant.
            if (_delay <= 0f) {
                OnPlayed(_player);
            }

            return true; // Auto register.
        }

        internal protected override bool Update(DialoguePlayer _player) {
            // Delay.
            ref float _delay = ref playDelay;
            if (_delay > 0f) {

                _delay -= _player.DeltaTime;
                if (_delay > 0f)
                    return true;

                // Complete.
                OnPlayed(_player);
            }

            return false;
        }

        protected override bool OnStop(DialoguePlayer _player, bool _isClosingDialogue) {
            // Complete delay.
            ref float _delay = ref playDelay;
            if (_delay > 0f) {

                _delay = 0f;
                OnPlayed(_player);
            }

            return true;
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <inheritdoc cref="OnPlay"/>
        protected abstract void OnPlayed(DialoguePlayer _player);
        #endregion
    }
}
