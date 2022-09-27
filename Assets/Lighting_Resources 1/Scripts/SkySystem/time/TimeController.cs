using System;
using SkySystem.lights;
using UnityEngine;

namespace SkySystem.time
{
    public enum TimeStates
    {
        None,
        Sunrise,
        Day,
        Sunset,
        Night
    }

    public class TimeController : SkyEffectBase
    {
        public enum TimeProgressTypes
        {
            None,
            RealWorld,
            Simulation
        }
    
        public static TimeController instance;
        
        public TimeProgressTypes timeProgress = TimeProgressTypes.None;
        public TimeInfo timeInfo;
        
        [Range(-90f, 90f)]
        public float latitude = 69.6546f;
        [Range(-180f, 180f)] 
        public float longitude = 18.9637f;
        
        [Range(0,360)]
        public float skyOffset;
        [Range(0f, 20f)]
        public float timeSpeed = 22f;
        
        public LightController sunController, moonController;
        
        public TimeStates TimeState { get; private set; }
        public float InternalHour { get; private set; }

        private const double Rad2Deg = 180.0 / Math.PI;

        private float _lst;

        private void Start()
        {
            if(instance == null)
                instance = this;
        }
        private void Update()
        {
            ProgressLightPositions();
            
            switch (timeProgress)
            {
                case TimeProgressTypes.RealWorld:
                    SetTime(DateTime.Now);
                    break;
                
                case TimeProgressTypes.Simulation:
                {
                    int nextSecond = (int) (24f / timeSpeed * 60f * Time.deltaTime);

                    DateTime nextTime = timeInfo.TimeInfoToDateTime() + new TimeSpan(0, 0, nextSecond);
                    
                    SetTime(nextTime);
                    break;
                }
            }
        }
        
        private void SetTime(DateTime time)
        {
            timeInfo.ConvertFromDateTime(time);
        }  
        
        private void SetTimeState(TimeStates time, bool updateAtomsphere)
        {
            if (time == TimeState)
                return;

            TimeState = time;
            
            if (!updateAtomsphere || !Application.isPlaying || SkyManager.instance == null) return;
            
            SkyManager.instance.AssignState();
            SkyManager.updateProperties.Invoke(SkyManager.instance.currentState);
        }

        
        private void ProgressLightPositions()
        {
            InternalHour = timeInfo.hour + (timeInfo.minute * 0.0166667f) + (timeInfo.second * 0.000277778f);

            float declination = 367 * timeInfo.year - 7 * (timeInfo.year + (timeInfo.month / 12 + 9) / 12) / 4 + 275 * timeInfo.month / 9 + timeInfo.day - 730530;
            declination += InternalHour / 24f;
            float ecl = 23.4393f - 3.563E-7f * declination;
            
            double sunAzimuthAngle, sunZenithAngle;
            
            CalculateSunPositionEnv(InternalHour, declination, ecl, out sunAzimuthAngle, out sunZenithAngle);
            sunController.SetRotation(new Vector3((float)sunZenithAngle, (float)sunAzimuthAngle + skyOffset, 0));
        
            double moonAzimuthAngle, moonZenithAngle;
            
            CalculateMoonPositionEnv(declination, ecl, out moonAzimuthAngle, out moonZenithAngle);
            moonController.SetRotation(new Vector3((float)moonZenithAngle, (float)moonAzimuthAngle + skyOffset, 0));
            
            UpdateState(sunZenithAngle);
        }

        private void UpdateState(double solarAngle)
        {
            switch (TimeState)
            {
                case TimeStates.Night:
                    moonController.GetLight().shadows = LightShadows.None;
                    sunController.GetLight().shadows = LightShadows.Hard;
                    break;
                case TimeStates.Day:
                    moonController.GetLight().shadows = LightShadows.Hard;
                    sunController.GetLight().shadows = LightShadows.None;
                    break;
                case TimeStates.None:
                    break;
                case TimeStates.Sunrise:
                    break;
                case TimeStates.Sunset:
                    break;
            }

            switch (solarAngle)
            {
                case < 5 and > -5 when TimeState == TimeStates.Night:
                    if(TimeState != TimeStates.Sunrise) SetTimeState(TimeStates.Sunrise, true);
                    break;
                case < 5 and > -5:
                {
                    if (TimeState == TimeStates.Day && TimeState != TimeStates.Sunset) SetTimeState(TimeStates.Sunset, true);
                    break;
                }
                case > 10:
                    if(TimeState != TimeStates.Day) SetTimeState(TimeStates.Day, true);
                    break;
                case < -10f:
                   if(TimeState != TimeStates.Night) SetTimeState(TimeStates.Night, true);
                    break;
            }
        }
        
        private void CalculateMoonPositionEnv(float d, float ecl, out double outAzimuth, out double outAltitude)
        {
            float N = 125.1228f - 0.0529538083f * d;
            float i = 5.1454f;
            float w = 318.0634f + 0.1643573223f * d;
            float a = 60.2666f;
            float e = 0.054900f;
            float M = 115.3654f + 13.0649929509f * d;

            float rad_M = Mathf.Deg2Rad * M;

            float E = rad_M + e * Mathf.Sin(rad_M) * (1f + e * Mathf.Cos(rad_M));

            float xv = a * (Mathf.Cos(E) - e);
            float yv = a * (Mathf.Sqrt(1f - e * e) * Mathf.Sin(E));

            float v = Mathf.Rad2Deg * Mathf.Atan2(yv, xv);
            float r = Mathf.Sqrt(xv * xv + yv * yv);

            float rad_N = Mathf.Deg2Rad * N;
            float sin_N = Mathf.Sin(rad_N);
            float cos_N = Mathf.Cos(rad_N);

            float l = Mathf.Deg2Rad * (v + w);
            float sin_l = Mathf.Sin(l);
            float cos_l = Mathf.Cos(l);

            float rad_i = Mathf.Deg2Rad * i;
            float cos_i = Mathf.Cos(rad_i);

            float xh = r * (cos_N * cos_l - sin_N * sin_l * cos_i);
            float yh = r * (sin_N * cos_l + cos_N * sin_l * cos_i);
            float zh = r * (sin_l * Mathf.Sin(rad_i));

            float cos_ecl = Mathf.Cos(Mathf.Deg2Rad * ecl);
            float sin_ecl = Mathf.Sin(Mathf.Deg2Rad * ecl);

            float xe = xh;
            float ye = yh * cos_ecl - zh * sin_ecl;
            float ze = yh * sin_ecl + zh * cos_ecl;

            float ra = Mathf.Atan2(ye, xe);
            float decl = Mathf.Atan2(ze, Mathf.Sqrt(xe * xe + ye * ye));

            float HA = Mathf.Deg2Rad * _lst - ra;

            float x = Mathf.Cos(HA) * Mathf.Cos(decl);
            float y = Mathf.Sin(HA) * Mathf.Cos(decl);
            float z = Mathf.Sin(decl);

            float latitude = Mathf.Deg2Rad * this.latitude;
            float sin_latitude = Mathf.Sin(latitude);
            float cos_latitude = Mathf.Cos(latitude);

            float xhor = x * sin_latitude - z * cos_latitude;
            float yhor = y;
            float zhor = x * cos_latitude + z * sin_latitude;

            float azimuth = Mathf.Atan2(yhor, xhor) + Mathf.Deg2Rad * 180f;
            float altitude = Mathf.Atan2(zhor, Mathf.Sqrt(xhor * xhor + yhor * yhor));

            outAzimuth = azimuth * Rad2Deg;
            outAltitude = altitude * Rad2Deg;
        }

        private void CalculateSunPositionEnv(float internalHour, float d, float ecl, out double outAzimuth, out double outAltitude)
        {
            float w = 282.9404f + 4.70935E-5f * d;
            float e = 0.016709f - 1.151E-9f * d;
            float M = 356.0470f + 0.9856002585f * d;

            float E = M + e * Mathf.Rad2Deg * Mathf.Sin(Mathf.Deg2Rad * M) * (1 + e * Mathf.Cos(Mathf.Deg2Rad * M));

            float xv = Mathf.Cos(Mathf.Deg2Rad * E) - e;
            float yv = Mathf.Sin(Mathf.Deg2Rad * E) * Mathf.Sqrt(1 - e * e);

            float v = Mathf.Rad2Deg * Mathf.Atan2(yv, xv);
            float r = Mathf.Sqrt(xv * xv + yv * yv);

            float l = v + w;

            float xs = r * Mathf.Cos(Mathf.Deg2Rad * l);
            float ys = r * Mathf.Sin(Mathf.Deg2Rad * l);

            float xe = xs;
            float ye = ys * Mathf.Cos(Mathf.Deg2Rad * ecl);
            float ze = ys * Mathf.Sin(Mathf.Deg2Rad * ecl);

            float decl_rad = Mathf.Atan2(ze, Mathf.Sqrt(xe * xe + ye * ye));
            float decl_sin = Mathf.Sin(decl_rad);
            float decl_cos = Mathf.Cos(decl_rad);

            float GMST0 = (l + 180);
            float GMST = GMST0 + internalHour * 15;
            _lst = GMST + longitude;

            float HA_deg = _lst - Mathf.Rad2Deg * Mathf.Atan2(ye, xe);
            float HA_rad = Mathf.Deg2Rad * HA_deg;
            float HA_sin = Mathf.Sin(HA_rad);
            float HA_cos = Mathf.Cos(HA_rad);

            float x = HA_cos * decl_cos;
            float y = HA_sin * decl_cos;
            float z = decl_sin;

            float sin_Lat = Mathf.Sin(Mathf.Deg2Rad * latitude);
            float cos_Lat = Mathf.Cos(Mathf.Deg2Rad * latitude);

            float xhor = x * sin_Lat - z * cos_Lat;
            float yhor = y;
            float zhor = x * cos_Lat + z * sin_Lat;

            float azimuth = Mathf.Atan2(yhor, xhor) + Mathf.Deg2Rad * 180;
            float altitude = Mathf.Atan2(zhor, Mathf.Sqrt(xhor * xhor + yhor * yhor));

            outAltitude = altitude * Rad2Deg;
            outAzimuth = azimuth * Rad2Deg;
        }
        
        public double ComputeStarAngle()
        {
            var span = timeInfo.TimeInfoToDateTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            return span.TotalDays;
        }
        
        public void Validate(SkyStates state)
        {
            if (timeInfo.day >= DateTime.DaysInMonth(timeInfo.year, timeInfo.month))
                timeInfo.day = 1;

            if (timeInfo.minute > 59)
            {
                timeInfo.hour++;
                timeInfo.minute = 1;
            }
            else if (timeInfo.minute < 1)
            {
                timeInfo.minute = 59;

                timeInfo.hour--;
            }

            if (timeInfo.hour > 23)
            {
                timeInfo.hour = 0;
            }
            else if (timeInfo.hour < 0)
            {
                timeInfo.hour = 23;
            }
            
            ProgressLightPositions();
        }
    }
    
}