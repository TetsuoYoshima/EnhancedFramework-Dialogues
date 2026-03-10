// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

#if LOCALIZATION_PACKAGE
#define LOCALIZATION_ENABLED
#endif

using EnhancedFramework.Core;
using System;
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization.Tables;
#endif

namespace EnhancedFramework.Dialogues {
    /// <summary>
    /// Behaviour used to select the next node to play from a <see cref="DialoguePlayer"/>.
    /// </summary>
    public enum NextNodeBehaviour {
        None = 0,

        PlayFirst = 1,
        PlayLast  = 2,
        Random    = 9,
    }

    // ===== Settings ===== \\

    /// <summary>
    /// Base class for any <see cref="Dialogue"/>-related configurable settings.
    /// <br/> You can inherit from <see cref="DialogueSettings{T}"/> for a quick implementation.
    /// </summary>
    [Serializable]
    public abstract class DialogueSettings
                                            #if LOCALIZATION_ENABLED
                                            : ILocalizable
                                            #endif
    {
        #region Global Members
        [Tooltip("Default behaviour used to determine the next node to play")]
        public NextNodeBehaviour NextNodeBehaviour = NextNodeBehaviour.PlayFirst;

        // -----------------------

        /// <summary>
        /// Total count of speakers in the dialogue.
        /// </summary>
        public abstract int SpeakerCount { get; }
        #endregion

        #region Speaker
        /// <summary>
        /// Get the name of the speaker at a given index.
        /// </summary>
        /// <param name="_index">Index of the speaker to get.</param>
        /// <returns>The name of this speaker.</returns>
        public abstract string GetSpeakerAt(int _index);
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        /// <inheritdoc cref="ILocalizable.GetLocalizationTables(Set{TableReference}, Set{TableReference})"/>
        public virtual void GetLocalizationTables(Set<TableReference> _stringTables, Set<TableReference> _assetTables) { }
        #endif
        #endregion
    }

    /// <summary>
    /// <see cref="DialogueSettings"/> class with a ready-to-use array of speakers.
    /// </summary>
    [Serializable]
    public abstract class DialogueSettings<T> : DialogueSettings {
        #region Global Members
        [Tooltip("All speakers in this dialogue")]
        public T[] Speakers = new T[0];

        // -----------------------

        public override int SpeakerCount {
            get { return Speakers.Length; }
        }
        #endregion

        #region Speaker
        /// <summary>
        /// Get the speaker in these settings at a given index.
        /// </summary>
        /// <param name="_index">Index of the speaker to get.</param>
        /// <returns>The speaker at the given index.</returns>
        public T GetSpeaker(int _index) {
            return Speakers[_index];
        }

        public override string GetSpeakerAt(int _index) {
            T _speaker = GetSpeaker(_index);
            return (_speaker != null) ? _speaker.ToString() : DialogueNode.DefaultSpeakerName;
        }
        #endregion
    }
}
