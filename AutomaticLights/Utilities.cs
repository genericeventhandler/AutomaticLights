using System;

namespace AutomaticLights
{
    public static class Utilities
    {
        /// <summary> return true if the ray 'dir' starting at 'p' doesn't hit 'body' </summary>
        /// <param name="p">ray Origin</param> <param name="dir">ray direction</param> <param
        /// name="body">obstacle</param> <returns></returns> <remarks> Author : ShotgunNinja - http://forum.kerbalspaceprogram.com/index.php?/topic/144484-auto-lights-at-night/&do=findComment&comment=2691985</remarks>
        public static bool Raytrace(Vector3d p, Vector3d dir, CelestialBody body)
        {
            // ray from origin to body center
            var diff = body.position - p;

            // projection of origin->body center ray over the raytracing direction
            double k = Vector3d.Dot(diff, dir);

            // the ray doesn't hit body if its minimal analytical distance along the ray is less than
            // its radius
            return k < 0.0 || (dir * k - diff).magnitude > body.Radius;
        }

        /// <summary> return true if the body is visible from the vessel </summary> <param
        /// name="vessel">vessel: origin</param> <param name="body">body: target</param>
        /// <returns>return: true if visible, false otherwise</returns> <remarks> Author :
        /// ShotgunNinja - http://forum.kerbalspaceprogram.com/index.php?/topic/144484-auto-lights-at-night/&do=findComment&comment=2691985</remarks>
        [Obsolete("Warning, only works for the sun, use the other method instead", false)]
        public static bool RaytraceBody(Vessel vessel, CelestialBody body)
        {
            // shortcuts
            CelestialBody mainbody = vessel.mainBody;
            CelestialBody refbody = mainbody.referenceBody;

            // generate ray parameters
            var vessel_pos = vessel.GetWorldPos3D();
            var dir = (body.position - vessel_pos).normalized;

            // raytrace
            return (body == mainbody || Raytrace(vessel_pos, dir, mainbody))
                && (body == refbody || refbody == null || Raytrace(vessel_pos, dir, refbody));
        }

        /// <summary>
        /// Return true if ray 'dir' starting at p and the lenght of distance doesn't hit body.
        /// </summary>
        /// <param name="p">ray origin</param>
        /// <param name="dir">ray direction</param>
        /// <param name="dist">ray length</param>
        /// <param name="body">obstacle</param>
        /// <returns>true if visible from craft</returns>
        /// <remarks> Author : ShotgunNinja - http://forum.kerbalspaceprogram.com/index.php?/topic/144484-auto-lights-at-night/&do=findComment&comment=2691985</remarks>
        public static bool Raytrace(Vector3d p, Vector3d dir, double dist, CelestialBody body)
        {
            // ray from origin to body center
            Vector3d diff = body.position - p;

            // projection of origin->body center ray over the raytracing direction
            double k = Vector3d.Dot(diff, dir);

            // the ray doesn't hit body if its minimal analytical distance along the ray is less than its radius
            // simplified from 'p + dir * k - body.position'
            return k < 0.0 || k > dist || (dir * k - diff).magnitude > body.Radius;
        }

        /// <summary>
        /// return true if the body is visible from the vessel
        /// </summary>
        /// <param name="vessel">origin</param>
        /// <param name="body">target</param>
        /// <param name="dir">will contain normalized vector from vessel to body</param>
        /// <param name="dist">will contain distance from vessel to body surface</param>
        /// <returns>true if visible</returns>
        /// <remarks> Author : ShotgunNinja - http://forum.kerbalspaceprogram.com/index.php?/topic/144484-auto-lights-at-night/&do=findComment&comment=2691985</remarks>
        public static bool RaytraceBody(Vessel vessel, CelestialBody body, out Vector3d dir, out double dist)
        {
            // shortcuts
            CelestialBody mainbody = vessel.mainBody;
            CelestialBody refbody = mainbody.referenceBody;

            // generate ray parameters
            Vector3d vessel_pos = vessel.GetWorldPos3D();
            dir = body.position - vessel_pos;
            dist = dir.magnitude;
            dir /= dist;
            dist -= body.Radius;

            // raytrace
            return (body == mainbody || Raytrace(vessel_pos, dir, dist, mainbody))
                && (body == refbody || refbody == null || Raytrace(vessel_pos, dir, dist, refbody));
        }

        public static string ValueOrDefault(this string value, string defaultValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return value;
        }

        public static double GetResource(string res)
        {
            var vessel = FlightGlobals.ActiveVessel;
            if (vessel == null || vessel.state != Vessel.State.ACTIVE)
            {
                //Debug("Vessel state not active");
                return 1;
            }

            var activeResources = vessel.GetActiveResources();
            foreach (var r in activeResources)
            {
                if (r.info.name.ToLower() == res.ToLower())
                {
                    //Debug("{0} v = {1} max = 2", r.amount, r.maxAmount);
                    if (r.maxAmount > 0)
                    {
                        return r.amount / r.maxAmount;
                    }
                }
            }


            //Debug("Didn't find the resource {0} ", res);
            return 1;
        }
    }
}