using HnSF;
using UnityEngine;

namespace rwby
{
    [StateVariable("Charge/Increment")]
    public struct VarIncrementChargeLevel : IStateVariables
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
         public IConditionVariables Condition { get => condition; set => condition = value; }
        public bool RunDuringHitstop { get => runDuringHitstop; set => runDuringHitstop = value; }
        public bool runDuringHitstop;

        public int increment;
        public int chargePerLevel;
        public int maxLevel;
        public bool checkAbilityButton;
        public PlayerInputType button;
        public bool canHold;

        public IStateVariables Copy()
        {
            return new VarIncrementChargeLevel()
            {
                increment = increment,
                chargePerLevel = chargePerLevel,
                maxLevel = maxLevel,
                checkAbilityButton = checkAbilityButton,
                button = button,
                canHold = canHold,
            };
        }
    }
}