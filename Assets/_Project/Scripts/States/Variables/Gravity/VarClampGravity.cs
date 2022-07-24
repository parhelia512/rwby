using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Gravity/Clamp Gravity")]
    public struct VarClampGravity : IStateVariables
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
        private int[] children;
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

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeField, SerializeReference]
        public FighterStatReferenceFloatBase minValue;

        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeField, SerializeReference]
        public FighterStatReferenceFloatBase maxValue;
    }
}