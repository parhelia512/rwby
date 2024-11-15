using Cysharp.Threading.Tasks;
using Fusion;
using HnSF.Fighters;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(FighterStateManager), typeof(Fusion.HitboxManager), typeof(FighterPhysicsManager), typeof(FighterBoxManager), typeof(FighterHitManager), typeof(FighterCombatManager))]
    [OrderAfter(typeof(FighterInputManager))]
    public class FighterManager : NetworkBehaviour, IFighterBase, ITargetable, IContentLoad
    {
        public IFighterCombatManager CombatManager
        {
            get { return combatManager; }
        }
        public IFighterStateManager StateManager
        {
            get { return stateManager; }
        }
        public IFighterPhysicsManager PhysicsManager
        {
            get { return physicsManager; }
        }
        public FighterInputManager InputManager { get { return inputManager; } }
        public FighterCombatManager FCombatManager { get { return combatManager; } }
        public FighterStateManager FStateManager { get { return stateManager; } }
        public FighterStatManager StatManager { get { return statManager; } }
        public FighterPhysicsManager FPhysicsManager { get { return physicsManager; } }
        public FighterHealthManager HealthManager{ get { return healthManager; } }
        public FighterBoxManager BoxManager { get { return boxManager; } }
        public SoundbankContainer SoundbankContainer { get { return soundbankContainer; } }
        public Transform TargetOrigin { get { return targetOrigin; } }
        public IEnumerable<ModIDContentReference> loadedContent
        {
            get { return GetLoadedContentList(); }
        }
        
        [Networked][field: HideInInspector] public bool TargetableNetworked { get; set; }
        public bool Targetable { get { return TargetableNetworked; } }

        [Networked][field: HideInInspector] public NetworkBool HardTargeting { get; set; }
        [Networked][field: HideInInspector] public NetworkObject CurrentTarget { get; set; }
        [Networked][field: HideInInspector] public NetworkBool Visible { get; set; }
        [Networked][field: HideInInspector] public NetworkBool Process { get; set; } = true;
        [Networked][field: HideInInspector] public NetworkBehaviour callbacks { get; set; }
        [Networked][field: HideInInspector] public int lastHurtByTeam { get; set; }
        [Networked][field: HideInInspector] public int lastHealByTeam { get; set; }
        
        // Stats
        [Networked][field: HideInInspector] public NetworkBool StoredRun { get; set; }
        [Networked][field: HideInInspector] public int CurrentJump { get; set; }
        [Networked][field: HideInInspector] public int CurrentAirDash { get; set; }

        [Header("Debug")] 
        public bool FRAMEBYFRAME = false;

        [Networked] public bool DisableUpdate { get; set; } = false;

        [Header("References")] 
        [NonSerialized] public NetworkManager networkManager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterCombatManager combatManager;
        [SerializeField] protected FighterStateManager stateManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterBoxManager boxManager;
        [SerializeField] protected FighterStatManager statManager;
        [SerializeField] protected FighterHealthManager healthManager;
        public IFighterDefinition fighterDefinition;
        [SerializeField] protected SoundbankContainer soundbankContainer;
        public FighterEffector fighterEffector;
        public FighterAnimator fighterAnimator;
        public FighterSounder fighterSounder;
        public FighterWhiteboard fighterWhiteboard;
        public FighterProjectileManager projectileManager;
        [SerializeField] protected Transform targetOrigin;
        public Transform visualTransform;
        public Transform visualModelTransform;
        public Transform myTransform;
        [SerializeReference] public IContentLoad[] contentLoaders = new IContentLoad[0];

        [Header("Lock On")]
        public LayerMask lockonLayerMask;
        public LayerMask lockonVisibilityLayerMask;
        public float lockonMaxDistance = 20;
        public float lockonFudging = 0.1f;
        private Collider[] lockonResultList = new Collider[10];

        [Header("Walls")]
        public LayerMask wallLayerMask;
        public LayerMask poleLayerMask;
        [HideInInspector] public RaycastHit[] wallHitResults = new RaycastHit[8];

        [Networked][field: HideInInspector] public Vector3 cWallNormal { get; set; }
        [Networked][field: HideInInspector] public Vector3 cWallPoint { get; set; }
        [Networked][field: HideInInspector] public int cWallSide { get; set; }

        [Header("Pole")]
        [HideInInspector] public Pole foundPole;
        [HideInInspector] public Collider[] colliderBuffer = new Collider[1];
        [Networked][field: HideInInspector] public float poleMagnitude { get; set; }
        [Networked][field: HideInInspector] public float poleSpin { get; set; }

        [Networked] public CmaeraShakeDefinition shakeDefinition { get; set; }
        [Networked(OnChanged = nameof(OnChangedCameraMode))][field: HideInInspector] public int cameraMode { get; set; }

        public delegate void EmptyDelegate(FighterManager fighterManager);
        public event EmptyDelegate OnCameraModeChanged;
        
        public float poleSpinLaunchForce = 10.0f;

        public Transform[] boneRefs;

        [Networked] public int CurrentEnemyStep { get; set; }
        public Collider[] colliderHitResults = new Collider[8];

        [Header("FOOTSTEP SOUNDS")]
        public SoundReference[] footstepsDirt;
        public SoundReference[] footstepsSnow;
        public SoundReference[] footstepsWood;
        public SoundReference[] footstepsLeaves;
        

        public static void OnChangedCameraMode(Changed<FighterManager> changed)
        {
            changed.Behaviour.OnCameraModeChanged?.Invoke(changed.Behaviour);
        }

        public virtual async UniTask<bool> OnFighterLoaded()
        {
            return true;
        }

        public virtual void Awake()
        {
            networkManager = NetworkManager.singleton;
            stateManager.movesets = fighterDefinition.GetMovesets();
            foreach (var moveset in stateManager.movesets)
            {
                (moveset as Moveset).Initialize();
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            HandleRespawn();
        }

        [Header("Hitstop")]
        public AnimationCurve xShakeCurve;
        public AnimationCurve yShakeCurve;
        public AnimationCurve falloffCurve;
        public int zShakeOffset = 2;
        public float HorizontalShakeBaseMultiplier = 0.15f;
        public float VerticalShakeBaseMultiplier = 0.1f;
        public int shakeCurveDivi = 4;
        
        public override void Render()
        {
            base.Render();
            visualModelTransform.localPosition = Vector3.zero;
            if (FCombatManager.HitStop > 0 && FCombatManager.HitStun > 0)
            {
                shakeCurveDivi = FCombatManager.LastHitStop / 5;
                shakeCurveDivi = Mathf.Clamp(shakeCurveDivi, 4, int.MaxValue);
                var hst = (FCombatManager.LastHitStop - FCombatManager.HitStop);
                var xCalcOffset = xShakeCurve.Evaluate((float)(hst % shakeCurveDivi) / (float)shakeCurveDivi) 
                                  * falloffCurve.Evaluate(hst / (float)FCombatManager.LastHitStop)
                                  * HorizontalShakeBaseMultiplier;
                var zCalcOffset = xShakeCurve.Evaluate(((float)(hst+zShakeOffset) % shakeCurveDivi) / (float)shakeCurveDivi) 
                                  * falloffCurve.Evaluate(hst / (float)FCombatManager.LastHitStop)
                                  * HorizontalShakeBaseMultiplier;
                var yCalcOffset = yShakeCurve.Evaluate((float)(hst % shakeCurveDivi) / (float)shakeCurveDivi) 
                                  * falloffCurve.Evaluate(hst / (float)FCombatManager.LastHitStop)
                                  * VerticalShakeBaseMultiplier;

                Vector3 dir = zCalcOffset * transform.forward
                              + xCalcOffset * transform.right
                              + (physicsManager.IsGrounded ? Vector3.zero : yCalcOffset * transform.up);

                visualModelTransform.localPosition = dir;
                //physicsManager.kCC.Motor.visualExtraOffset = dir;
            }
        }

        [HideInInspector] public float groundSlopeAngle;
        [HideInInspector] public Vector3 groundSlopeDir;

        public override void FixedUpdateNetwork()
        {
            if (DisableUpdate) return;
            FighterUpdate();
        }

        public virtual void FighterUpdate()
        {
            if (!Process) return;
            combatManager.CounterhitState = false;
            GetFloorAngle();
            inputManager.FeedInput();
            inputManager.Tick();
            if (Runner.Simulation.IsResimulation && Runner.Simulation.IsFirstTick)
            {
                FPhysicsManager.ResimulationResync();
            }

            if (FRAMEBYFRAME && !(Input.GetKeyDown(KeyCode.X) || Input.GetKey(KeyCode.C)))
            {
                FPhysicsManager.Freeze();
                return;
            }
            
            visualTransform.gameObject.SetActive(Visible);
            HandleLockon();
            if (FCombatManager.HitStop > 0)
            {
                FCombatManager.HitStop--;
                FPhysicsManager.Freeze();
                fighterEffector.PauseCurrentEffects();
                FStateManager.Tick();
                return;
            }

            if (FCombatManager.BurstMeter < FighterCombatManager.MAX_BURST)
            {
                FCombatManager.BurstMeter += 1;
            }

            if (FCombatManager.throwLocked)
            {
                if (combatManager.ThrowTechTimer > 0)
                {
                    if (InputManager.GetA(out int boff, 0, 0).firstPress)
                    {
                        FCombatManager.TechThrow();
                    }
                }
                combatManager.ThrowTechTimer--;
                return;
            }
            if(FCombatManager.HitStop == 0) fighterEffector.ResumeCurrentEffects();
            if(FCombatManager.HitStop > -600) FCombatManager.HitStop--;
            if(FCombatManager.BlockStun > -600) FCombatManager.BlockStun--;
            if(FCombatManager.HitStun > -600) FCombatManager.HitStun--;
            
            BoxManager.ResetAllBoxes();
            FPhysicsManager.CheckIfGrounded();
            FStateManager.Tick();
            FPhysicsManager.Tick();
            FCombatManager.Tick();
        }

        private void GetFloorAngle()
        {
            groundSlopeAngle = 0;
            groundSlopeDir = Vector3.zero;
            // SPHERECAST
            // "Casts a sphere along a ray and returns detailed information on what was hit."
            if (Runner.GetPhysicsScene().SphereCast(myTransform.position + Vector3.up, 0.5f, Vector3.down, out wallHitResults[0], 1.5f, wallLayerMask))
            {
                // Angle of our slope (between these two vectors). 
                // A hit normal is at a 90 degree angle from the surface that is collided with (at the point of collision).
                // e.g. On a flat surface, both vectors are facing straight up, so the angle is 0.
                groundSlopeAngle = Vector3.Angle(Vector3.up, wallHitResults[0].normal);

                // Find the vector that represents our slope as well. 
                //  temp: basically, finds vector moving across hit surface 
                Vector3 temp = Vector3.Cross(wallHitResults[0].normal, Vector3.down);
                //  Now use this vector and the hit normal, to find the other vector moving up and down the hit surface
                groundSlopeDir = Vector3.Cross(temp, wallHitResults[0].normal); 
            }
        }

        protected void HitstopShake()
        {
            /*
            // Shake during hitstop.
            if (FCombatManager.HitStop != 0
                && (FCombatManager.HitStun > 0 || FCombatManager.BlockStun > 0)
                && FCombatManager.HitStop % hitstopShakeFrames == 0)
            {
                Vector3 dir = shakeDirs[currentShakeDirection].z * transform.forward
                    + shakeDirs[currentShakeDirection].x * transform.right;
                hitstopDir *= -1;
                
                if(hitstopDir == 1)
                {
                    currentShakeDirection++;
                    if(currentShakeDirection >= shakeDirs.Length)
                    {
                        currentShakeDirection = 0;
                    }
                }
            }*/
        }
        
        private void HandleLockon()
        {
            if(HardTargeting == false)
            {
                TryLockon();
            }
            else
            {
                TryLockoff();
            }
        }

        private void TryLockon()
        {
            if (inputManager.GetLockOn(out int bOffset).firstPress == false) return;
            PickLockonTarget(lockonMaxDistance);
            HardTargeting = true;
        }

        private void TryLockoff()
        {
            float dist = 0;
            if(CurrentTarget != null)
            {
                dist = Vector3.Distance(transform.position, CurrentTarget.transform.position);
            }
            if (dist <= lockonMaxDistance + 0.5f && inputManager.GetLockOn(out int buttonOffset).isDown == true) return;
            //if ((dist <= lockonMaxDistance + 0.5f) 
            //    && (inputManager.GetLockOn(out int bOffset).isDown == false && (CurrentTarget != null && CurrentTarget.GetComponent<ITargetable>().Targetable == true)) ) return;
            CurrentTarget = null;
            HardTargeting = false;
        }

        public void PickLockonTarget(float maxDistance)
        {
            CurrentTarget = null;
            var target = GetLockonTarget(maxDistance);
            if(target != null)
            {
                CurrentTarget = target.GetComponent<NetworkObject>();
            }
        }
        
        public GameObject GetLockonTarget(float maxDistance)
        {
            int hitCount = Runner.GetPhysicsScene().OverlapSphere(GetCenter(), maxDistance, lockonResultList, 
                lockonLayerMask, QueryTriggerInteraction.UseGlobal);
            // The direction of the lockon defaults to the forward of the camera.
            Vector3 referenceDirection = GetMovementVector(0, 1);
            // If the movement stick is pointing in a direction, then our lockon should
            // be based on that angle instead.
            Vector2 movementDir = InputManager.GetMovement(0);
            if (movementDir.magnitude >= InputConstants.movementDeadzone)
            {
                referenceDirection = GetMovementVector(movementDir.x, movementDir.y);
            }

            // Loop through all targets and find the one that matches the angle the best.
            GameObject closestTarget = null;
            float closestAngle = -2.0f;
            float closestDistance = Mathf.Infinity;
            for(int i = 0; i < hitCount; i++)
            {
                Collider c = lockonResultList[i];
                // Ignore self.
                if (c.transform.parent.gameObject == gameObject)
                {
                    continue;
                }
                // Only objects with ILockonable can be locked on to.
                if (c.transform.parent.TryGetComponent(out ITargetable targetLockonComponent) == false)
                {
                    continue;
                }
                // The target can not be locked on to right now.
                if (targetLockonComponent.Targetable == false)
                {
                    continue;
                }

                Vector3 targetDistance = targetLockonComponent.GetBounds().center - GetBounds().center;
                // If we can't see the target, it can not be locked on to.
                if (Physics.Raycast(GetBounds().center, targetDistance.normalized, out RaycastHit h, targetDistance.magnitude, lockonVisibilityLayerMask))
                {
                    continue;
                }

                targetDistance.y = 0;
                float currAngle = Vector3.Dot(referenceDirection, targetDistance.normalized);
                bool withinFudging = Mathf.Abs(currAngle - closestAngle) <= lockonFudging;
                // Targets have similar positions, choose the closer one.
                if (withinFudging)
                {
                    if (targetDistance.sqrMagnitude < closestDistance)
                    {
                        closestTarget = c.transform.parent.gameObject;
                        closestAngle = currAngle;
                        closestDistance = targetDistance.sqrMagnitude;
                    }
                }
                // Target is closer to the angle than the last one, this is the new target.
                else if (currAngle > closestAngle)
                {
                    closestTarget = c.transform.parent.gameObject;
                    closestAngle = currAngle;
                    closestDistance = targetDistance.sqrMagnitude;
                }
            }
            return closestTarget;
        }

        public Vector3 GetCenter()
        {
            return physicsManager.ECBCenter();
        }
        
        public virtual void ClearWall()
        {
            cWallNormal = Vector3.zero;
        }
        
        public virtual void AssignWall(Vector3 inputAngle, RaycastHit hitResult)
        {
            cWallNormal = hitResult.normal;
            cWallPoint = hitResult.point;
            if (cWallNormal == Vector3.zero) return;
            float angle = Vector3.SignedAngle(inputAngle, -hitResult.normal, Vector3.up);
            cWallSide = angle < 0 ? -1 : 1;
        }

        public virtual bool IsWallValid()
        {
            return cWallNormal != Vector3.zero;
        }

        public virtual void ResetVariablesOnGround()
        {
            CurrentEnemyStep = 0;
            CurrentAirDash = 0;
            CurrentJump = 0;
            FPhysicsManager.forceGravity = 0;
            combatManager.ResetString();
        }


        /// <summary>
        /// Translates the movement vector based on the look transform's forward.
        /// </summary>
        /// <param name="frame">The frame we want to check the movement input for.</param>
        /// <returns>A direction vector based on the camera's forward.</returns>
        public virtual Vector3 GetMovementVector(int frame = 0)
        {
            Vector2 movement = InputManager.GetMovement(frame);
            if(movement == Vector2.zero)
            {
                return Vector3.zero;
            }
            return GetMovementVector(movement.x, movement.y);
        }

        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public virtual Vector3 GetMovementVector(float horizontal, float vertical)
        {
            Vector3 forward = inputManager.GetCameraForward();
            Vector3 right = inputManager.GetCameraRight();

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            return forward * vertical + right * horizontal;
        }

        public virtual Vector3 GetVisualMovementVector(int frame = 0)
        {
            Vector2 movement = ExtDebug.DeadZoner(InputManager.GetMovement(frame), InputConstants.movementDeadzone);
            if(movement == Vector2.zero)
            {
                return Vector3.zero;
            }
            return GetVisualMovementVector(movement.x, movement.y);
        }

        public virtual Vector3 GetVisualMovementVector(float horizontal, float vertical)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            return forward * vertical + right * horizontal;
        }

        public virtual void RotateTowards(Vector3 direction, float speed)
        {
            Vector3 newDir = Vector3.RotateTowards(transform.forward, direction, speed * Runner.DeltaTime, 0.0f);
            physicsManager.ForceSetRotation(Quaternion.LookRotation(newDir), false);//.kCC.Motor.SetRotation(Quaternion.LookRotation(newDir), false);
        }

        public void SetRotation(Vector3 direction, bool bypassInterpolation = true)
        {
            physicsManager.SetRotation(direction, bypassInterpolation);
        }
        
        public void SetTargetable(bool value)
        {
            TargetableNetworked = value;
        }

        public virtual void Hurt(int hurterTeam, int amt)
        {
            lastHurtByTeam = hurterTeam;
            var oldVal = healthManager.Health;
            healthManager.ModifyHealth(amt);
            ((IFighterCallbacks)callbacks).FighterHealthChanged(this, oldVal);
        }

        public virtual void Heal(int healerTeam, int amt)
        {
            lastHealByTeam = healerTeam;
            healthManager.ModifyHealth(amt);
        }

        public virtual void HandleDeath()
        {
            TargetableNetworked = false;
            Visible = false;
            Process = false;
            combatManager.SetHitStop(0);
            combatManager.SetHitStun(0);
            combatManager.ResetProration();
            combatManager.ResetString();
            combatManager.ResetCharge();
            combatManager.HitboxManager.Reset();
            boxManager.ResetAllBoxes();
            physicsManager.ResetForces();
        }

        public virtual void HandleRespawn()
        {
            TargetableNetworked = true;
            Visible = true;
            Process = true;
            combatManager.Cleanup();
            stateManager.SetMoveset(0);
            statManager.SetupStats((stateManager.movesets[0] as Moveset).fighterStats);
            combatManager.Aura = fighterDefinition.Aura;
            combatManager.BurstMeter = FighterCombatManager.MAX_BURST;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public Bounds GetBounds()
        {
            return physicsManager.cc.bounds;
        }

        public virtual List<ModIDContentReference> GetLoadedContentList()
        {
            List<ModIDContentReference> references = new List<ModIDContentReference>();

            foreach (var contentLoad in contentLoaders)
            {
                references.AddRange(contentLoad.loadedContent);
            }
            
            return references;
        }
    }
}