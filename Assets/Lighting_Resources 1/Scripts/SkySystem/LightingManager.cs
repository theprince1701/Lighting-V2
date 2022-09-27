using System;
using System.Collections.Generic;
using SkySystem.time;
using UnityEngine;

public class LightingManager : MonoBehaviour
{
    public delegate void _BroadcastTimeEvent(TimeStates time);

    public static _BroadcastTimeEvent BroadcastTimeEvent;
}



