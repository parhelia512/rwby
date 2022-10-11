using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionNextState : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.NONE;

        public int stateMovesetID;
        [SelectImplementation(typeof(FighterStateReferenceBase))] [SerializeField, SerializeReference]
        public FighterStateReferenceBase state;
        public bool inverse;
    }
}