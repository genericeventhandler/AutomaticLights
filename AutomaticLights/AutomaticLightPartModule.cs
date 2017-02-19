namespace AutomaticLights
{
    using System.Linq;

    public class AutomaticLightPartModule : PartModule
    {
        private static string lastMessage;

        private static bool isActive;

        [KSPField()]
        public bool checkResource;

        public override string GetInfo()
        {
            if (string.IsNullOrEmpty(lastMessage))
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

        [KSPField()]
        public int framesToSkip;

        [KSPField()]
        public bool debugIsOn;

        [KSPEvent(guiActive = true, guiName = "Toggle mode", active = true)]
        public void ToggleMode()
        {
            isActive = !isActive;
            lightsOn = isActive;
            SendMessageToScreen("Automatic lights are " + (isActive ? " on" : "off"));
            isOn = true;
            TurnOnOffAuto();
            if (isOn)
            {
                isOn = false;
                TurnOnOffAuto();
            }
        }

        [KSPEvent(guiActive = true, guiName = "Debug Values", active = true)]
        public void DisplayDebugInfo()
        {
            string msg = string.Format("Debug: E{0} ", Utilities.GetResource(ElectricCharge));
            debugIsOn = true;
            Debug(msg);
        }

        private bool lightsOn;

        private const string ElectricCharge = "ElectricCharge";
        private static int counter = 1;

        public void Update()
        {
            DoAction();
        }

        public override void OnUpdate()
        {
            DoAction();
        }

        public void DoAction()
        {
            if (!isActive)
            {
                return;
            }

            counter = 1;

            if (FlightGlobals.ActiveVessel == null)
            {
                Debug("no active vessel");
                return;
            }

            var vessel = FlightGlobals.ActiveVessel;
            if (vessel.state != Vessel.State.ACTIVE)
            {
                lastMessage = "Vessel not active!";
                Debug();
                return;
            }

            //if (vessel.situation == Vessel.Situations.PRELAUNCH)
            //{
            //    // Don't turn on the lights if we are prelaunch or splashed.
            //    lastMessage = "Vessel is in prelaunch.";
            //    Debug();
            //    return;
            //}

            var rex = Utilities.GetResource("ElectricCharge");

            // Check that we have the required resource.
            if (rex > 0 && rex > minResourceLevel)
            {
                TurnOnOffAuto();
            }
            else
            {
                TurnOnOff(false);
                counter = framesToSkip;
            }
        }

        private void TurnOnOff(bool turnOn)
        {
            var p = this.part;
            foreach (var m in p.Modules)
            {
                var ml = m as ModuleLight;
                if (ml != null)
                {
                    //Debug("turning lights " + (turnOn ? "on" : "off"));

                    if (turnOn)
                    {
                        if (!ml.isOn)
                        {
                            ml.LightsOn();
                        }
                    }
                    else
                    {
                        if (ml.isOn)
                        {
                            ml.LightsOff();
                        }
                    }
                }
            }
        }

        private bool isOn = false;

        private void TurnOnOffAuto()
        {
            Vector3d vector;
            double dist;
            var vessel = FlightGlobals.ActiveVessel;
            if (vessel != null)
            {
                var sun = FlightGlobals.Bodies.FirstOrDefault(x => x.name == "Sun");
                if (sun != null)
                {
                    // if the sun is null, we've got bigger problems
                    if (Utilities.RaytraceBody(vessel, sun, out vector, out dist))
                    {
                        //Debug("raytrace returned true - turning off lights");
                        TurnOnOff(false);
                    }
                    else
                    {
                        //Debug("raytrace returned false - turning on lights");
                        TurnOnOff(true);
                    }
                }
                else
                {
                    Debug("Sun not found!");
                }
            }
            else
            {
                Debug("Vessel not found!");
            }
        }

        private void SendMessageToScreen(string message)
        {
            if (lastMessage != message)
            {
                ScreenMessages.PostScreenMessage(message);
                lastMessage = message;
            }
        }

        private void Debug()
        {
            Debug(lastMessage);
        }

        private void Debug(string message)
        {
            if (debugIsOn)
            {
                SendMessageToScreen(message);
            }
        }
    }
}