// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Dialogues {
    /// <summary>
    /// <see cref="Dialogue"/>-related game database.
    /// </summary>
    public sealed class DialogueDatabase : BaseDatabase<DialogueDatabase>, IPreprocessCallback {
        #region Global Members
        [Section("Dialogue Database")]

        [Tooltip("All dialogues in this database")]
        [SerializeField] private EnhancedCollection<Dialogue> dialogues = new EnhancedCollection<Dialogue>();

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Use this to search for any specific node type")]
        [SerializeField] private SerializedType<DialogueNode> searchType = new SerializedType<DialogueNode>(SerializedTypeConstraint.Null | SerializedTypeConstraint.Abstract);
        #endregion

        #region Dialogue
        /// <summary>
        /// Finds the first <see cref="Dialogue"/> in the database matching a given name.
        /// </summary>
        /// <param name="_name">Name of the <see cref="Dialogue"/> to find.</param>
        /// <param name="_dialogue">Matching <see cref="Dialogue"/> with the given name (null if none).</param>
        /// <returns>True if a <see cref="Dialogue"/> with the given name could be successfully found, false otherwise.</returns>
        public bool FindDialogue(string _name, out Dialogue _dialogue) {
            ref List<Dialogue> _span = ref dialogues.collection;
            for (int i = _span.Count; i-- > 0;) {

                _dialogue = _span[i];
                if (_dialogue.name.RemovePrefix().ToLower().EqualOrdinal(_name.RemovePrefix().ToLower())) {
                    return true;
                }
            }

            _dialogue = null;
            return false;
        }

        /// <summary>
        /// Resets all dialogues in the database.
        /// </summary>
        public void ResetDialogues() {
            ref List<Dialogue> _span = ref dialogues.collection;
            for (int i = _span.Count; i-- > 0;) {
                _span[i].ResetForNextPlay();
            }
        }
        #endregion

        #region Database
        /// <summary>
        /// Set all <see cref="Dialogue"/> in this database.
        /// </summary>
        /// <param name="_dialogues">All <see cref="Dialogue"/> to include in this database.</param>
        internal void SetDatabase(IList<Dialogue> _dialogues) {
            dialogues.ReplaceBy(_dialogues);
        }

        // -------------------------------------------
        // Preprocess
        // -------------------------------------------

        bool IPreprocessCallback.OnPreprocess() {
            SetDatabase(PreprocessManager.Load<Dialogue>());
            return true;
        }
        #endregion        

        #region Utility
        /// <summary>
        /// Utility method used to search for specific node(s) in all game dialogues,
        /// and logging an informative message for each matching node that is found.
        /// </summary>
        /// <param name="_inherit">If true, also searches for all types that inherit from the given type.</param>
        [Button(ActivationMode.Always, SuperColor.Crimson)]
        public void SearchForNodes(bool _inherit = true) {

            ref List<Dialogue> _span = ref dialogues.collection;
            Type _type = searchType.Type;

            for (int i = _span.Count; i-- > 0;) {

                Dialogue _dialogue = _span[i];
                DoFindNode(_dialogue, _dialogue.Root);
            }

            // ----- Local Method ----- \\

            void DoFindNode(Dialogue _dialogue, DialogueNode _root) {

                ref DialogueNode[] _nodes = ref _root.nodes;
                for (int i = _nodes.Length; i-- > 0;) {

                    DialogueNode _innerNode = _nodes[i];

                    if ((_innerNode.GetType() == _type) || (_inherit && _inherit.GetType().IsSubclassOf(_type))) {
                        Debug.LogWarning($"Found Node => {_innerNode.Text} - {_dialogue.name} [{_innerNode.GetType().Name}]");
                    }

                    if (!_root.ShowNodes) {
                        continue;
                    }

                    DoFindNode(_dialogue, _innerNode);
                }
            }
        }
        #endregion
    }
}
