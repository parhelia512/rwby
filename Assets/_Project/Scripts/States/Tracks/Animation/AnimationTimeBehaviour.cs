using HnSF;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AnimationTimeBehaviour : FighterStateBehaviour
    {
        public ForceSetType setType;
        public bool timeInFrames = false;
        public int layer;
        public int index;
        public float time;
        public int frameTime;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterAnimator animator = (playerData as FighterManager).fighterAnimator;
            switch (setType)
            {
                case ForceSetType.SET:
                    animator.SetAnimationTime(layer, index, timeInFrames ? frameTime * animator.Runner.DeltaTime : time);
                    break;
                case ForceSetType.ADD:
                    animator.AddAnimationTime(layer, index, timeInFrames ? frameTime * animator.Runner.DeltaTime : time);
                    break;
            }
        }
    }
}