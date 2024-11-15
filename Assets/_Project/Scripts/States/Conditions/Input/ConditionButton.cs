using HnSF;

namespace rwby
{
    [System.Serializable]
    public struct ConditionButton : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.BUTTON;

        public enum ButtonStateType
        {
            IsDown = 0,
            FirstPress = 1,
            Released = 2
        }

        public bool inverse;
        public bool checkAbilityButton;
        public PlayerInputType button;
        public ButtonStateType buttonState;
        public int offset;
        public int buffer;

        public IConditionVariables Copy()
        {
            return new ConditionButton()
            {
                inverse = inverse,
                checkAbilityButton = checkAbilityButton,
                button = button,
                buttonState = buttonState,
                offset = offset,
                buffer = buffer
            };
        }
    }
}