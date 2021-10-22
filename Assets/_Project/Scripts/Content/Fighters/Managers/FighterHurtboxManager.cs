using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class FighterHurtboxManager : NetworkBehaviour
    {
        public HurtboxCollection Hurtboxes { get { return hurtboxCollection; } }

        [SerializeField] protected FighterCombatManager combatManager;
        [SerializeField] protected HurtboxCollection hurtboxCollection;
        public HitboxRoot hRoot;

        public Settings settings;

        [Networked] public int CurrentHurtboxIdentifier { get; set; }

        public Hurtbox[] hurtboxes;

        public void Awake()
        {
            settings = GameManager.singleton.settings;
            foreach(var hb in hurtboxes)
            {
                hb.hurtableNetworkObject = combatManager.GetComponent<NetworkObject>();
                hb.hurtable = combatManager;
            }
        }

        public void CreateHurtboxes(int frame, int hurtboxIdentifier)
        {
            CurrentHurtboxIdentifier = hurtboxIdentifier;
            ResetHurtboxes();
            StateHurtboxDefinition shd = GetHurtboxDefinition();

            int hurtboxCount = GetHurtboxCount(shd);
            CheckHurtboxCount(hurtboxCount);
            UpdateHurtboxes(shd);
        }

        public StateHurtboxDefinition GetHurtboxDefinition()
        {
            return Hurtboxes.GetHurtbox(CurrentHurtboxIdentifier);
        }

        public void ResetHurtboxes()
        {
            foreach(Hurtbox hb in hurtboxes)
            {
                hRoot.SetHitboxActive(hb, false);
            }
        }

        private void CheckHurtboxCount(int hurtboxCount)
        {
            if(hurtboxCount < hurtboxes.Length)
            {
                for(int i = hurtboxes.Length-1; i >= hurtboxCount; i--)
                {
                    hRoot.SetHitboxActive(hurtboxes[i], false);
                }
            }else if (hurtboxCount >= hurtboxes.Length)
            {
                Debug.LogError($"More collboxes requested than available for {gameObject}");
            }
        }

        private int GetHurtboxCount(StateHurtboxDefinition shd)
        {
            int counter = 0;
            for(int i = 0; i < shd.hurtboxGroups.Count; i++)
            {
                counter += shd.hurtboxGroups[i].boxes.Count;
            }
            return counter;
        }

        private void UpdateHurtboxes(StateHurtboxDefinition shd)
        {
            int counter = 0;
            for(int i = 0; i < shd.hurtboxGroups.Count; i++)
            {
                 for(int j = 0; j < shd.hurtboxGroups[i].boxes.Count; j++)
                {
                    UpdateHurtbox(shd.hurtboxGroups[i], (HnSF.Combat.BoxDefinition)shd.hurtboxGroups[i].boxes[j], (Hurtbox)hRoot.Hitboxes[counter]);
                    counter++;
                }
            }
        }

        private void UpdateHurtbox(HnSF.Combat.HurtboxGroup hurtboxGroup, HnSF.Combat.BoxDefinition boxDef, Hurtbox hitbox)
        {
            switch (boxDef.shape)
            {
                case HnSF.Combat.BoxShape.Rectangle:
                    hitbox.Type = HitboxTypes.Box;
                    hitbox.BoxExtents = boxDef.size/2.0f;
                    break;
                case HnSF.Combat.BoxShape.Circle:
                    hitbox.Type = HitboxTypes.Sphere;
                    hitbox.SphereRadius = boxDef.radius;
                    break;
            }
            hitbox.Offset = boxDef.offset;
            hitbox.hurtboxGroup = hurtboxGroup;
            hitbox.Root.SetHitboxActive(hitbox, true);
        }
    }
}