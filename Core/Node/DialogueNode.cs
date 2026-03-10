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
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization.Tables;
#endif

#if UNITY_EDITOR
using UnityEditor.AnimatedValues;
#endif

namespace EnhancedFramework.Dialogues {
    /// <summary>
    /// <see cref="Dialogue"/> node base class.
    /// <br/> Inherit from this to create your own nodes.
    /// <para/>
    /// Cannot be set as abstract for serialization purposes.
    /// </summary>
    [Serializable, Ethereal]
    public class DialogueNode
                              #if LOCALIZATION_ENABLED
                              : ILocalizable
                              #endif
    {
        #region Global Members
        public const string DefaultSpeakerName  = "[NONE]";
        public const string DefaultText         = "[EMPTY]";

        [Tooltip("Unique guid of this node")]
        [PreventCopy, SerializeField, Enhanced, ReadOnly] internal int guid = EnhancedUtility.GenerateGUID();

        [Tooltip("Indicates if this node is available to be played")]
        [SerializeField] internal protected bool available = true;

        [Tooltip("This node will be automatically be skipped and ignored if set to false")]
        [SerializeField] internal protected bool enabled = true;

        #if UNITY_EDITOR
        // Dialogue editor window related properties.

        [NonSerialized] internal bool isSelected = false;
        [SerializeField,   HideInInspector] internal AnimBool foldout = new AnimBool(true);
        [SerializeReference, NonSerialized] internal DialogueNode parent = null;
        #endif

        [Tooltip("All connection nodes from this node")]
        [SerializeReference, HideInInspector, PreventCopy] internal protected DialogueNode[] nodes = new DialogueNode[0];

        // -----------------------

        /// <summary>
        /// The <see cref="string"/> text content of this node (empty is none).
        /// </summary>
        public virtual string Text {
            get { return string.Empty; }
            set { }
        }

        /// <summary>
        /// Unique guid of this node.
        /// </summary>
        public int Guid {
            get { return guid; }
        }

        /// <summary>
        /// Indicates if this node is available to be played.
        /// </summary>
        public virtual bool IsAvailable {
            get { return available; }
        }

        /// <summary>
        /// Index of this node speaker (-1 if none assigned).
        /// </summary>
        public virtual int SpeakerIndex {
            get { return -1; }
        }

        /// <summary>
        /// Total count of this node connections (<see cref="DialogueNode"/>).
        /// </summary>
        public virtual int NodeCount {
            get { return nodes.Length; }
        }

        /// <summary>
        /// Indicates if the dialogue should be closed after playing this node (check for any available connection(s) by default).
        /// </summary>
        public virtual bool IsClosingNode {
            get {
                ref DialogueNode[] _span = ref nodes;
                for (int i = _span.Length; i-- > 0;) {
                    if (_span[i].IsAvailable) {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Indicates if this node is a root node.
        /// </summary>
        public virtual bool IsRoot {
            get { return false; }
        }

        // -----------------------

        /// <summary>
        /// Short description of this node, displayed on top of the inspector.
        /// </summary>
        public virtual string Description {
            get { return string.Empty; }
        }

        /// <summary>
        /// Default speaker name displayed for this node (especially used in the editor).
        /// </summary>
        public virtual string DefaultSpeaker {
            get { return DefaultSpeakerName; }
        }

        /// <summary>
        /// Indicates if this node connections (<see cref="DialogueNode"/>) should be displayed in the editor.
        /// </summary>
        internal protected virtual bool ShowNodes {
            get { return true; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from creating new instances of this base class.
        /// </summary>
        protected DialogueNode() { }
        #endregion

        #region Operator
        public static implicit operator int(DialogueNode _node) {
            return _node.guid;
        }

        public override string ToString() {
            return ((int)this).ToString();
        }
        #endregion

        #region Node Management
        /// <summary>
        /// Get this node connection at a specific index (<see cref="DialogueNode"/>).
        /// <br/> Use <see cref="NodeCount"/> to get the total amount of connection nodes.
        /// </summary>
        /// <param name="_index">The index to get the connection node at.</param>
        /// <returns>This node connection at the given index (<see cref="DialogueNode"/>).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual DialogueNode GetNodeAt(int _index) {
            return nodes[_index];
        }

        /// <summary>
        /// All this object sub node connections.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ref DialogueNode[] GetNodeRefs() {
            return ref nodes;
        }

        /// <summary>
        /// Adds a new <see cref="DialogueNode"/> to this node connections.
        /// </summary>
        /// <param name="_node">The <see cref="DialogueNode"/> to add as a connection.</param>
        public virtual void AddNode(DialogueNode _node) {
            ArrayUtility.Add(ref nodes, _node);
        }

        /// <summary>
        /// Copies all the values of a specific <see cref="DialogueNode"/> into this one.
        /// </summary>
        /// <param name="_source">The source <see cref="DialogueNode"/> to copy the values from.</param>
        /// <param name="_copyConnections">Whether the connection nodes should also be copied or not.</param>
        /// <returns>This node instance.</returns>
        internal DialogueNode CopyNode(DialogueNode _source, bool _copyConnections = true) {
            EnhancedUtility.CopyObjectContent(_source, this);

            if (_copyConnections) {
                ref DialogueNode[] _thisNodes  = ref nodes;
                ref DialogueNode[] _otherNodes = ref _source.nodes;

                int _count = _otherNodes.Length;
                Array.Resize(ref _thisNodes, _count);

                for (int i = 0; i < _count; i++) {
                    DialogueNode _innerNode = _otherNodes[i];
                    DialogueNode _new = Activator.CreateInstance(_innerNode.GetType()) as DialogueNode;

                    _thisNodes[i] = _new.CopyNode(_innerNode, _copyConnections);
                }
            }

            return this;
        }

        /// <summary>
        /// Transmutes this <see cref="DialogueNode"/> into a node of another type.
        /// </summary>
        /// <param name="_dialogue"><inheritdoc cref="Doc(Dialogue, DialogueSettings, DialoguePlayer)" path="/param[@name='_dialogue']"/></param>
        /// <param name="_type">The new node type in which to transmute this node.
        /// <br/> Must inherit from <see cref="DialogueNode"/>.</param>
        /// <param name="_doTransmuteSelf">Whether this node should be transmuted or not.</param>
        /// <param name="_doTransmuteConnections">Whether this node connections should be transmuted or not.</param>
        /// <returns>The new transmuted node instance.</returns>
        internal DialogueNode Transmute(Dialogue _dialogue, Type _type, bool _doTransmuteSelf = true, bool _doTransmuteConnections = true) {
            // Connections.
            if (_doTransmuteConnections) {

                ref DialogueNode[] _span = ref nodes;
                int _count = _span.Length;

                for (int i = 0; i < _count; i++) {
                    _span[i].Transmute(_dialogue, _type);
                }
            }

            // Self.
            if (_doTransmuteSelf) {
                var _new = Activator.CreateInstance(_type);
                DialogueNode _node = EnhancedUtility.CopyObjectContent(this, _new, true) as DialogueNode;

                if (_node is DialogueLink _link) {
                    _link.RemoveLink();
                }

                UpdateLink(_dialogue.Root);

                // ----- Local Method ----- \\

                void UpdateLink(DialogueNode _root) {

                    ref DialogueNode[] _span = ref _root.nodes;
                    int _count = _span.Length;

                    for (int i = 0; i < _count; i++) {
                        if (_span[i] == this) {
                            _span[i] = _node;
                        }

                        UpdateLink(_span[i]);
                    }
                }

                return _node;
            }

            return this;
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Called to reset this node behaviour.
        /// </summary>
        internal protected virtual void Reset() { }

        /// <summary>
        /// Plays this <see cref="DialogueNode"/>.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        /// <param name="_player"><inheritdoc cref="Doc(Dialogue, DialogueSettings, DialoguePlayer)" path="/param[@name='_player']"/></param>
        public virtual void Play(DialoguePlayer _player) { }

        /// <summary>
        /// Quits this <see cref="DialogueNode"/>, before moving to the next one.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        /// <param name="_player"><inheritdoc cref="Doc(Dialogue, DialogueSettings, DialoguePlayer)" path="/param[@name='_player']"/></param>
        /// <param name="_isClosingDialogue">Indicates if the dialogue is being closed or will continue to be played.</param>
        /// <param name="_onQuit">Delegate to be called once this node was quit.</param>
        public virtual void Quit(DialoguePlayer _player, bool _isClosingDialogue, Action _onQuit) {
            _onQuit?.Invoke();
        }

        /// <summary>
        /// Skips this node content.
        /// </summary>
        /// <param name="_player"><inheritdoc cref="Doc(Dialogue, DialogueSettings, DialoguePlayer)" path="/param[@name='_player']"/></param>
        public virtual void Skip(DialoguePlayer _player) {
            _player.PlayNextNode();
        }
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        /// <inheritdoc cref="ILocalizable.GetLocalizationTables(Set{TableReference}, Set{TableReference})"/>
        public virtual void GetLocalizationTables(Set<TableReference> _stringTables, Set<TableReference> _assetTables) {
            // If this node connections are hidden, ignore them.
            // Avoids cyclic loops with links.
            if (!ShowNodes)
                return;

            ref DialogueNode[] _span = ref GetNodeRefs();
            for (int i = _span.Length; i-- > 0;) {
                _span[i].GetLocalizationTables(_stringTables, _assetTables);
            }
        }
#endif
        #endregion

        #region Utility
        /// <summary>
        /// Get if there is any sub-node available from this one.
        /// </summary>
        /// <returns>True if there is at least one available sub-node, false otherwise.</returns>
        public bool HasAnyAvailableNode() {
            ref DialogueNode[] _span = ref GetNodeRefs();

            for (int i = _span.Length; i-- > 0;) {
                if (_span[i].IsAvailable)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the total count of available sub-node(s) from this one.
        /// </summary>
        /// <returns>Total amount of available sub-node(s).</returns>
        public int GetAvailableNodeCount() {
            ref DialogueNode[] _span = ref GetNodeRefs();
            int _count = 0;

            for (int i = _span.Length; i-- > 0;) {
                if (_span[i].IsAvailable) {
                    _count++;
                }
            }

            return _count;
        }
        #endregion

        #region Editor Utility
        /// <summary>
        /// Called when this node is drawn in the editor.
        /// </summary>
        /// <param name="_dialogue"><inheritdoc cref="Doc(Dialogue, DialogueSettings, DialoguePlayer)" path="/param[@name='_dialogue']"/></param>
        internal protected virtual void OnEditorDraw(Dialogue _dialogue) { }

        /// <summary>
        /// Get the name of this node speaker (used to display the associated color in the editor).
        /// </summary>
        /// <param name="_settings"><inheritdoc cref="Doc(Dialogue, DialogueSettings, DialoguePlayer)" path="/param[@name='_settings']"/></param>
        /// <returns>Editor-related displayed name of this node speaker.</returns>
        internal protected virtual string GetEditorSpeakerName(DialogueSettings _settings) {
            int _speakerIndex = SpeakerIndex;

            if ((_speakerIndex < 0) || (_speakerIndex >= _settings.SpeakerCount)) {
                return DefaultSpeaker;
            }

            return _settings.GetSpeakerAt(_speakerIndex);
        }

        /// <summary>
        /// Get the name of the icons to display next to this node in the editor.
        /// <br/>
        /// The icons to load must be located in the 'Editor Default Resources' folder, at the root of the project.
        /// </summary>
        /// <param name="_index">Index of the icon to load.</param>
        /// <param name="_iconName">Name of the icon to load.</param>
        /// <returns>Total number of icon(s) to be loaded.</returns>
        internal protected virtual int GetEditorIcon(int _index, out string _iconName) {
            _iconName = string.Empty;
            return 0;
        }

        /// <summary>
        /// Get the text to be displayed for this node in the editor.
        /// <br/> The edited text is always <see cref="Text"/>.
        /// </summary>
        /// <returns>This node displayed text.</returns>
        internal protected virtual string GetEditorDisplayedText() {
            string _text = Text;
            return string.IsNullOrEmpty(_text.Trim()) ? DefaultText : _text;
        }

        /// <summary>
        /// Get the additional context menu items to be displayed for this node in the editor.
        /// </summary>
        /// <param name="_index">Menu item index.</param>
        /// <param name="_content"><see cref="GUIContent"/> to be display on the item.</param>
        /// <param name="_callback">Callback when the item is clicked.</param>
        /// <param name="_enabled">Whether this menu item should be enabled or not.</param>
        /// <returns>Total number of item(s) to be added to the menu.</returns>
        internal protected virtual int OnEditorContextMenu(int _index, out GUIContent _content, out Action _callback, out bool _enabled) {
            _content  = null;
            _callback = null;
            _enabled  = false;

            return 0;
        }
        #endregion

        #region Documentation
        /// <summary>
        /// Documentation only method.
        /// </summary>
        /// <param name="_dialogue">The source <see cref="Dialogue"/> of this node.</param>
        /// <param name="_settings">The <see cref="DialogueSettings"/> of this node <see cref="Dialogue"/>.</param>
        /// <param name="_player">The <see cref="DialoguePlayer"/> used to play this node.</param>
        #pragma warning disable
        private void Doc(Dialogue _dialogue, DialogueSettings _settings, DialoguePlayer _player) { }
        #endregion
    }
}
