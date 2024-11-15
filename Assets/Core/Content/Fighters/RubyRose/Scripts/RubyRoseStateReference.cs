using HnSF;

namespace rwby.core
{
    [System.Serializable]
    public class RubyRoseStateReference :  FighterStateReferenceBase
    {
        public RubyRoseStates state = default;
        
        public override int GetState()
        {
            return (int)state;
        }

        public override FighterStateReferenceBase Copy()
        {
            return new RubyRoseStateReference()
            {
                state = state
            };
        }
    }
}