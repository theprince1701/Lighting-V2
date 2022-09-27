using System;
using SkySystem.time;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Volume))]
public class SkyController : SkyEffectBase
{
    public Vector3 starRotation;
    
    private Volume _volume;
    private PhysicallyBasedSky _sky;
    
    private bool assigned;
    private Color targetHorizonTint;
    private Color targetZenithTint;
    private Color targetGroundTint;
    private float targetEmissionMultiplier;
    
    private Volume FindVolume()
    {
        var v = GetComponent<Volume>();

        return v != null ? v : null;
    }

    private void Start()
    {
        _volume = FindVolume();
        _volume.profile.TryGet(out _sky);
    }

    private void SetRotation(Vector3 rotation)
    {
        _sky.spaceRotation.value = rotation;
    }

    protected override void UpdateEffect(SkyStates state)
    {
        if (state == null)
            return;
        
        targetHorizonTint = state.skyColor;
        targetGroundTint = state.groundColor;
        targetEmissionMultiplier = state.emissionMultiplier;

        _sky.spaceEmissionTexture.value = state.emissionMap;
        assigned = true;
    }

    public void Validate(SkyStates state, TimeController time)
    {
        if (state == null)
            return;
        
        _volume = FindVolume();
        _volume.profile.TryGet(out _sky);

        var totalDaysUtc = time.ComputeStarAngle();
        var rotateX = (totalDaysUtc * starRotation.x * 360.0) % 360.0;
        var rotateY = (totalDaysUtc * starRotation.y * 360.0) % 360.0;
        var rotateZ = (totalDaysUtc * starRotation.z * 360.0) % 360.0;

        SetRotation(new Vector3((float) rotateX, (float) rotateY, (float) rotateZ));
        
        var starAlpha = Mathf.Lerp(_sky.spaceEmissionMultiplier.value, state.emissionMultiplier, time.InternalHour / 24);
        var horizonTint = Color.Lerp(_sky.horizonTint.value, state.skyColor, time.InternalHour / 24);
        var groundColor = Color.Lerp(_sky.groundTint.value, state.groundColor, time.InternalHour / 24);
        _sky.spaceEmissionMultiplier.value = starAlpha;
        _sky.horizonTint.value = horizonTint;
        _sky.groundTint.value = groundColor;
    }

    private void Update()
    {
        var totalDaysUtc = SkyManager.instance.time.ComputeStarAngle();
        var rotateX = (totalDaysUtc * starRotation.x * 360.0) % 360.0;
        var rotateY = (totalDaysUtc * starRotation.y * 360.0) % 360.0;
        var rotateZ = (totalDaysUtc * starRotation.z * 360.0) % 360.0;

        SetRotation(new Vector3((float) rotateX, (float) rotateY, (float) rotateZ));
        
        var starAlpha = Mathf.Lerp(_sky.spaceEmissionMultiplier.value, targetEmissionMultiplier, Time.deltaTime);
        var horizonTint = Color.Lerp(_sky.horizonTint.value, targetHorizonTint, Time.deltaTime);
   //     var groundColor = Color.Lerp(_sky.groundTint.value,  targetGroundTint, Time.deltaTime);
        
        _sky.spaceEmissionMultiplier.value = starAlpha;
        _sky.horizonTint.value = horizonTint;
   //     _sky.groundTint.value = groundColor;
    }
}
