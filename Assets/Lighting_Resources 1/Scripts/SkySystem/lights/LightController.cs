using System;
using SkySystem.time;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SkySystem.lights
{
    [RequireComponent(typeof(Light), typeof(HDAdditionalLightData))]
    public class LightController : SkyEffectBase
    {
        [Range(0, 100000)] public float lightIntensity;
        
        public Color lightColor = Color.white;
        public Gradient lightGradient;
        public float gradientOffset;
        public float volumetricMultiplier;
        public LightShadows lightShadows;
        
        public float lightAngle { get; set; }
        public  float testAngle;

        private Light _lightSource;
        private HDAdditionalLightData _additionalLightData;
        
        private Color _color;
        private float _volumetricMultplier;
        private float _intensity;

        public Light GetLight()
        {
            var l = GetComponent<Light>() as Light;

            return l != null ? l : null;
        }
        private HDAdditionalLightData GetAdditionalData()
        {
            var additionalLightData = GetComponent<HDAdditionalLightData>() as HDAdditionalLightData;

            return additionalLightData != null ? additionalLightData : null;
        }
        private void Start()
        {
            if (_lightSource == null)
                _lightSource = GetLight();

            if (_additionalLightData == null)
                _additionalLightData = GetAdditionalData();
        }

        private void Update()
        {
            if (_lightSource == null)
                return;
            
            var lightDot = Vector3.Dot(transform.forward, Vector3.down);
            lightDot = Mathf.Clamp01(lightDot);
            
            _color = 
                Color.Lerp(_lightSource.color, CalculateLightColor(lightDot + (lightDot <= testAngle ? -gradientOffset : gradientOffset)), Time.deltaTime);
            _lightSource.color = _color;

            _intensity =
                Mathf.Lerp(_lightSource.intensity, lightIntensity, Time.deltaTime);
            _lightSource.intensity = _intensity;

            _volumetricMultplier =
                Mathf.Lerp(_additionalLightData.volumetricDimmer, volumetricMultiplier, Time.deltaTime);
            _additionalLightData.volumetricDimmer = _volumetricMultplier;
        }

        public void SetRotation(Vector3 rotation)
        {
            transform.rotation = Quaternion.Euler(rotation);
        }

        public void Validate(float angle)
        {
            _lightSource = GetLight();
            float gradOffset = (angle <= testAngle ? -gradientOffset : gradientOffset);
            float time = angle + gradOffset;
            _lightSource.color =
                lightColor * CalculateLightColor(time);
        }
        
        private Color CalculateLightColor(float time)
        {
            return lightGradient.Evaluate(time);
        }
    }
}