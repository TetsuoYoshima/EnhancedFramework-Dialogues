// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedFramework.Dialogues {
    // ===== Base Event ===== \\
    
    /// <summary>
    /// Base class to derive any <see cref="Dialogue"/>-related event from.
    /// <br/> Designed to be played and stopped from any node.
    /// </summary>
    [Serializable]
    public abstract class DialogueEvent {
        #region Global Members
        /// <summary>
        /// Whether this event can be played or not.
        /// </summary>
        public abstract bool IsAvailable { get; }

        /// <summary>
        /// Indicates if this event is currently playing.
        /// </summary>
        public abstract bool IsPlaying { get; }
        #endregion

        #region Behaviour
        /// <summary>
        /// Plays this event.
        /// </summary>
        /// <param name="_player">The <see cref="DialoguePlayer"/> associated with this event.</param>
        /// <param name="_playingEvents">Buffer where to register all playing events.</param>
        /// <returns>True if this event could be successfully played and should be automatically registered, false otherwise.</returns>
        internal bool Play(DialoguePlayer _player, List<DialogueEvent> _playingEvents) {
            if (!IsAvailable)
                return false;

            if (!OnPlay(_player, _playingEvents))
                return false;

            _playingEvents.Add(this);
            return true;
        }

        /// <summary>
        /// Updates this event.
        /// </summary>
        /// <param name="_player">The <see cref="DialoguePlayer"/> associated with this event.</param>
        /// <returns>True if this event is still processing, false if it can be automatically unregister.</returns>
        internal protected virtual bool Update(DialoguePlayer _player) {
            return false;
        }

        /// <summary>
        /// Stops from playing this event.
        /// </summary>
        /// <param name="_player">The <see cref="DialoguePlayer"/> associated with this event.</param>
        /// <param name="_isClosingDialogue">Indicates if the associated dialogue is being closed, or not.</param>
        /// <returns>True if this event was successfully stopped, false if it requires some time for processing operations.</returns>
        internal bool Stop(DialoguePlayer _player, bool _isClosingDialogue) {
            if (!IsAvailable)
                return true;

            if (OnStop(_player, _isClosingDialogue)) {
                _player.OnEventStopped(this);
                return true;
            }

            return false;
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        /// <inheritdoc cref="Play"/>
        protected abstract bool OnPlay(DialoguePlayer _player, List<DialogueEvent> _playingEvents);

        /// <inheritdoc cref="Stop"/>
        protected virtual bool OnStop(DialoguePlayer _player, bool _isClosingDialogue) {
            return true;
        }
        #endregion
    }

    // ===== Group ===== \\

    /// <summary>
    /// <see cref="DialogueEvent"/> array wrapper.
    /// </summary>
    /// <typeparam name="T"><see cref="DialogueEvent"/> type contained in this group.</typeparam>
    [Serializable]
    public sealed class DialogueEventGroup<T> : DialogueEvent where T : DialogueEvent {
        #region Global Members
        [Tooltip("All events contained in this group")]
        [SerializeField, DisplayName(nameof(Name), true)] private BlockArray<T> events = new BlockArray<T>();

        // -----------------------

        #if UNITY_EDITOR
        /// <summary>
        /// Cached value of this event display name (editor only).
        /// </summary>
        [NonSerialized] private string displayName = null;
        #endif

        /// <summary>
        /// Displayed name of this group.
        /// </summary>
        public string Name {
            get {
                #if UNITY_EDITOR
                ref string _name = ref displayName;

                if (_name == null) {
                    Type _type = typeof(T);
                    DisplayNameAttribute _attribute = _type.GetCustomAttribute<DisplayNameAttribute>();

                    if (_attribute != null) {
                        _name = _attribute.Label.text;
                    } else {
                        _name = _type.Name.Replace(typeof(DialogueEvent).Name, string.Empty);
                    }

                    _name = ObjectNames.NicifyVariableName(_name);
                }

                return _name;
                #else
                return typeof(DialogueEvent).Name;
                #endif
            }
        }

        // -----------------------

        public override bool IsAvailable {
            get { return events.Count != 0; }
        }

        public override bool IsPlaying {
            get {
                ref T[] _span = ref events.Array;
                for (int i = _span.Length; i-- > 0;) {
                    if (_span[i].IsPlaying) {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        #region Behaviour
        protected override bool OnPlay(DialoguePlayer _player, List<DialogueEvent> _playingEvents) {
            ref T[] _span = ref events.Array;
            int    _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].Play(_player, _playingEvents);
            }

            return false; // Do not register this event.
        }

        // -------------------------------------------
        // /!\ Should Not Be Called /!\
        // -------------------------------------------

        internal protected override bool Update(DialoguePlayer _player) {
            #if DEVELOPMENT
            // Should not be called, as this event should not be registered.
            this.LogWarningMessage("Updating dialogue event group - this should not happen");
            #endif

            ref T[] _span = ref events.Array;
            int    _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].Update(_player);
            }

            return true;
        }

        protected override bool OnStop(DialoguePlayer _player, bool _isClosingDialogue) {
            #if DEVELOPMENT
            // Should not be called, as this event should not be registered.
            this.LogWarningMessage("Stopping dialogue event group - this should not happen");
            #endif

            ref T[] _span = ref events.Array;
            int    _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].Stop(_player, _isClosingDialogue);
            }

            return true;
        }
        #endregion
    }
}
