// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedEditor;
using EnhancedFramework.Timeline;
using System.ComponentModel;
using UnityEngine.Timeline;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.Dialogues.Timeline {
    /// <summary>
    /// <see cref="TrackAsset"/> class for every <see cref="IDialoguePlayableAsset"/>.
    /// </summary>
    [TrackColor(.627f, .125f, .941f)] // Purple
    [TrackClipType(typeof(IDialoguePlayableAsset))]
    [TrackBindingType(typeof(Dialogue), TrackBindingFlags.None)]
    [DisplayName("Enhanced Framework/Dialogue Track")]
    public sealed class DialogueTrack : EnhancedTrack { }
}
