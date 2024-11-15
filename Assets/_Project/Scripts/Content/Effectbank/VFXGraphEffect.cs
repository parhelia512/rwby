using Animancer;
using UnityEngine;
using UnityEngine.VFX;


namespace rwby
{
    public class VFXGraphEffect : BaseEffect
    {
        public VisualEffect visualEffect;
        public float totalTime = 5.0f;
        
        private void Awake()
        {
            visualEffect.Play();
            //visualEffect.pause = true;
            /*
            animancer.Layers[0].Weight = 1.0f;
            animancer.Layers[0].Speed = 0;
            animancer.Layers[0].CreateIfNew(clip);
            animancer.Layers[0].GetOrCreateState(clip).Weight = 1.0f;
            animancer.Playable.PauseGraph();*/
        }

        public override void PauseEffect()
        {
            base.PauseEffect();
            visualEffect.pause = true;
        }

        public override void SetFrame(float time)
        {
            visualEffect.pause = false;
            //if (time > totalTime) return;
            /*
            visualEffect.Reinit();
            visualEffect.pause = true;
            visualEffect.Simulate(time);*/

            //visualEffect.Stop();
            //visualEffect.Play();
            //visualEffect.pause = true;
            //visualEffect.Simulate(time);
            /*
            var s = animancer.Layers[0].GetOrCreateState(clip);
            s.Time = time;
            animancer.Evaluate();*/
        }
    }
}