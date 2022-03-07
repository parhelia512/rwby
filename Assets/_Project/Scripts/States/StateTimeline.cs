using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "StateTimeline", menuName = "rwby/StateTimeline")]
    public class StateTimeline : HnSF.StateTimeline
    {
        public bool autoIncrement;
        public bool autoLoop;
        public int loopFrame = 1;
    }
}