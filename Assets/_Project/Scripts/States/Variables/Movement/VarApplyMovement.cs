using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [StateVariable("Movement/Apply Movement")]
    public struct VarApplyMovement : IStateVariables
    {
        public string name;
        public string Name
        {
            get => name;
            set => name = value;
        }
        [SerializeField, HideInInspector] private int id;
        public int ID
        {
            get => id;
            set => id = value;
        }
        [SerializeField, HideInInspector] private int parent;
        public int Parent
        {
            get => parent;
            set => parent = value;
        }
        [SerializeField, HideInInspector] private int[] children;
        public int[] Children
        {
            get => children;
            set => children = value;
        }
        [SerializeField] public Vector2Int[] frameRanges;
        public Vector2Int[] FrameRanges
        {
            get => frameRanges;
            set => frameRanges = value;
        }
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference]
        public IConditionVariables condition;
        public IConditionVariables Condition => condition;

        public VarInputSourceType inputSource;
        public bool normalizeInputSource;
        public bool useRotationIfInputZero;
        
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase baseAccel;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase movementAccel;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase deceleration;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase minSpeed;
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase maxSpeed;
        [SelectImplementation((typeof(FighterStatReferenceBase<AnimationCurve>)))] [SerializeReference]
        public FighterStatReferenceAnimationCurveBase accelerationFromDot;

        [ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeMinClamp;
        [ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeMaxClamp;
        [ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeDivi;
        [ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeMulti;
        [ShowIf("inputSource", VarInputSourceType.slope)]
        public float slopeMultiMax;
    }
}