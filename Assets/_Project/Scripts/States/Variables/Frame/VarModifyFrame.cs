using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Frame/Modify Frame")]
    public struct VarModifyFrame : IStateVariables
    {
        public string name;
        public string Name
        {
            get => name;
            set => name = value;
        }
        public int ID
        {
            get => id;
            set => id = value;
        }
         public IConditionVariables Condition { get => condition; set => condition = value; }
        
        public int Parent
        {
            get => parent;
            set => parent = value;
        }

        public int[] Children
        {
            get => children;
            set => children = value;
        }

        public Vector2Int[] FrameRanges
        {
            get => frameRanges;
            set => frameRanges = value;
        }
    
        [SerializeField, HideInInspector] private int id;
        [SerializeField] public Vector2Int[] frameRanges;
        [SelectImplementation(typeof(IConditionVariables))] [SerializeField, SerializeReference] 
        public IConditionVariables condition;
        public bool RunDuringHitstop { get => runDuringHitstop; set => runDuringHitstop = value; }
        public bool runDuringHitstop;

        public VarModifyType modifyType;
        public int value;
        
        [SerializeField, HideInInspector] private int parent;
        [SerializeField, HideInInspector] private int[] children;

        public IStateVariables Copy()
        {
            return new VarModifyFrame()
            {
                modifyType = modifyType,
                value = value
            };
        }
    }
}