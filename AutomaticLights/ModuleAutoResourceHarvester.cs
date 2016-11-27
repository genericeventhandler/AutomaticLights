namespace AutomaticLights
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ModuleAutoResourceHarvester : PartModule
    {
        [KSPField()]
        public double low;

        [KSPField]
        public double high;

        [KSPField]
        public string watch;

        private static bool isDebug;
        private static bool isActive;

        [KSPEvent(guiActive = true, guiName = "Toggle debug", active = true)]
        public void ToggleMode()
        {
            isDebug = !isDebug;
            SendMessageToScreen("Debug mode is " + (isDebug ? " on" : "off"));
        }

        [KSPEvent(guiActive = true, guiName = "Toggle On/Off", active = true)]
        public void ToggleOnOff()
        {
            isActive = !isActive;
            SendMessageToScreen("Auto mining is " + (isActive ? " on" : "off"));
        }

        public static int counter = 1;

        public override string GetInfo()
        {
            return "Automatic Mining Adaptor";
        }

        public override void OnFixedUpdate()
        {
            DoAction();
        }

        public override void OnUpdate()
        {
            DoAction();
        }

        public void DoAction()
        {
            if (isActive)
            {
                if (FlightGlobals.ActiveVessel == null)
                {
                    return;
                }

                var vessel = FlightGlobals.ActiveVessel;
                if (vessel == null || vessel.state != Vessel.State.ACTIVE)
                {
                    return;
                }

                var currentPower = Utilities.GetResource("ElectricCharge");
                var resourceToWatch = Utilities.GetResource(watch);

                //Debug("EC = {0},  {1} = {2}", currentPower, watch, resourceToWatch);

                if (currentPower < low || resourceToWatch >= 0.99)
                {
                    Debug("Turn off miner, {0} < {1} || {2} = {3} >= 0.99", Math.Round(currentPower,1), low, watch, Math.Round(resourceToWatch,1));
                    ToggleGenerator(false);
                }

                if (currentPower > high && resourceToWatch < 0.99)
                {
                    Debug("Turn on miner EC {0} > {1} && {2} = {3} < 0.99", Math.Round(currentPower,1), high, watch, Math.Round(resourceToWatch,1));
                    ToggleGenerator(true);
                }
            }
        }

        private void SendMessageToScreen(string message)
        {
            ScreenMessages.PostScreenMessage(message);
        }

        private void Debug(string message, params object[] args)
        {
            Debug(string.Format(message, args));
        }

        private static string lastMessage;

        private void Debug(string message)
        {
            if (isDebug)
            {
                if (message != lastMessage)
                {
                    SendMessageToScreen(message);
                    lastMessage = message;
                }
            }
        }

        private void ToggleGenerator(bool onOff)
        {
            var parent = this.part;
            if (parent != null)
            {
                //Debug("found a part, looking for modules");
                foreach (var m in parent.Modules)
                {
                    ModuleResourceHarvester mg = m as ModuleResourceHarvester;
                    if (mg != null)
                    {
                        if (onOff)
                        {
                            //Debug("Turning on generator");
                            mg.StartResourceConverter();
                        }
                        else
                        {
                            //Debug("Turning off generator");
                            mg.StopResourceConverter();
                        }
                    }
                }
            }
        }
    }
}

