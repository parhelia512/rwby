namespace rwby
{
    public class StateConditionMapper : HnSF.StateConditionMapperBase
    {
        public StateConditionMapper()
        {
            functions.Add(typeof(ConditionNone), BaseStateConditionFunctions.NoCondition);
            functions.Add(typeof(ConditionMoveStickMagnitude), BaseStateConditionFunctions.MovementStickMagnitude);
            functions.Add(typeof(ConditionFallSpeed), BaseStateConditionFunctions.FallSpeed);
            functions.Add(typeof(ConditionIsGrounded), BaseStateConditionFunctions.IsGrounded);
            functions.Add(typeof(ConditionButton), BaseStateConditionFunctions.Button);
            functions.Add(typeof(ConditionButtonSequence), BaseStateConditionFunctions.ButtonSequence);
            functions.Add(typeof(ConditionAnd), BaseStateConditionFunctions.ANDCondition);
            functions.Add(typeof(ConditionOr), BaseStateConditionFunctions.ORCondition);
            functions.Add(typeof(ConditionCanAirJump), BaseStateConditionFunctions.CanAirJump);
            functions.Add(typeof(ConditionCanAirDash), BaseStateConditionFunctions.CanAirDash);
            functions.Add(typeof(ConditionMoveset), BaseStateConditionFunctions.Moveset);
            functions.Add(typeof(ConditionHitstunValue), BaseStateConditionFunctions.HitstunValue);
            functions.Add(typeof(ConditionBlockstunValue), BaseStateConditionFunctions.BlockstunValue);
            functions.Add(typeof(ConditionInBlockstun), BaseStateConditionFunctions.InBlockstun);
            functions.Add(typeof(ConditionInHitstun), BaseStateConditionFunctions.InHitstun);
            functions.Add(typeof(ConditionLockedOn), BaseStateConditionFunctions.LockedOn);
            functions.Add(typeof(ConditionWallValid), BaseStateConditionFunctions.WallValid);
            functions.Add(typeof(ConditionHoldingTowardsWall), BaseStateConditionFunctions.HoldingTowardsWall);
            functions.Add(typeof(ConditionHitboxHitCount), BaseStateConditionFunctions.HitboxHitCount);
            functions.Add(typeof(ConditionHitOrBlockCount), BaseStateConditionFunctions.HitCount);
            functions.Add(typeof(ConditionFloorAngle), BaseStateConditionFunctions.FloorAngle);
            functions.Add(typeof(ConditionCompareSlopeDir), BaseStateConditionFunctions.CompareSlopeDir);
            functions.Add(typeof(ConditionPoleValid), BaseStateConditionFunctions.PoleValid);
            functions.Add(typeof(ConditionHasThrowees), BaseStateConditionFunctions.HasThrowees);
            functions.Add(typeof(ConditionCompareInputDir), BaseStateConditionFunctions.CompareInputDir);
            functions.Add(typeof(ConditionWallAngle), BaseStateConditionFunctions.WallAngle);
            functions.Add(typeof(ConditionChargeLevel), BaseStateConditionFunctions.ChargeLevel);
            functions.Add(typeof(ConditionCheckSuccessfulBlock), BaseStateConditionFunctions.CheckSuccessfulBlock);
            functions.Add(typeof(ConditionCanGroundBounce), BaseStateConditionFunctions.CanGroundBounce);
            functions.Add(typeof(ConditionCanWallBounce), BaseStateConditionFunctions.CanWallBounce);
            functions.Add(typeof(ConditionWhiteboardIntIntComparison), BaseStateConditionFunctions.WhiteboardIntIntComparison);
            functions.Add(typeof(ConditionWhiteboardBoolean), BaseStateConditionFunctions.WhiteboardBoolean);
            functions.Add(typeof(ConditionButtonHeld), BaseStateConditionFunctions.ButtonHeld);
            functions.Add(typeof(ConditionNextState), BaseStateConditionFunctions.NextState);
            functions.Add(typeof(ConditionHardKnockdown), BaseStateConditionFunctions.HardKnockdown);
            functions.Add(typeof(ConditionExternal), BaseStateConditionFunctions.External);
            functions.Add(typeof(ConditionStickDir), BaseStateConditionFunctions.StickDir);
            functions.Add(typeof(ConditionCheckSuccessfulPushblock), BaseStateConditionFunctions.CheckSuccessfulPushblock);
            functions.Add(typeof(ConditionHasHitstun), BaseStateConditionFunctions.HasHitstun);
            functions.Add(typeof(ConditionCanBurst), BaseStateConditionFunctions.CanBurst);
            functions.Add(typeof(ConditionAuraPercentage), BaseStateConditionFunctions.AuraPercentage);
            functions.Add(typeof(ConditionCheckForCollider), BaseStateConditionFunctions.CheckForCollider);
            functions.Add(typeof(ConditionCanEnemyStep), BaseStateConditionFunctions.CanEnemyStep);
        }
    }
}