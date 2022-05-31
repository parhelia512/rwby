using HnSF;
using UnityEngine;

namespace rwby
{
    public struct VarEmpty : IStateVariables
    {
        public int FunctionMap => (int)BaseStateFunctionEnum.EXTERNAL;
        public IConditionVariables Condition => condition;
        public IStateVariables[] Children => children;

        public Vector2[] FrameRanges
        {
            get => frameRanges;
            set => frameRanges = value;
        }
    
        [SerializeField] public Vector2[] frameRanges;
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference] 
        public IConditionVariables condition;

        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference] 
        private IStateVariables[] children;
    }
}