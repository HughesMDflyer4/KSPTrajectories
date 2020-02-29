/*
  Copyright© (c) 2016-2017 Youen Toupin, (aka neuoy).
  Copyright© (c) 2017-2018 A.Korsunsky, (aka fat-lobyte).
  Copyright© (c) 2017-2018 S.Gray, (aka PiezPiedPy).

  This file has been modified to add support for kRPC.

  This file is part of Trajectories.
  Trajectories is available under the terms of GPL-3.0-or-later.
  See the LICENSE.md file for more details.

  Trajectories is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  Trajectories is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

  You should have received a copy of the GNU General Public License
  along with Trajectories.  If not, see <http://www.gnu.org/licenses/>.
*/

using KRPC.Service.Attributes;
using KRPC.Utils;
using System.Linq;
using UnityEngine;

namespace Trajectories
{
    /// <summary>
    /// API for Trajectories. Note: this API only returns correct values for the "active vessel".
    /// </summary>
    [KRPCService(GameScene = KRPC.Service.GameScene.Flight, Name = "Trajectories")]
    public static class API
    {
        /// <summary>
        /// Returns the version number of trajectories in a string formated as Major.Minor.Patch i.e. 2.1.0
        /// </summary>
        [KRPCProperty]
        public static string GetVersion
        {
            get
            {
                string version_txt = typeof(API).Assembly.GetName().Version.ToString();
                version_txt = version_txt.Remove(version_txt.LastIndexOf("."));
                return version_txt;
            }
        }

        /// <summary>
        /// Returns the major version number of trajectories
        /// </summary>
        [KRPCProperty]
        public static int GetVersionMajor
        {
            get
            {
                string[] version = GetVersion.Split('.');
                return System.Convert.ToInt32(version[0]);
            }
        }


        /// <summary>
        /// Returns the minor version number of trajectories
        /// </summary>
        [KRPCProperty]
        public static int GetVersionMinor
        {
            get
            {
                string[] version = GetVersion.Split('.');
                return System.Convert.ToInt32(version[1]);
            }
        }


        /// <summary>
        /// Returns the patch version number of trajectories
        /// </summary>
        [KRPCProperty]
        public static int GetVersionPatch
        {
            get
            {
                string[] version = GetVersion.Split('.');
                return System.Convert.ToInt32(version[2]);
            }
        }

        /// <summary>
        /// Modifies the AlwaysUpdate value in the settings page.
        /// </summary>
        [KRPCProperty]
        public static bool AlwaysUpdate
        {
            get
            {
                return Settings.fetch.AlwaysUpdate;
            }
            set
            {
                Settings.fetch.AlwaysUpdate = value;
            }
        }

        /// <summary>
        /// Returns trajectory patch EndTime or Null if no active vessel or calculated trajectory.
        /// See GetTimeTillImpact for remaining time until impact.
        /// </summary>
        [KRPCProcedure]
        public static double GetEndTime()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (Trajectory.Patch patch in Trajectory.fetch.Patches)
                {
                    if (patch.ImpactPosition.HasValue)
                        return patch.EndTime;
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns the remaining time until Impact in seconds or Null if no active vessel or calculated trajectory.
        /// </summary>
        [KRPCProcedure]
        public static double GetTimeTillImpact()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (Trajectory.Patch patch in Trajectory.fetch.Patches)
                {
                    if (patch.ImpactPosition.HasValue)
                        return patch.EndTime - Planetarium.GetUniversalTime();
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns the calculated impact position of the trajectory or Null if no active vessel or calculated trajectory.
        /// </summary>
        [KRPCProcedure]
        public static Tuple<float, float, float> GetImpactPosition()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (Trajectory.Patch patch in Trajectory.fetch.Patches)
                {
                    if (patch.ImpactPosition != null)
                        return new Tuple<float, float, float>(patch.ImpactPosition.Value.x, patch.ImpactPosition.Value.y, patch.ImpactPosition.Value.z);
                }
            }
            return new Tuple<float, float, float>(0, 0, 0);
        }

        /// <summary>
        /// Returns the calculated impact position of the trajectory in lat and long.
        /// </summary>
        [KRPCProcedure]
        public static Tuple<double, double> GetImpactLatLong()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (Trajectory.Patch patch in Trajectory.fetch.Patches)
                {
                    if (patch.ImpactPosition != null)
                    {
                        var latlong = patch.SpaceOrbit.referenceBody.GetLatitudeAndLongitude(patch.ImpactPosition.Value);
                        return new Tuple<double, double>(latlong.x, latlong.y);
                    }
                }
            }
            return new Tuple<double, double>(0, 0);
        }

        /// <summary>
        /// Returns the calculated impact velocity of the trajectory or Null if no active vessel or calculated trajectory.
        /// </summary>
        [KRPCProcedure]
        public static Tuple<float, float, float> GetImpactVelocity()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (Trajectory.Patch patch in Trajectory.fetch.Patches)
                {
                    if (patch.ImpactVelocity != null)
                        return new Tuple<float, float, float>(patch.ImpactVelocity.Value.x, patch.ImpactVelocity.Value.y, patch.ImpactVelocity.Value.z);
                }
            }
            return new Tuple<float, float, float>(0, 0, 0);
        }

        /// <summary>
        /// Returns the planned direction or Null if no active vessel or set target.
        /// </summary>
        [KRPCProcedure]
        public static Tuple<float, float, float> PlannedDirection()
        {
            if (FlightGlobals.ActiveVessel != null && Trajectory.Target.Body != null)
                return new Tuple<float, float, float>(NavBallOverlay.GetPlannedDirection().x, NavBallOverlay.GetPlannedDirection().y, NavBallOverlay.GetPlannedDirection().z);
            return new Tuple<float, float, float>(0, 0, 0);
        }

        /// <summary>
        /// Returns the corrected direction or Null if no active vessel or set target.
        /// </summary>
        [KRPCProcedure]
        public static Tuple<float, float, float> CorrectedDirection()
        {
            if (FlightGlobals.ActiveVessel != null && Trajectory.Target.Body != null)
                return new Tuple<float, float, float>(NavBallOverlay.GetCorrectedDirection().x, NavBallOverlay.GetCorrectedDirection().y, NavBallOverlay.GetCorrectedDirection().z);
            return new Tuple<float, float, float>(0, 0, 0);
        }

        /// <summary>
        /// Returns true if a target has been set, false if not, or Null if no active vessel.
        /// </summary>
        [KRPCProcedure]
        public static bool HasTarget()
        {
            if (FlightGlobals.ActiveVessel != null && Trajectory.Target.Body != null)
                return true;
            return false;
        }

        /// <summary>
        /// Set the trajectories target to a latitude, longitude and altitude at the HomeWorld.
        /// </summary>
        [KRPCProcedure]
        public static void SetTarget(double lat, double lon, double alt = 2.0)
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                //Hard coded to Kerbin currently, should be changed
                CelestialBody body = FlightGlobals.Bodies.SingleOrDefault(x => x.isHomeWorld);
                if (body != null)
                {
                    Vector3d worldPos = body.GetWorldSurfacePosition(lat, lon, alt);
                    Trajectory.Target.SetFromWorldPos(body, worldPos - body.position);
                }
            }
        }

        /// <summary>
        /// Set the trajectories descent profile to Prograde.
        /// </summary>
        [KRPCProperty]
        public static bool ProgradeEntry
        {
            get
            {
                if (FlightGlobals.ActiveVessel != null)
                    return DescentProfile.fetch.ProgradeEntry;
                return false;
            }
            set
            {
                if ((FlightGlobals.ActiveVessel != null) && !DescentProfile.fetch.ProgradeEntry)
                {
                    DescentProfile.fetch.ProgradeEntry = true;
                    DescentProfile.fetch.Reset(0d);
                    DescentProfile.fetch.Save();
                }
            }
        }

        /// <summary>
        /// Set the trajectories descent profile to Prograde.
        /// </summary>
        [KRPCProperty]
        public static bool RetrogradeEntry
        {
            get
            {
                if (FlightGlobals.ActiveVessel != null)
                    return DescentProfile.fetch.RetrogradeEntry;
                return false;
            }
            set
            {
                if ((FlightGlobals.ActiveVessel != null) && !DescentProfile.fetch.RetrogradeEntry)
                {
                    DescentProfile.fetch.RetrogradeEntry = true;
                    DescentProfile.fetch.Reset();
                    DescentProfile.fetch.Save();
                }
            }
        }

        /// <summary>
        /// Triggers a recalculation of the trajectory.
        /// </summary>
        [KRPCProcedure]
        private static void UpdateTrajectory()
        {
            if (FlightGlobals.ActiveVessel != null)
                Trajectory.fetch.ComputeTrajectory(FlightGlobals.ActiveVessel, DescentProfile.fetch);
        }
    }
}
