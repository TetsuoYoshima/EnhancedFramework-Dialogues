// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedEditor;
using System;

namespace EnhancedFramework.Dialogues {
    /// <summary>
    /// <see cref="DialogueNode"/> used to reset a dialogue, like it's never been played before.
    /// </summary>
    [Serializable, DisplayName("Base/Reset")]
    public class DialogueResetNode : DialogueNode {
        #region Global Members
        public override string Description {
            get { return "Resets all nodes in this dialogue, like they've never been played before"; }
        }

        public override string DefaultSpeaker {
            get { return "[RESET]"; }
        }
        #endregion

        #region Behaviour
        public override void Play(DialoguePlayer _player) {
            _player.Dialogue.ResetNodes();
            _player.PlayNextNode();
        }
        #endregion

        #region Editor Utility
        protected internal override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "Grid.EraserTool";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return 1;
        }

        protected internal override string GetEditorDisplayedText() {
            return "RESET";
        }
        #endregion
    }
}
