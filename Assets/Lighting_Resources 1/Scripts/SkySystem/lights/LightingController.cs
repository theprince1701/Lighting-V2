using SkySystem.time;
using UnityEngine;

namespace SkySystem.lights
{
    public class LightingController : SkyEffectBase
    {
        [SerializeField] private LightController sunController;
        [SerializeField] private LightController moonController;
        
        private LightController FindSunController()
        {
            var sun = GameObject.FindGameObjectWithTag("Sun").GetComponent<LightController>();

            return sun != null ? sun : null;
        }
        
        private LightController FindMoonController()
        {
            var moon = GameObject.FindGameObjectWithTag("Moon").GetComponent<LightController>();

            return moon != null ? moon : null;
        }

        private void Start()
        {
            if (sunController == null)
                sunController = FindSunController();

            if (moonController == null)
                moonController = FindMoonController();
        }

        protected override void UpdateEffect(SkyStates time)
        {
            if (time.moon)
            {
                moonController.lightIntensity = time.lightIntensity;
                moonController.volumetricMultiplier = time.lightVolumetricMultiplier;
            }
            else if (time.sun)
            {
                sunController.lightIntensity = time.lightIntensity;
                sunController.volumetricMultiplier = time.lightVolumetricMultiplier;
            }
        }
        
        public void Validate(SkyStates state, TimeController time)
        {
            if (state == null)
                return;
            
            sunController = FindSunController();
            moonController = FindMoonController();

            var sunDot = Vector3.Dot(sunController.transform.forward, Vector3.down);
            sunDot = Mathf.Clamp01(sunDot);
            sunController.Validate(sunDot);
            
            var moonDot = Vector3.Dot(moonController.transform.forward, Vector3.down);
            moonDot = Mathf.Clamp01(moonDot);
            moonController.Validate(moonDot);
        }
    }
}