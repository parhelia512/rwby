using Fusion;
using HnSF.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class FighterHitManager : NetworkBehaviour, IAttacker
    {
        protected GameManager gameManager;
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;

        public LayerMask hitLayermask;
        public LayerMask grabLayermask;
        public List<LagCompensatedHit> lch = new List<LagCompensatedHit>();

        [Networked, Capacity(10)] public NetworkLinkedList<IDGroupCollisionInfo> hitObjects { get; }

        private void Awake()
        {
            gameManager = GameManager.singleton;
        }

        public virtual void Reset()
        {
            hitObjects.Clear();
            for (int i = 0; i < hitboxGroupHitCounts.Length; i++)
            {
                hitboxGroupHitCounts.Set(i, 0);
                hitboxGroupBlockedCounts.Set(i, 0);
            }
        }

        public virtual bool IsHitHurtboxValid(CustomHitbox atackerHitbox, Hurtbox h)
        {
            if (h.ownerNetworkObject == Object) return false;
            for(int i = 0; i < hitObjects.Count; i++)
            {
                if(hitObjects[i].collisionType == IDGroupCollisionType.Hurtbox
                    && hitObjects[i].hitIHurtableNetID == h.ownerNetworkObject.Id
                    && hitObjects[i].hitByIDGroup == atackerHitbox.definition.HitboxInfo[atackerHitbox.definitionIndex].ID)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox h)
        {
            if (h.ownerNetworkObject == Object) return false;
            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i].collisionType == IDGroupCollisionType.Hitbox
                    && hitObjects[i].hitIHurtableNetID == h.ownerNetworkObject.Id
                    && hitObjects[i].hitByIDGroup == attackerHitbox.definition.HitboxInfo[attackerHitbox.definitionIndex].ID)
                {
                    return false;
                }
            }
            return true;
        }

        
        [Networked, Capacity(10)] public NetworkArray<int> hitboxGroupHitCounts { get; }
        [Networked, Capacity(10)] public NetworkArray<int> hitboxGroupBlockedCounts { get; }
        public virtual void DoHit(CustomHitbox hitbox, Hurtbox enemyHurtbox, HurtInfo hurtInfo)
        {
            hitObjects.Add(new IDGroupCollisionInfo()
            {
                collisionType = IDGroupCollisionType.Hurtbox,
                hitByIDGroup = hitbox.definition.HitboxInfo[hitbox.definitionIndex].ID,
                hitIHurtableNetID = enemyHurtbox.ownerNetworkObject.Id
            });

            HitReaction reaction = (HitReaction)enemyHurtbox.hurtable.Hurt(hurtInfo);

            HitInfo hi = hurtInfo.hitInfo as HitInfo;
            
            switch (reaction.reaction)
            {
                case HitReactionType.HIT:
                    hitboxGroupHitCounts.Set(hitbox.definitionIndex, hitboxGroupHitCounts[hitbox.definitionIndex] + 1);
                    combatManager.SetHitStop(hi.attackerHitstop);
                    /*
                    if (string.IsNullOrEmpty(hi.effectbankName) == false)
                    {
                        BaseEffect be = manager.EffectbankContainer.CreateEffect(enemyHurtbox.transform.position,
                            transform.rotation, hi.effectbankName, hi.effectName);
                        be.PlayEffect();
                    }
                    if (string.IsNullOrEmpty(hi.hitSoundbankName) == false)
                    {
                        manager.SoundbankContainer.PlaySound(hi.hitSoundbankName, hi.hitSoundName);
                    }*/
                    // TODO: Better way of handling camera shake on hit/block/etc.
                    /*
                    if (Runner.IsResimulation == false && Object.HasInputAuthority == true)
                    {
                        PlayerCamera.instance.ShakeCamera(hi.shakeValue, hi.hitstop * Runner.DeltaTime);
                    }*/
                    break;
                case HitReactionType.BLOCKED:
                    hitboxGroupBlockedCounts.Set(hitbox.definitionIndex, hitboxGroupBlockedCounts[hitbox.definitionIndex] + 1);
                    combatManager.SetHitStop(hi.attackerHitstop);
                    /*BaseEffect bb = manager.EffectbankContainer.CreateEffect(enemyHurtbox.transform.position,
                            transform.rotation * Quaternion.Euler(0, 180, 0), "global", "shieldhit1");
                    bb.PlayEffect(true, false);
                   
                    if (string.IsNullOrEmpty(hi.blockSoundbankName) == false)
                    {
                        manager.SoundbankContainer.PlaySound(hi.blockSoundbankName, hi.blockSoundName);
                    }*/
                    break;
                case HitReactionType.AVOIDED:
                    break;
            }
        }

        public virtual void DoClash(CustomHitbox hitbox, CustomHitbox enemyHitbox)
        {
            hitObjects.Add(new IDGroupCollisionInfo()
            {
                collisionType = IDGroupCollisionType.Hitbox,
                hitByIDGroup = hitbox.definition.HitboxInfo[hitbox.definitionIndex].ID,
                hitIHurtableNetID = enemyHitbox.ownerNetworkObject.Id
            });

            combatManager.SetHitStop(17);
        }

        public virtual HurtInfo BuildHurtInfo(CustomHitbox hitbox, Hurtbox hurtbox)
        {
            Vector3 hitPoint = hurtbox.transform.position;
            HitInfo hitInfo = hitbox.definition.HitboxInfo[hitbox.definitionIndex];
            HurtInfo hurtInfo;
            
            // TODO: Attack origin point.
            switch (hitInfo.hitForceRelation)
            {
                case HitboxForceRelation.ATTACKER:
                    hurtInfo = new HurtInfo(hitInfo, hurtbox.definitionIndex,
                        manager.myTransform.position, manager.myTransform.forward, manager.myTransform.right,
                        manager.FPhysicsManager.GetOverallForce(), hitPoint);
                    break;
                case HitboxForceRelation.HITBOX:
                    hurtInfo = new HurtInfo(hitInfo, hurtbox.definitionIndex,
                         hitbox.Position, manager.myTransform.forward, manager.myTransform.right,
                         manager.FPhysicsManager.GetOverallForce(), hitPoint);
                    break;
                case HitboxForceRelation.WORLD:
                    hurtInfo = new HurtInfo(hitInfo, hurtbox.definitionIndex,
                        manager.myTransform.position, Vector3.forward, Vector3.right,
                        (manager.FPhysicsManager as FighterPhysicsManager).GetOverallForce(), hitPoint);
                    break;
                default:
                    hurtInfo = new HurtInfo(hitInfo, hurtbox.definitionIndex,
                        manager.myTransform.position, manager.myTransform.forward, manager.myTransform.right,
                        manager.FPhysicsManager.GetOverallForce(), hitPoint);
                    break;
            }
            return hurtInfo;
        }
    }
}
