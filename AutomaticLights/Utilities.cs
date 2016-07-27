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

        public static string ValueOrDefault(this string value, string defaultValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return value;
        }
    }
}