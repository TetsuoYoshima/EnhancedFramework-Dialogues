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
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

using DisplayName = EnhancedEditor.DisplayNameAttribute;
#endif

namespace EnhancedFramework.Dialogues {
    // ===== Base ===== \\

    /// <summary>
    /// Base <see cref="DialogueNode"/> line class.
    /// <br/> Inherit from this to create your own lines.
    /// </summary>
    /// <typeparam name="T">This line content type.</typeparam>
    [Serializable]
    public abstract class DialogueLine<T> : DialogueNode {
        #region Global Members
        [Tooltip("This line content")]
        [Enhanced, Block] public T Line = Activator.CreateInstance<T>();

        [Space(10f)]

        [Tooltip("This line speaker")]
        [SerializeField, Enhanced, DisplayName("Speaker"), Popup(nameof(Dialogue.EditorSpeakers))] protected int speakerIndex = 0;

        [Tooltip("This line can only be played once, and will not be available after that until reset")]
        [SerializeField] protected bool onlyOnce = false;

        #if UNITY_EDITOR
        [Space(10f)]

        [Tooltip("Editor-only utility comment section")]
        [SerializeField, TextArea(2, 7)] internal string comment = string.Empty;
        #endif

        [Space(10f)]

        [Tooltip("Required flags for this line to be available")]
        public FlagValueGroup RequiredFlags = new FlagValueGroup();

        [Tooltip("Modified flags set after this line is played")]
        public FlagValueGroup AfterFlags = new FlagValueGroup();

        // -----------------------

        [NonSerialized] private bool wasPlayed = false;

        /// <summary>
        /// The duration of this line (in seconds).
        /// </summary>
        public virtual float Duration {
            get { return 0f; }
        }

        // -----------------------

        public override int SpeakerIndex {
            get { return speakerIndex; }
        }

        public override bool IsAvailable {
            get {
                if (onlyOnce && wasPlayed) {
                    return false;
                }

                return base.IsAvailable && RequiredFlags.Valid;
            }
        }
        #endregion

        #region Behaviour
        public override void Play(DialoguePlayer _player) {
            base.Play(_player);

            // Skip content.
            if (onlyOnce && wasPlayed) {
                _player.PlayNextNode();
                return;
            }

            // Update flag values (safer than on exit).
            AfterFlags.SetValues();
            wasPlayed = true;
        }

        protected internal override void Reset() {
            base.Reset();
            wasPlayed = false;
        }
        #endregion

        #region Editor Utility
        protected internal override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "console.infoicon.sml";
                    break;

                case 1:
                    _iconName = "CrossIcon";//"close_button";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return (nodes.Length == 0) ? 2 : 1;
        }
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// <see cref="DialogueLine{T}"/> node class with a single text and an associated audio file.
    /// </summary>
    [Serializable, DisplayName("Base/Line [Standard]")]
    public class DialogueTextLine : DialogueLine<DialogueTextLine.Content> {
        /// <summary>
        /// Wrapper for a <see cref="DialogueTextLine"/> line content.
        /// </summary>
        [Serializable]
        public class Content {
            #region Content
            [Tooltip("Text of this line")]
            [Enhanced, EnhancedTextArea(true)] public string Text = DefaultText;

            [Tooltip("Audio asset of this line")]
            public AudioAsset Audio = null;
            #endregion
        }

        #region Global Members
        public override string Text {
            get { return Line.Text; }
            set { Line.Text = value; }
        }

        public override float Duration {
            get {
                ref AudioAsset _audio = ref Line.Audio;
                return _audio.IsValid()
                     ? _audio.Duration
                     : (Text.Length * .05f);
            }
        }
        #endregion
    }

    #if LOCALIZATION_ENABLED
    /// <summary>
    /// <see cref="DialogueLine{T}"/> node class with a localized text and an associated localized audio file.
    /// </summary>
    [Serializable, DisplayName("Base/Line [Localized]")]
    public class DialogueLocalizedLine : DialogueLine<DialogueLocalizedLine.Content> {
        /// <summary>
        /// Wrapper for a <see cref="DialogueLocalizedLine"/> line content.
        /// </summary>
        [Serializable]
        public class Content {
            #region Content
            [Tooltip("Localized text of this line")]
            public LocalizedString Text = new LocalizedString();

            [Tooltip("Localized audio of this line")]
            public LocalizedAsset<AudioAsset> Audio = new LocalizedAsset<AudioAsset>();
            #endregion
        }

        #region Global Members
        public override string Text {
            get { return Line.Text.GetLocalizedValue(); }
            set { Line.Text.SetLocalizedValue(value); }
        }

        public override float Duration {
            get {
                if (GetAudioFile(out AudioAsset _audio) && _audio.IsValid()) {
                    return _audio.Duration;
                }

                return Text.Length * .05f;
            }
        }
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        public override void GetLocalizationTables(Set<TableReference> _stringTables, Set<TableReference> _assetTables) {
            base.GetLocalizationTables(_stringTables, _assetTables);

            // Get all localization tables used by this node.
            ref Content _line = ref Line;

            if (_line.Text.GetLocalizedTable(out TableReference _table)) {
                _stringTables.Add(_table);
            }

            if (_line.Audio.GetLocalizedTable(out _table)) {
                _assetTables.Add(_table);
            }
        }
        #endif
        #endregion

        #region Utility
        /// <summary>
        /// Get this line audio asset.
        /// </summary>
        /// <param name="_audio">This line audio asset.</param>
        /// <returns>True if an audio asset was successfully found and loaded, false otherwise.</returns>
        public virtual bool GetAudioFile(out AudioAsset _audio) {
            return Line.Audio.GetLocalizedValue(out _audio);
        }
        #endregion
    }
    #endif
}
