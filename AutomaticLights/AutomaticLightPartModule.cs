namespace AutomaticLights
{
    using System.Linq;

    public class AutomaticLightPartModule : PartModule
    {
        [KSPField()]
        public bool checkResource;

        /// <summary>
        /// This value should be a percentage 0.0 -&gt; 1.0
        /// </summary>
        [KSPField()]
        public double minResourceLevel;

        [KSPField()]
        public string resourceName;

        private const string ElectricCharge = "ElectricCharge";
        private static int counter = 0;

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (counter++ % 100 != 0)
            {
                // only update every 100 frames.
                return;
            }

            counter = 1;

            var vessel = FlightGlobals.ActiveVessel;
            if (vessel.state != Vessel.State.ACTIVE)
            {
                return;
            }

            if (vessel.situation != Vessel.Situations.PRELAUNCH)
            {
                // don't drain power on the pad.
                return;
            }

            if (checkResource)
            {
                var activeResources = vessel.GetActiveResources();
                Vessel.ActiveResource rex = null;
                foreach (var r in activeResources)
                {
                    if (r.info.name == resourceName.ValueOrDefault(ElectricCharge))
                    {
                        rex = r;
                        break;
                    }
                }

                if (rex == null)
                {
                    UnityEngine.Debug.Log("Missing resource " + resourceName.ValueOrDefault(ElectricCharge));
                    return;
                }

                // Check that we have the required resource.
                if (rex.amount > 0 && rex.amount / rex.maxAmount > minResourceLevel)
                {
                    TurnOnOffAuto();
                }
                else
                {
                    TurnOnOff(false);
                }
            }
        }

        private void TurnOnOff(bool turnOn)
        {
            var vessel = FlightGlobals.ActiveVessel;
            if (vessel != null)
            {
                foreach (var p in vessel.GetActiveParts())
                {
                    foreach (var m in p.Modules)
                    {
                        var ml = m as ModuleLight;
                        if (ml != null)
                        {
                            ml.SetLightState(turnOn);
                            break; // this part shouldn't have more than one module light.
                        }
                    }
                }
            }
        }

        private void TurnOnOffAuto()
        {
            var vessel = FlightGlobals.ActiveVessel;
            if (vessel != null)
            {
                var sun = FlightGlobals.Bodies.FirstOrDefault();
                if (sun != null)
                {
                    // if the sun is null, we've got bigger problems
                    if (Utilities.RaytraceBody(vessel, sun))
                    {
                        TurnOnOff(false);
                    }
                    else
                    {
                        TurnOnOff(true);
                    }
                }
            }
        }
    }
}