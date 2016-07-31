namespace AutomaticLights
{
    using System.Linq;

    public class AutomaticLightPartModule : PartModule
    {
        private static string lastMessage;

        [KSPField()]
        public bool checkResource;

        public override string GetInfo()
        {
            if(string.IsNullOrEmpty(lastMessage))
            {
                return "Startup";
            }

            return lastMessage;
        }

        /// <summary>
        /// This value should be a percentage 0.0 -&gt; 1.0
        /// </summary>
        [KSPField()]
        public double minResourceLevel;

        [KSPField()]
        public string resourceName;

        private const string ElectricCharge = "ElectricCharge";
        private static int counter = 1;

        public override void OnFixedUpdate()
        {
            base.OnUpdate();
            if (counter++ % 100 != 0)
            {
                // only update every 100 frames.
                return;
            }

            counter = 1;

            if(FlightGlobals.ActiveVessel == null)
            {
                return;
            }

            var vessel = FlightGlobals.ActiveVessel;
            if (vessel.state != Vessel.State.ACTIVE)
            {
                lastMessage = "Vessel not active!";
                return;
            }

            if (vessel.situation != Vessel.Situations.ORBITING && vessel.situation != Vessel.Situations.LANDED)
            {
                // Only do updates when we are orbiting, or landed
                lastMessage = "Vessel not orbiting or landed.";
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
                    lastMessage = "Turn off auto";
                    TurnOnOffAuto();
                }
                else
                {
                    lastMessage = "Resources low";
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
                            if (ml.isOn != turnOn)
                            {
                                lastMessage = "turning lights " + (turnOn ? "on" : "off");
                                ml.SetLightState(turnOn);
                                ml.isOn = turnOn;
                                ml.UpdateLightColors();
                                //p.SendEvent(turnOn ? Constants.LightOnEvent : Constants.LightOffEvent);
                                break;
                            }

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