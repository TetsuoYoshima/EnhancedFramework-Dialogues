// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;

namespace EnhancedFramework.Dialogues {
    /// <summary>
    /// Base class used to play a <see cref="Dialogue.Dialogue"/>.
    /// <br/>Non-generic global version of <see cref="DialoguePlayer{T}"/>.
    /// <para/>
    /// Should never be directly inherited from, always prefer using <see cref="DialoguePlayer{T}"/> instead.
    /// </summary>
    [Serializable]
    public abstract class DialoguePlayer : IPoolableObject {
        #region Global Members
        /// <summary>
        /// The currently playing <see cref="Dialogue.Dialogue"/>.
        /// </summary>
        public Dialogue Dialogue = null;

        /// <summary>
        /// The currently playing <see cref="DialogueNode"/>.
        /// </summary>
        public DialogueNode CurrentNode = null;

        /// <summary>
        /// Whether this player is currently active, playing a dialogue, or not.
        /// </summary>
        public bool IsPlaying { get; private set; } = false;

        /// <summary>
        /// Name of this player associated <see cref="Dialogues.Dialogue"/>.
        /// </summary>
        public string Name {
            get { return Dialogue.name; }
        }

        /// <summary>
        /// Delta time used for time-related operations within this player.
        /// </summary>
        public virtual float DeltaTime {
            get { return ChronosManager.Instance.DeltaTime; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents inheriting from this class in other assemblies.
        /// </summary>
        private protected DialoguePlayer() { }
        #endregion

        #region Activation
        private Action onClosedCallback = null;
        private Action onClosed         = null;

        // -----------------------

        /// <inheritdoc cref="Setup(Dialogue, DialogueNode)"/>
        public void Setup(Dialogue _dialogue) {
            Setup(_dialogue, _dialogue.Root);
        }

        /// <summary>
        /// Setups this player with the <see cref="Dialogues.Dialogue"/> to play.
        /// </summary>
        /// <param name="_dialogue">The <see cref="Dialogues.Dialogue"/> to play</param>
        /// <param name="_currentNode">The first <see cref="DialogueNode"/> to play.</param>
        public virtual void Setup(Dialogue _dialogue, DialogueNode _currentNode) {
            Dialogue    = _dialogue;
            CurrentNode = _currentNode;

            IsPlaying = true;
            OnSetup();
        }

        /// <summary>
        /// Updates this <see cref="DialoguePlayer"/>.
        /// <br/> Required for updating events.
        /// </summary>
        public virtual void Update() {
            UpdateEvents();
        }

        /// <summary>
        /// Stop playing the dialogue.
        /// </summary>
        /// <param name="_onNodeQuit">Delegate to be called once the current node was quit.</param>
        public void Close(Action _onNodeQuit = null) {

            onClosedCallback = _onNodeQuit;

            // Inactive already.
            if (!IsPlaying) {
                OnClosed();
                return;
            }

            // Close.
            IsPlaying = false;

            onClosed ??= OnClosed;
            OnClose(onClosed);
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        /// <summary>
        /// Called once this player is setup.
        /// <para/>
        /// By default, plays the first node of this player.
        /// <br/> Use this to update the game current state and interface.
        /// </summary>
        protected virtual void OnSetup() {
            if (CurrentNode.IsAvailable) {
                PlayCurrentNode();
            } else {
                Close();
            }
        }

        /// <summary>
        /// Called when this player is being closed.
        /// <para/>
        /// By default, quits the current playing node.
        /// <br/> Use this to update the game current state and interface.
        /// </summary>
        /// <param name="_onNodeQuit">Delegate to be called once the current node was quit.</param>
        protected virtual void OnClose(Action _onNodeQuit = null) {
            CurrentNode.Quit(this, true, _onNodeQuit);
        }

        /// <summary>
        /// Called when this player is finally closed.
        /// </summary>
        protected virtual void OnClosed() {
            onClosedCallback?.Invoke();
            Dialogue.OnPlayerClosed(this);
        }
        #endregion

        #region Behaviour
        private Action playCurrentNodeCallback = null;

        // -----------------------

        /// <summary>
        /// Replays the current node from the start.
        /// </summary>
        public virtual void ReplayCurrentNode() {
            PlayNode(CurrentNode);
        }

        /// <summary>
        /// Quits the current node and play another one.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        /// <param name="_node">The next <see cref="DialogueNode"/> to play.</param>
        /// <param name="_autoPlay">Whether to automatically play this node or simply mark it as the next one to play.</param>
        public virtual void PlayNode(DialogueNode _node, bool _autoPlay = true) {
            ref DialogueNode _currentNode = ref CurrentNode;

            DialogueNode _previous = _currentNode;
            _currentNode = _node;

            Action _onQuit;

            if (_autoPlay) {
                playCurrentNodeCallback ??= PlayCurrentNode;
                _onQuit = playCurrentNodeCallback;
            } else {
                _onQuit = null;
            }

            _previous.Quit(this, false, _onQuit);
        }

        /// <summary>
        /// Plays the next <see cref="DialogueNode"/>, based on the current one.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        public virtual void PlayNextNode(bool _autoPlay = true) {

            // If there is no other node to play, finish playing this dialogue.
            if (!GetNextNode(out DialogueNode _next)) {
                Close();
                return;
            }

            PlayNode(_next, _autoPlay);
        }

        /// <summary>
        /// Plays this player <see cref="CurrentNode"/>.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        public virtual void PlayCurrentNode() {
            ref DialogueNode _currentNode = ref  CurrentNode;

            // Skip of disabled.
            if (!_currentNode.enabled) {
                PlayNextNode();
                return;
            }

            _currentNode.Play(this);
        }

        // -----------------------

        /// <summary>
        /// Get the next <see cref="DialogueNode"/> to be played.
        /// </summary>
        /// <param name="_next">The next <see cref="DialogueNode"/> to play.</param>
        /// <returns>True if a new node to play was successfully found, false otherwise.</returns>
        public abstract bool GetNextNode(out DialogueNode _next);

        /// <param name="_behaviour">Behaviour used to get the next node.</param>
        /// <param name="_node">Current node to get the next node from.</param>
        /// <inheritdoc cref="GetNextNode(out DialogueNode))"/>
        public virtual bool GetNextNode(NextNodeBehaviour _behaviour, DialogueNode _node, out DialogueNode _next) {

            ref DialogueNode[] _span = ref _node.GetNodeRefs();
            int _count = _span.Length;

            switch (_behaviour) {

                // Get the first available node.
                case NextNodeBehaviour.PlayFirst:
                    for (int i = 0; i < _count; i++) {
                        _next = _span[i];

                        if (_next.IsAvailable) {
                            return true;
                        }
                    }
                    break;

                // Get the last available node.
                case NextNodeBehaviour.PlayLast:
                    for (int i = _count; i-- > 0;) {
                        _next = _span[i];

                        if (_next.IsAvailable) {
                            return true;
                        }
                    }
                    break;

                // Play a random node.
                case NextNodeBehaviour.Random:
                    int _totalCount = 0;

                    for (int i = _count; i-- > 0;) {
                        if (_span[i].IsAvailable) {
                            _totalCount++;
                        }
                    }

                    if (_totalCount != 0) {

                        // Random.
                        int _random;
                        if (_totalCount == 1) {

                            _random = 0;

                        } else {

                            _random = Mathm.RandomNoRepeat(0, _totalCount, Dialogue.lastRandomIndex);
                            Dialogue.lastRandomIndex = _random;
                        }

                        // Get.
                        for (int i = _count; i-- > 0;) {
                            _next = _span[i];

                            if (_next.IsAvailable) {
                                if (_random == 0) {
                                    return true;
                                }

                                _random--;
                            }
                        }
                    }
                    break;

                case NextNodeBehaviour.None:
                default:
                    break;
            }

            _next = null;
            return false;
        }
        #endregion

        #region Event
        private List<DialogueEvent> playingEvents = new List<DialogueEvent>();
        private Action onStopEventsComplete = null;

        // -----------------------

        /// <summary>
        /// Plays all given events.
        /// </summary>
        /// <param name="_events">All events to play.</param>
        public void PlayEvents(IList<DialogueEvent> _events) {
            if (_events == null)
                return;

            ref List<DialogueEvent> _playingEvents = ref playingEvents;
            int _count = _events.Count;

            for (int i = 0; i < _count; i++) {
                _events[i].Play(this, _playingEvents);
            }
        }

        /// <summary>
        /// Updates all currently active events in this player.
        /// </summary>
        private void UpdateEvents() {
            ref List<DialogueEvent> _events = ref playingEvents;
            for (int i = _events.Count; i-- > 0;) {

                if (!_events[i].Update(this)) {
                    _events.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Stops playing all current events.
        /// </summary>
        /// <param name="_isClosingDialogue">Indicates if the associated dialogue is being closed, or not.</param>
        /// <param name="_onComplete">Delegate to be called once all events are stopped.</param>
        public void StopEvents(bool _isClosingDialogue, Action _onComplete) {
            ref List<DialogueEvent> _events = ref playingEvents;
            int _count = _events.Count;

            // Instant - no event.
            if (_count == 0) {
                _onComplete?.Invoke();
                return;
            }

            // Set callback.
            onStopEventsComplete = _onComplete;

            // Stop.
            for (int i = _count; i-- > 0;) {
                _events[i].Stop(this, _isClosingDialogue);
            }
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        /// <summary>
        /// Called when an event successfully stopped playing.
        /// </summary>
        internal void OnEventStopped(DialogueEvent _event) {
            ref List<DialogueEvent> _events = ref playingEvents;

            // If failed to remove, ignore.
            if (!_events.Remove(_event))
                return;

            if (_events.Count != 0)
                return;

            // Complete delegate.
            ref Action _callback = ref onStopEventsComplete;
            if (_callback != null) {

                _callback.Invoke();
                _callback = null;
            }
        }
        #endregion

        #region Pool
        void IPoolableObject.OnCreated(IObjectPool _pool) { }

        void IPoolableObject.OnRemovedFromPool() { }

        void IPoolableObject.OnSentToPool() { }
        #endregion

        #region Utility
        /// <returns><inheritdoc cref="GetSettings{T}(out T)" path="/param[@name='_settings']"/></returns>
        /// <inheritdoc cref="GetSettings{T}(out T)"/>
        public abstract T GetSettings<T>() where T : DialogueSettings;

        /// <summary>
        /// Get this player associated <see cref="DialogueSettings"/>.
        /// </summary>
        /// <typeparam name="T">The expected <see cref="DialogueSettings"/> type.</typeparam>
        /// <param name="_settings">This player associated <see cref="DialogueSettings"/>.</param>
        /// <returns>True if this player settings could be casted to the expected type, false otherwise.</returns>
        public abstract bool GetSettings<T>(out T _settings) where T : DialogueSettings;
        #endregion
    }

    /// <summary>
    /// Base class to inherit all <see cref="Dialogue"/> players from.
    /// <br/> Use this to implement a specific behaviour when playing the associated <see cref="Dialogue"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="DialogueSettings"/> type required by this player.</typeparam>
    [Serializable]
    public abstract class DialoguePlayer<T> : DialoguePlayer where T : DialogueSettings, new() {
        #region Global Members
        /// <summary>
        /// The currently playing <see cref="Dialogue"/> associated <see cref="DialogueSettings"/>.
        /// </summary>
        public T Settings = null;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from creating new instances using this class constructor.
        /// <br/> To create a new player, always use <see cref="Dialogue.CreatePlayer"/>.
        /// </summary>
        internal protected DialoguePlayer() : base() { }
        #endregion

        #region Activation
        public sealed override void Setup(Dialogue _dialogue, DialogueNode _currentNode) {
            // Set settings.
            Settings = _dialogue.Settings as T;

            base.Setup(_dialogue, _currentNode);
        }
        #endregion

        #region Behaviour
        public override bool GetNextNode(out DialogueNode _next) {
            return GetNextNode(Settings.NextNodeBehaviour, CurrentNode, out _next);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="GetSettings{U}"/>
        public T GetSettings() {
            return GetSettings<T>();
        }

        public sealed override U GetSettings<U>() {
            if (GetSettings(out U _settings)) {
                return _settings;
            }

            throw new InvalidCastException($"Could not cast settings of type \'{Settings.GetType().Name}\' in type \'{typeof(U).Name}\'.");
        }

        public override bool GetSettings<U>(out U _settings) {
            return EnhancedUtility.IsType(Settings, out _settings);
        }
        #endregion
    }
}
