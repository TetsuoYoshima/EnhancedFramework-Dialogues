// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using HutongGames.PlayMaker;

namespace EnhancedFramework.Dialogues.PlayMaker {
    /// <summary>
    /// Base <see cref="FsmStateAction"/> for a <see cref="Dialogue"/>.
    /// </summary>
    public abstract class BaseDialogueFSM : FsmStateAction {
        #region Global Members
        public const string CategoryName = "Dialogue";
        #endregion
    }
}
