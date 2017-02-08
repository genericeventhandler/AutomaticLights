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

            if (checkResource)
            {
                var rex = Utilities.GetResource("ElectricCharge");

                // Check that we have the required resource.
                if (rex > 0 && rex > minResourceLevel)
                {
                    lastMessage = "Turn off auto";
                    Debug();
                    TurnOnOffAuto();
                }
                else
                {
                    if (lightsOn)
                    {
                        counter = counter - 1;
                        if (counter < 0)
                        {
                            SendMessageToScreen("Resources low");
                            TurnOnOff(false);
                            counter = framesToSkip;
                        }
                    }
                    else
                    {
                        Debug("lightsOn = false and resource is low");
                    }
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
                                Debug();
                                ml.SetLightState(turnOn);
                                ml.isOn = turnOn;
                                ml.UpdateLightColors();
                                lightsOn = turnOn;

                                //p.SendEvent(turnOn ? Constants.LightOnEvent : Constants.LightOffEvent);
                                break;
                            }

                            break; // this part shouldn't have more than one module light.
                        }
                    }
                }
            }

            Debug("exit turn on / off");
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
                        Debug("raytrace returned true - turning off lights");
                        TurnOnOff(false);
                    }
                    else
                    {
                        Debug("raytrace returned false - turning on lights");
                        TurnOnOff(true);
                    }
                }
            }
        }

        private void SendMessageToScreen(string message)
        {
            ScreenMessages.PostScreenMessage(message);
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