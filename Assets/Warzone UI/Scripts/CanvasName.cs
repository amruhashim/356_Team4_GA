using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace solidocean
{
    [CreateAssetMenu(menuName = "Last UI/Canvas Editor/New Canvas")]
    public class CanvasName : ScriptableObject
    {
        [Tooltip("If you do not want to go back from this canvas, check this bool as false (For example, you cannot go back from the \"Press Any Button\" Canvas.)")]
        public bool canGoPreviousCanvas;
    }
}
