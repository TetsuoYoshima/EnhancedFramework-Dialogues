// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedFramework.Timeline;
using UnityEngine.Playables;

namespace EnhancedFramework.Dialogues.Timeline {
    /// <summary>
    /// Base interface to inherit any <see cref="Dialogue"/> <see cref="PlayableAsset"/> from.
    /// </summary>
    public interface IDialoguePlayableAsset { }

    /// <summary>
    /// Base non-generic <see cref="Dialogue"/> <see cref="PlayableAsset"/> class.
    /// </summary>
    public abstract class DialoguePlayableAsset : EnhancedPlayableAsset, IDialoguePlayableAsset { }

    /// <summary>
    /// Base generic class for every <see cref="Dialogue"/> <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EnhancedPlayableBehaviour"/> playable for this asset.</typeparam>
    public abstract class DialoguePlayableAsset<T> : EnhancedPlayableAsset<T, Dialogue>, IDialoguePlayableAsset
                                                     where T : EnhancedPlayableBehaviour<Dialogue>, new() {
        #region Global Members
        public const string NamePrefix = "Dialogue/";
        #endregion
    }
}
