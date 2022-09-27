using SkySystem.time;
using UnityEngine;


[System.Serializable]
public class SkyStates
{
    public string name;
    public bool sky;
    public bool fog;
    public bool light;
    
    public TimeStates time;
    //fog settings
    [ConditionalHide("fog", true)] public Color fogColor;
    [ConditionalHide("fog", true)] public float fogHeight;
    [ConditionalHide("fog", true)] public float fogMaxHeight;

    //light settings
    [ConditionalHide("Light", true)] public bool moon;
    [ConditionalHide("Light", true)] public bool sun;
    [ConditionalHide("light", true)] public float lightIntensity;
    [ConditionalHide("light", true)] public float lightVolumetricMultiplier;
    [ConditionalHide("light", true)] public float lightVolumetricShadowMultiplier;
    
    //sky
    [ConditionalHide("sky", true)] public Color skyColor;
    [ConditionalHide("sky", true)] public Color groundColor;
    [ConditionalHide("sky", true)] public Cubemap emissionMap;
    [ConditionalHide("sky", true)] public float emissionMultiplier;
}