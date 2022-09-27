using System.Collections.Generic;
using SkySystem.lights;
using SkySystem.time;
using UnityEngine;

public class SkyManager : MonoBehaviour
{
    public TimeController time;
    public LightingController lighting;
    public FogController fog;
    public SkyController sky;
    
    
    public List<SkyStates> states = new List<SkyStates>();
    public SkyStates currentState { get; private set; }

    public delegate void UpdateSkyPropertiesDelegate(SkyStates state);
    public static UpdateSkyPropertiesDelegate updateProperties;

    public static SkyManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    
    private void OnValidate()
    {
        AssignState();
        
        time.Validate(currentState);
        sky.Validate(currentState, time);
        lighting.Validate(currentState, time);
    }

    public void Validate()
    {
        OnValidate();
    }

    public void AssignState()
    {
        var timeState = time.TimeState;
        
        for (int i = 0; i < states.Count; i++)
        {
            if (timeState != states[i].time) continue;
            
            currentState = states[i];
            break;
        }
    }
}
