﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kethane
{
    public class KethaneDetector : PartModule
    {
        [KSPField(isPersistant = false)]
        public float DetectingPeriod;

        [KSPField(isPersistant = false)]
        public float DetectingHeight;

        private static AudioSource PingEmpty;
        private static AudioSource PingDeposit;

        public override void OnStart(PartModule.StartState state)
        {
            #region Sound effects
            PingEmpty = gameObject.AddComponent<AudioSource>();
            WWW wwwE = new WWW("file://" + KSPUtil.ApplicationRootPath.Replace("\\", "/") + "PluginData/mmi_kethane/sounds/echo_empty.wav");
            if ((PingEmpty != null) && (wwwE != null))
            {
                PingEmpty.clip = wwwE.GetAudioClip(false);
                PingEmpty.volume = 1;
                PingEmpty.Stop();
            }

            PingDeposit = gameObject.AddComponent<AudioSource>();
            WWW wwwD = new WWW("file://" + KSPUtil.ApplicationRootPath.Replace("\\", "/") + "PluginData/mmi_kethane/sounds/echo_deposit.wav");
            if ((PingDeposit != null) && (wwwD != null))
            {
                PingDeposit.clip = wwwD.GetAudioClip(false);
                PingDeposit.volume = 1;
                PingDeposit.Stop();
            }
            #endregion
        }

        public override void OnUpdate()
        {
            CelestialBody body = this.vessel.mainBody;
            if (body == null)
                return;

            // Rotation code just for test

            Transform BaseT = this.part.transform.FindChild("model").FindChild("Kethane Sensor");

            Vector3 bodyCoords = BaseT.InverseTransformPoint(body.transform.position);

            Vector2 pos = Misc.CartesianToPolar(bodyCoords);

            double alpha = Misc.NormalizeAngle(pos.x);
            double beta = Misc.NormalizeAngle(pos.y);

            Transform RotH = BaseT.FindChild("Horizontal Rotation");
            Transform RotV = RotH.FindChild("Vertical Rotation");

            double LocH = RotH.localRotation.eulerAngles.y;
            double LocV = Misc.NormalizeAngle(RotV.localRotation.eulerAngles.x - 90);

            if (Math.Abs(beta - LocH) > 0.1f)
                RotH.RotateAroundLocal(new Vector3(0, 1, 0), (beta > LocH ? 0.25f : -0.25f) * Time.deltaTime);

            if (Math.Abs(alpha - LocV) > 0.1f)
                RotV.RotateAroundLocal(new Vector3(1, 0, 0), (alpha > LocV ? 0.25f : -0.25f) * Time.deltaTime);
        }

        public override void OnFixedUpdate()
        {
            var controller = KethaneController.GetInstance(this.vessel);
            if (controller.IsDetecting && this.vessel != null && this.vessel.gameObject.active)
            {
                controller.TimerEcho += Time.deltaTime * (1 + Math.Log(TimeWarp.CurrentRate));

                double Altitude = Misc.GetTrueAltitude(vessel);
                controller.TimerThreshold = this.DetectingPeriod + Altitude * 0.000005d; // 0,5s delay at 100km
                var DepositUnder = controller.GetDepositUnder();

                if (controller.TimerEcho >= controller.TimerThreshold)
                {
                    if (DepositUnder != null && Altitude <= this.DetectingHeight && DepositUnder.Kethane >= 1.0f)
                    {
                        controller.DrawMap(true);
                        controller.LastLat = vessel.latitude;
                        controller.LastLon = vessel.longitude;
                        if (vessel == FlightGlobals.ActiveVessel && controller.ScanningSound)
                            PingDeposit.Play();
                    }
                    else
                    {
                        controller.DrawMap(false);
                        if (vessel == FlightGlobals.ActiveVessel && controller.ScanningSound)
                            PingEmpty.Play();
                    }
                    controller.TimerEcho = 0;
                }
            }
        }
    }
}
