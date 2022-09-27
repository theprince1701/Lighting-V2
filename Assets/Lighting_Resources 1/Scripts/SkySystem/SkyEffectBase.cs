using UnityEngine;

public class SkyEffectBase : MonoBehaviour
{
    public void OnEnable() => SkyManager.updateProperties += UpdateEffect;

    public void OnDisable() => SkyManager.updateProperties -= UpdateEffect;

protected virtual void UpdateEffect(SkyStates time)
    {
    }

    public void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        SkyManager.instance = FindObjectOfType<SkyManager>();
        
        SkyManager.instance.Validate();
    }
}
