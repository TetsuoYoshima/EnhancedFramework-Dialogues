// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
//  Use the [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Namespace", "Assembly", "Class")]
//  attribute to remove a managed reference error when renaming a script or an assembly.
//
// ============================================================================================= //

#if LOCALIZATION_PACKAGE
#define LOCALIZATION_ENABLED
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using DisplayName = EnhancedEditor.DisplayNameAttribute;
#endif

#if UNITY_EDITOR
using UnityEditor;
using ArrayUtility = EnhancedEditor.ArrayUtility;
#endif

[assembly: InternalsVisibleTo("EnhancedFramework.Dialogues.Editor")]
namespace EnhancedFramework.Dialogues {
    // ===== Utility ===== \\

    /// <summary>
    /// <see cref="Dialogue"/> root node class.
    /// </summary>
    [Serializable, Ethereal]
    public sealed class DialogueRoot : DialogueNode {
        #region Global Members
        #if UNITY_EDITOR
        /// <summary>
        /// Editor only, used to display and edit the dialogue name.
        /// </summary>
        [SerializeField] internal Dialogue dialogue = null;

        // -----------------------

        public override string Text {
            get {
                ref Dialogue _dialogue = ref dialogue;
                if (_dialogue == null)
                    return base.Text;

                return _dialogue.name.RemovePrefix();
            }
            set {
                ref Dialogue _dialogue = ref dialogue;
                if (_dialogue == null)
                    return;

                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(_dialogue), $"{_dialogue.name.GetPrefix()}{value}");
            }
        }
        #endif

        public override string DefaultSpeaker {
            get { return "[ROOT]"; }
        }

        public override bool IsRoot {
            get { return true; }
        }
        #endregion

        #region Behaviour
        public override void Play(DialoguePlayer _player) {
            base.Play(_player);

            // Automatically play the next node.
            _player.PlayNextNode();
        }
        #endregion

        #region Editor Utility
        internal protected override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "Profiler.Custom";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return 1;
        }
        #endregion
    }

    /// <summary>
    /// Default <see cref="DialoguePlayer"/> class, only sending logs about its current state.
    /// </summary>
    [Serializable, DisplayName("<None>")]
    public sealed class DialogueDefaultPlayer : DialoguePlayer<DialogueDefaultSettings> {
        #region State
        protected override void OnSetup() {
            base.OnSetup();

            this.LogMessage($"Setup \'{Name}\', ready to be played", Dialogue);
        }

        protected override void OnClose(Action _onNodeQuit = null) {
            base.OnClose(_onNodeQuit);

            this.LogMessage($"Closing \'{Name}\'", Dialogue);
            CancelPlay();
        }
        #endregion

        #region Behaviour
        private DelayHandler delayedCall = default;

        // -----------------------

        public override void PlayCurrentNode() {
            base.PlayCurrentNode();

            this.LogMessage($"Playing node {CurrentNode.Guid} - \"{CurrentNode.Text}\"", Dialogue);

            // Use a delay before playing the next node,
            // avoiding infinite loops on referenced links.
            delayedCall = Delayer.Call(.1f, () => PlayNextNode(true), false);
        }

        private void CancelPlay() {
            delayedCall.Cancel();
        }
        #endregion
    }

    /// <summary>
    /// Default <see cref="DialogueSettings"/> class, only containing an array of <see cref="string"/> for speakers.
    /// </summary>
    [Serializable, DisplayName("<Default>")]
    public sealed class DialogueDefaultSettings : DialogueSettings<string> {
        #region Global Members
        /// <inheritdoc cref="DialogueDefaultSettings"/>
        public DialogueDefaultSettings() {
            Speakers = new string[] { "Player", "NPC" };
        }
        #endregion

        #region Speaker
        public override string GetSpeakerAt(int _index) {
            return Speakers[_index];
        }
        #endregion
    }

    // ===== Dialogue ===== \\

    /// <summary>
    /// <see cref="ScriptableObject"/> database for a dialogue.
    /// </summary>
    [CreateAssetMenu(fileName = FilePrefix + "NewDialogue", menuName = FrameworkUtility.MenuPath + "Dialogue", order = FrameworkUtility.MenuOrder + 50)]
    public sealed class Dialogue : EnhancedScriptableObject
                                 #if LOCALIZATION_ENABLED
                                 , ILocalizable
                                 #endif
    {
        public const string FilePrefix = "DLG_";

        #region Global Members
        [Section("Dialogue")]

        [Tooltip("Node type used when creating a new default node in this dialogue")]
        [SerializeField, DisplayName("Default Node")]
        private SerializedType<DialogueNode> defaultNodeType = new SerializedType<DialogueNode>(SerializedTypeConstraint.None, typeof(DialogueTextLine),
                                                                                                                               #if LOCALIZATION_ENABLED
                                                                                                                               typeof(DialogueLocalizedLine),
                                                                                                                               #endif
                                                                                                                               typeof(DialogueLink),
                                                                                                                               typeof(DialogueResetNode));

        [Tooltip("Node type used when creating a new link in this dialogue")]
        [SerializeField, DisplayName("Default Link")]
        private SerializedType<DialogueLink> defaultLinkType = new SerializedType<DialogueLink>(SerializedTypeConstraint.BaseType, typeof(DialogueLink));

        [Space(5f)]

        [Tooltip("Class used to play this dialogue, managing its behaviour")]
        [SerializeField, DisplayName("Dialogue Player")]
        private SerializedType<DialoguePlayer> playerType = new SerializedType<DialoguePlayer>(SerializedTypeConstraint.None, typeof(DialogueDefaultPlayer));


        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Configurable settings of this dialogue")]
        [SerializeReference, Enhanced, Block] private DialogueSettings settings = new DialogueDefaultSettings();

        // -----------------------

        [Tooltip("Root node of this dialogue")]
        [SerializeReference, HideInInspector] internal DialogueRoot root = new DialogueRoot();

        // -----------------------

        private static List<string> editorSpeakersBuffer = new List<string>();

        #if UNITY_EDITOR
        internal bool isEditorCachedSpeakers = false;
        #endif

        // -----------------------

        /// <summary>
        /// Node type used when creating a new default node in this dialogue (must inherit from <see cref="DialogueNode"/>).
        /// </summary>
        public Type DefaultNodeType {
            get { return defaultNodeType.Type; }
            set { defaultNodeType.Type = value; }
        }

        /// <summary>
        /// Node type used when creating a new link in this dialogue (must inherit from <see cref="DialogueLink"/>).
        /// </summary>
        public Type DefaultLinkType {
            get { return defaultLinkType.Type; }
            set { defaultLinkType.Type = value; }
        }

        /// <summary>
        /// Type class used to play this dialogue, managing its behaviour (must inherit from <see cref="DialoguePlayer{T}"/>).
        /// </summary>
        public Type PlayerType {
            get { return playerType.Type; }
            set {
                playerType.Type = value;

                var _settings = Activator.CreateInstance(GetSettingsType(value));
                if (settings != null) {
                    _settings = EnhancedUtility.CopyObjectContent(settings, _settings);
                }

                settings = _settings as DialogueSettings;
            }
        }

        /// <summary>
        /// <see cref="DialoguePlayer"/>-related configurable settings of this dialogue.
        /// </summary>
        public DialogueSettings Settings {
            get { return settings; }
        }

        /// <summary>
        /// The root <see cref="DialogueNode"/> of this <see cref="Dialogue"/>.
        /// </summary>
        public DialogueRoot Root {
            get { return root; }
        }

        /// <summary>
        /// Speaker names of this dialogue.
        /// <br/> Should only be used in editor, especially by popups and property drawers.
        /// </summary>
        public List<string> EditorSpeakers {
            get {
                ref List<string> _buffer = ref editorSpeakersBuffer;

                #if UNITY_EDITOR
                if (isEditorCachedSpeakers) {
                    return _buffer;
                }
                #endif

                GetSpeakers(_buffer);

                #if UNITY_EDITOR
                isEditorCachedSpeakers = true;
                #endif

                return _buffer;
            }
        }

        /// <summary>
        /// Get if there exist any duplicate speaker name in this dialogue.
        /// </summary>
        public bool HasDuplicateName {
            get {
                List<string> _speakers = EditorSpeakers;
                int _count = _speakers.Count;

                for (int i = 0; i < _count; i++) {
                    string _speaker = _speakers[i];

                    for (int j = i + 1; j < _count; j++) {
                        if (_speakers[j].EqualOrdinal(_speaker)) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates if this <see cref="Dialogue"/> has any available node to play.
        /// </summary>
        public bool IsPlayable {
            get {
                ref DialogueNode[] _span = ref root.nodes;
                for (int i = _span.Length; i-- > 0;) {
                    if (_span[i].IsAvailable) {
                        return true;
                    }
                }

                return false;
            }
        }

        // -------------------------------------------
        // Events
        // -------------------------------------------

        /// <summary>
        /// Called whenver this <see cref="Dialogue"/> starts being played.
        /// </summary>
        public Action<Dialogue, DialoguePlayer> OnPlayed = null;

        /// <summary>
        /// Called whenever this <see cref="Dialogue"/> is being closed.
        /// </summary>
        public Action<Dialogue, DialoguePlayer> OnClosed = null;
        #endregion

        #region Scriptable Object
        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        private void Awake() {
            // Root dialogue setup.
            root.dialogue = this;

            RefreshValues();
        }

        protected override void OnValidate() {
            base.OnValidate();

            RefreshValues();
        }

        // -----------------------

        private void RefreshValues() {
            if (Application.isPlaying)
                return;

            // Settings type update.
            if ((settings == null) || (GetSettingsType(PlayerType) != settings.GetType())) {
                PlayerType = playerType;
            }

            ResetNodes();
        }
        #endif
        #endregion

        #region Player
        private DialoguePlayer currentPlayer = null;
        private bool requireReset = false;

        // -----------------------

        /// <inheritdoc cref="CreatePlayer(DialogueNode)"/>
        public DialoguePlayer CreatePlayer() {
            return CreatePlayer(root);
        }

        /// <summary>
        /// Creates and setup a new <see cref="DialoguePlayer"/> for this dialogue.
        /// <br/> Use this to play its content.
        /// </summary>
        /// <param name="_currentNode">First node to play.</param>
        /// <returns>The newly created <see cref="DialoguePlayer"/> to play this dialogue.</returns>
        public DialoguePlayer CreatePlayer(DialogueNode _currentNode) {
            // Reset.
            if (requireReset) {
                ResetNodes();
                requireReset = false;
            }

            // Play.
            DialoguePlayer _player = DialoguePlayerManager.GetPoolInstance(PlayerType);
            currentPlayer = _player;

            _player.Setup(this, _currentNode);

            // Event.
            OnPlayed?.Invoke(this, _player);
            return _player;
        }

        /// <summary>
        /// Closes the last created <see cref="DialoguePlayer"/> for this dialogue.
        /// </summary>
        /// <returns>True if the player could be successfully closed, false otherwise.</returns>
        public bool ClosePlayer() {
            ref DialoguePlayer _player = ref currentPlayer;

            if (_player != null) {
                _player.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when a <see cref="DialoguePlayer"/> of this dialogue is closed.
        /// </summary>
        /// <param name="_player">The <see cref="DialoguePlayer"/> being closed.</param>
        internal void OnPlayerClosed(DialoguePlayer _player) {
            ref DialoguePlayer _currentPlayer = ref currentPlayer;

            if (_currentPlayer == _player) {
                _currentPlayer = null;

                // Event.
                OnClosed?.Invoke(this, _player);
            }

            DialoguePlayerManager.ReleasePoolInstance(_player);
        }

        /// <summary>
        /// Get this <see cref="Dialogue"/> current active <see cref="DialoguePlayer"/>.
        /// </summary>
        /// <param name="_player">This dialogue active <see cref="DialoguePlayer"/> (null if none).</param>
        /// <returns>True if an active <see cref="DialoguePlayer"/> could be found, false otherwise.</returns>
        public bool GetPlayer(out DialoguePlayer _player) {
            ref DialoguePlayer _currentPlayer = ref currentPlayer;

            if ((_currentPlayer != null) && _currentPlayer.IsPlaying) {
                _player = _currentPlayer;
                return true;
            }

            _player = null;
            return false;
        }
        #endregion

        #region Nodes
        /// <summary>
        /// Adds a new default node to this dialogue, at a specific root node.
        /// </summary>
        /// <inheritdoc cref="AddNode(DialogueNode, Type)"/>
        public DialogueNode AddDefaultNode(DialogueNode _root) {
            return AddNode(_root, DefaultNodeType);
        }

        /// <summary>
        /// Adds a new specific type of <see cref="DialogueNode"/> to a specific root node from this dialogue.
        /// </summary>
        /// <param name="_root">The root <see cref="DialogueNode"/> to add a new node to.</param>
        /// <param name="_nodeType">The type of node to create and add (must inherit from <see cref="DialogueNode"/>).</param>
        /// <returns>The newly created node.</returns>
        public DialogueNode AddNode(DialogueNode _root, Type _nodeType) {
            if (!_nodeType.IsSubclassOf(typeof(DialogueNode))) {
                return null;
            }

            DialogueNode _node = Activator.CreateInstance(_nodeType) as DialogueNode;
            _root.AddNode(_node);

            return _node;
        }

        /// <summary>
        /// Removes a specific <see cref="DialogueNode"/> from this dialogue.
        /// </summary>
        /// <param name="_node">The <see cref="DialogueNode"/> to remove.</param>
        public void RemoveNode(DialogueNode _node) {
            if (FindNode(_node, out DialogueNode _root)) {
                ArrayUtility.Remove(ref _root.nodes, _node);
            }
        }

        /// <summary>
        /// Finds the <see cref="DialogueNode"/> matching a given guid.
        /// </summary>
        /// <param name="_guid">GUID to find matching node.</param>
        /// <param name="_node">Found node matching the given guid (null if none).</param>
        /// <returns>True if a node matching the given guid was successfully found, false otherwise.</returns>
        public bool FindNode(int _guid, out DialogueNode _node) {
            return DoFindNode(_guid, root, out _node);

            // ----- Local Method ----- \\

            static bool DoFindNode(int _guid, DialogueNode _root, out DialogueNode _doNode) {

                ref DialogueNode[] _span = ref _root.nodes;
                int _count = _span.Length;

                bool _showNodes = _root.ShowNodes;

                for (int i = 0; i < _count; i++) {
                    DialogueNode _innerNode = _span[i];

                    if (_innerNode.Guid == _guid) {
                        _doNode = _innerNode;
                        return true;
                    }

                    if (!_showNodes) {
                        continue;
                    }

                    if (DoFindNode(_guid, _innerNode, out _doNode)) {
                        return true;
                    }
                }

                _doNode = null;
                return false;
            }
        }

        /// <summary>
        /// Finds a given <see cref="DialogueNode"/> with its root node.
        /// </summary>
        /// <param name="_node">The node to find.</param>
        /// <param name="_root">Root of the given node (null if not found).</param>
        /// <returns>True if the given node was successfully found, false otherwise.</returns>
        public bool FindNode(DialogueNode _node, out DialogueNode _root) {
            return DoFindNode(_node, root, out _root);

            // ----- Local Method ----- \\

            static bool DoFindNode(DialogueNode _node, DialogueNode _root, out DialogueNode _doRoot) {

                ref DialogueNode[] _span = ref _root.nodes;
                int _count = _span.Length;

                bool _showNodes = _root.ShowNodes;

                for (int i = 0; i < _count; i++) {
                    DialogueNode _innerNode = _span[i];

                    if (_innerNode == _node) {
                        _doRoot = _root;
                        return true;
                    }

                    if (!_showNodes) {
                        continue;
                    }

                    if (DoFindNode(_node, _innerNode, out _doRoot)) {
                        return true;
                    }
                }

                _doRoot = null;
                return false;
            }
        }

        /// <summary>
        /// Resets all nodes. Called to clear behaviour when exiting play mode.
        /// </summary>
        public void ResetNodes() {
            DoResetNode(root);

            // ----- Local Method ----- \\

            static void DoResetNode(DialogueNode _root) {
                _root.Reset();

                if (!_root.ShowNodes) // Avoid infinite loops - like with links.
                    return;

                ref DialogueNode[] _span = ref _root.nodes;
                for (int i = _span.Length; i-- > 0;) {
                    DoResetNode(_span[i]);
                }
            }
        }

        /// <summary>
        /// Mark this dialogue as requiring to be reset before next play.
        /// </summary>
        public void ResetForNextPlay() {
            requireReset = true;
        }
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        /// <inheritdoc cref="ILocalizable.GetLocalizationTables(Set{TableReference}, Set{TableReference})"/>
        public void GetLocalizationTables(Set<TableReference> _stringTables,  Set<TableReference> _assetTables) {
            settings.GetLocalizationTables(_stringTables, _assetTables);
            root    .GetLocalizationTables(_stringTables, _assetTables);
        }
        #endif
        #endregion

        #region Utility
        /// <summary>
        /// Random-related helper, used to ensure the same value is not selected twice in a row.
        /// </summary>
        internal int lastRandomIndex = -1;

        // -----------------------

        /// <summary>
        /// Get the name of all speakers in this dialogue.
        /// </summary>
        /// <param name="_buffer">Buffer used to store result.</param>
        /// <returns>The total amount of speaker.</returns>
        public int GetSpeakers(List<string> _buffer) {
            ref DialogueSettings _settings = ref settings;
            int _count = _settings.SpeakerCount;

            _buffer.SoftResize(_count);

            for (int i = 0; i < _count; i++) {
                _buffer[i] = $"{_settings.GetSpeakerAt(i)} [{i + 1}]";
            }

            return _count;
        }

        /// <summary>
        /// Get this dialogue <see cref="DialogueSettings"/> type (<see cref="DialoguePlayer{T}"/>-related).
        /// </summary>
        /// <param name="_player">The <see cref="DialoguePlayer{T}"/> type to get the associated settings.</param>
        /// <returns>This dialogue settings type.</returns>
        private Type GetSettingsType(Type _player) {
            while (_player.BaseType != null) {
                _player = _player.BaseType;

                if (_player.IsGenericType && (_player.GetGenericTypeDefinition() == typeof(DialoguePlayer<>))) {
                    return _player.GetGenericArguments()[0];
                }
            }

            return null;
        }
        #endregion
    }
}
