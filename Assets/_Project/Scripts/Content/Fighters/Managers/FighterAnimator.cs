using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Animancer;
using UnityEngine.Profiling;

namespace rwby
{
    [OrderBefore(typeof(CombatPairFinder))]
    [OrderAfter(typeof(FighterStateManager))]
    public class FighterAnimator : NetworkBehaviour
    {
        [HideInInspector] public Dictionary<ModObjectSetContentReference, int> bankMap = new Dictionary<ModObjectSetContentReference, int>();
        [HideInInspector] public List<IAnimationbankDefinition> banks = new List<IAnimationbankDefinition>();
        [SerializeField] private AnimancerComponent animancer;
        
        [SerializeField, Networked] private FighterAnimationRoot animationSet { get; set; }
        [Networked] public bool animateInResimulation { get; set; }
        private bool animDirty = true;
        public bool smoothAnimate = true;

        private FighterAnimationRoot previousAnimationSet;
        private FighterAnimationRoot currentAnimancerRepresentation;

        private void Awake()
        {
            if (animancer == null) return;
            animancer.Playable.PauseGraph();
            animancer.Layers[0].Weight = 1.0f;
            animDirty = true;
        }

        public void SetAnimationSet(int layer, AnimationReference[] wantedAnimations, float fadeTime = 0.0f)
        {
            // TODO: Relook at fading out old animations.
            //UpdateFadeSet(fadeTime);
            ClearAnimationSet(layer);
            AddAnimationToSet(layer, wantedAnimations);
        }

        private void UpdateFadeSet(float fadeTime)
        {
            var temp = animationSet;
            if (Mathf.Approximately(fadeTime, 0.0f) || animationSet.layer0[0].bank == 0)
            {
                ClearFadeLayer();
                temp = animationSet;
                temp.layerFadeAmt = 0;
                temp.layerFadeWeight = 0;
                animationSet = temp;
                return;
            }
            
            temp.layerFadeAmt = 1.0f / fadeTime;
            temp.layerFadeWeight = 1.0f;

            for (int i = 0; i < temp.layer0.Length; i++)
            {
                temp.fadeLayer.Set(i, temp.layer0[i]);
            }
            
            animationSet = temp;
        }

        public void AddAnimationToSet(int layer, AnimationReference[] wantedAnimations)
        {
            for (int i = 0; i < wantedAnimations.Length; i++)
            {
                var temp = animationSet;
                temp.layer0.Set(i, new FighterAnimationNode()
                {
                    bank = bankMap[wantedAnimations[i].animationbank.reference]+1,
                    animation = banks[bankMap[wantedAnimations[i].animationbank.reference]].AnimationMap[wantedAnimations[i].animation]+1,
                    frame = 0,
                    weight = 1.0f
                });
                animationSet = temp;
            }

            animDirty = true;
        }

        public void SetAnimationWeight(int layer, int[] animations, float weight)
        {
            var temp = animationSet;
            for (int i = 0; i < animations.Length; i++)
            {
                var temp2 = temp.layer0[animations[i]];
                temp2.weight = weight;
                temp.layer0.Set(animations[i], temp2);
            }
            animationSet = temp;
            animDirty = true;
        }
        
        public void AddAnimationWeight(int layer, int[] animations, float weight)
        {
            var temp = animationSet;
            for (int i = 0; i < animations.Length; i++)
            {
                var temp2 = temp.layer0[animations[i]];
                temp2.weight = Mathf.Clamp(temp2.weight + weight, 0, 1);
                temp.layer0.Set(animations[i], temp2);
            }
            animationSet = temp;
            animDirty = true;
        }

        public void SetAnimationTime(int layer, int[] animations, int frame)
        {
            var temp = animationSet;
            for (int i = 0; i < animations.Length; i++)
            {
                var temp2 = temp.layer0[animations[i]];
                temp2.frame = frame;
                temp.layer0.Set(animations[i], temp2);
            }
            animationSet = temp;
            animDirty = true;
        }
        
        public void AddAnimationTime(int layer, int[] animations, int frame)
        {
            var temp = animationSet;
            for (int i = 0; i < animations.Length; i++)
            {
                var temp2 = temp.layer0[animations[i]];
                temp2.frame += frame;
                temp.layer0.Set(animations[i], temp2);
            }
            animationSet = temp;
            animDirty = true;
        }
        
        public override void Render()
        {
            base.Render();

            if (smoothAnimate)
            {
                float alpha = Runner.Simulation.StateAlpha;
                for (int i = 0; i < animationSet.layer0.Length; i++)
                {
                    if (previousAnimationSet.layer0[i] != animationSet.layer0[i]) break;
                    if (previousAnimationSet.layer0[i].bank == 0) break;
                    var c = banks[animationSet.layer0[i].bank - 1].Animations[animationSet.layer0[i].animation - 1]
                        .clip;
                    var cState = animancer.Layers[0].GetOrCreateState(c);
                    cState.Weight = (animationSet.layer0[i].weight) * alpha
                                    + (previousAnimationSet.layer0[i].weight) * (1.0f - alpha);
                    cState.Time = (animationSet.layer0[i].frame * Runner.DeltaTime) * alpha
                                  + (previousAnimationSet.layer0[i].frame * Runner.DeltaTime) * (1.0f - alpha);
                }

                animancer.Evaluate();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!animateInResimulation && Runner.IsResimulation)
            {
                if (Runner.IsLastTick && currentAnimancerRepresentation != animationSet)
                {
                    SyncAnimancer();
                    animDirty = false;
                }
                return;
            }
            
            if (Runner.IsResimulation && Runner.IsFirstTick && currentAnimancerRepresentation != animationSet)
            {
                SyncAnimancer();
                animDirty = false;
            }

            if (!Mathf.Approximately(animationSet.layerFadeWeight, 0.0f))
            {
                var temp = animationSet;
                temp.layerFadeWeight = Mathf.Clamp(temp.layerFadeWeight-(temp.layerFadeAmt * Runner.DeltaTime), 0.0f, 1.0f);
                animDirty = true;
                animationSet = temp;
            }

            if (animDirty)
            {
                previousAnimationSet = currentAnimancerRepresentation;
                SyncAnimancer();
                animDirty = false;
            }
        }
        
        private void SyncAnimancer()
        {
            currentAnimancerRepresentation = animationSet;
            ClearWeights(0);
            
            for (int i = 0; i < animationSet.layer0.Length; i++)
            {
                if (animationSet.layer0[i].bank == 0) break;
                var c = banks[animationSet.layer0[i].bank-1].Animations[animationSet.layer0[i].animation-1].clip;
                var cState = animancer.Layers[0].GetOrCreateState(c);
                cState.Weight = animationSet.layer0[i].weight;
                cState.Time = animationSet.layer0[i].frame * Runner.DeltaTime;
            }
            
            for (int i = 0; i < animationSet.fadeLayer.Length; i++)
            {
                if (animationSet.fadeLayer[i].bank == 0) break;
                var c = banks[animationSet.fadeLayer[i].bank - 1]
                    .Animations[animationSet.fadeLayer[i].animation - 1].clip;
                var cState = animancer.Layers[3].GetOrCreateState(c);
                cState.Weight = animationSet.fadeLayer[i].weight;
                cState.Time = animationSet.fadeLayer[i].frame * Runner.DeltaTime;
            }

            animancer.Layers[3].Weight = animationSet.layerFadeWeight;
            animancer.Evaluate();
        }

        public AnimationClip GetClip(AnimationReference animation)
        {
            return banks[bankMap[animation.animationbank.reference]].GetAnimation(animation.animation).clip;
        }

        private void ClearWeights(int layer)
        {
            for (int i = 0; i < animancer.Layers[0].ChildCount; i++)
            {
                animancer.Layers[0].GetChild(i).Weight = 0;
            }
            
            for (int i = 0; i < animancer.Layers[3].ChildCount; i++)
            {
                animancer.Layers[3].GetChild(i).Weight = 0;
            }
        }

        private void ClearAnimationSet(int layer)
        {
            var temp = animationSet;
            for (int i = 0; i < temp.layer0.Length; i++)
            {
                temp.layer0.Set(i, new FighterAnimationNode());
            }
            animationSet = temp;
        }
        
        private void ClearFadeLayer()
        {
            var temp = animationSet;
            for (int i = 0; i < temp.fadeLayer.Length; i++)
            {
                temp.fadeLayer.Set(i, new FighterAnimationNode());
            }
            animationSet = temp;
        }

        public void RegisterBank(ModObjectSetContentReference bank)
        {
            if (bankMap.ContainsKey(bank)) return;
            banks.Add(ContentManager.singleton.GetContentDefinition<IAnimationbankDefinition>(
                ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference(bank.modGUID, (int)ContentType.Animationbank, bank.contentGUID))));
            bankMap.Add(bank, banks.Count-1);

            for (int i = 0; i < banks[^1].Animations.Count; i++)
            {
                if (banks[^1].Animations[i].clip == null) continue;
                banks[^1].Animations[i].clip.wrapMode = WrapMode.ClampForever;
                var state = animancer.Layers[0].CreateState(banks[^1].Animations[i].clip);
                state.ApplyAnimatorIK = banks[^1].Animations[i].footIK;
                state.ApplyFootIK = banks[^1].Animations[i].footIK;
            }
        }
    }
}