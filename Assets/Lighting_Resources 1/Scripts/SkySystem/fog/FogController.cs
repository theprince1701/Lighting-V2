using System;
using System.Collections;
using System.Collections.Generic;
using SkySystem.time;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class FogProperties
{
    public TimeStates time;
    public Color fogColor;
    public float fogDensity;
    public float fogMaxHeight;
    public float fogBaseHeight;
}

public class FogController : SkyEffectBase
{
    private enum FogTypes
    {
        DensityVolume,
        VolumeFog
    }

    [SerializeField] private FogTypes fogType = FogTypes.VolumeFog;
    [SerializeField] private FogProperties[] fogProperties = new FogProperties[4];
    
    private Volume _volume;
    private Fog _fog;

    private Color _targetColor;
    private float _targetMaxHeight;
    private float _targetBaseHeight;
    private float _targetDensity;
    
    private Volume FindVolume()
    {
        var volume = FindObjectOfType<Volume>() as Volume;

        return volume != null ? volume : null;
    }
    
    private void Start()
    {
        if (_volume == null)
            _volume = FindVolume();

        if (_fog == null && _volume != null)
            _volume.profile.TryGet(out _fog);
    }
    
    public void Validate(TimeStates time)
    {      
   //     UpdateEffect(time);
        _volume = FindVolume();                                                                       
                                                                                                          
        _volume.profile.TryGet(out _fog);                                                             
        _fog.tint.value = Color.Lerp(_fog.color.value, _targetColor, TimeController.instance.InternalHour / 24);
        _fog.maximumHeight.value = Mathf.Lerp(_fog.maximumHeight.value, _targetMaxHeight, TimeController.instance.InternalHour / 24);
        _fog.baseHeight.value = Mathf.Lerp(_fog.baseHeight.value, _targetBaseHeight, TimeController.instance.InternalHour / 24);     
    }

    private void Update()
    {
        if (_fog == null)
            return;
        
        _fog.tint.value = Color.Lerp(_fog.color.value, _targetColor, Time.deltaTime);
        _fog.maximumHeight.value = Mathf.Lerp(_fog.maximumHeight.value, _targetMaxHeight, Time.deltaTime);
        _fog.baseHeight.value = Mathf.Lerp(_fog.baseHeight.value, _targetBaseHeight, Time.deltaTime);
    }

    protected override void UpdateEffect(SkyStates time)
    {
        foreach (var t in fogProperties)
        {
           // if (time != t.time) continue;
            _targetColor = t.fogColor;
            _targetMaxHeight = t.fogMaxHeight;
            _targetBaseHeight = t.fogBaseHeight;
            _targetDensity = t.fogDensity;

            break;
        }
    }
}
