namespace AutomaticLights
{
    using System;

    public class AutoGeneratorModule : PartModule
    {
        [KSPField()]
        public double low;

        [KSPField]
        public double high;

        [KSPField]
        public string watch;

        [KSPField]
        public double maxPercentFill;

        [KSPField]
        public string power;

        private static bool isDebug;

        [KSPEvent(guiActive = true, guiName = "Toggle debug", active = true)]
        public void ToggleMode()
        {
            isDebug = !isDebug;
            SendMessageToScreen("Debug mode is " + (isDebug ? " on" : "off"));
        }

        [KSPEvent(guiActive = true, guiName = "Debug Values", active = true)]
        public void DisplayDebugInfo()
        {
            string msg = string.Format("Debug: R:{0} L:{1} H:{2} E:{3} LQ:{4}", watch, low, high, Utilities.GetResource(power), Utilities.GetResource(watch));
            isDebug = true;
            Debug(msg);
        }

        public static int counter = 1;

        public override string GetInfo()
        {
            return "GEH auto fuel cell, and RTG";
        }

        public void Update()
        {
            try
            {
                DoAction();
            }
            catch { }
        }

        public override void OnUpdate()
        {
            try
            {
                DoAction();
            }
            catch { }
        }

        private DateTime elapsed;

        public void DoAction()
        {
            if (maxPercentFill <= 0 || maxPercentFill > 1)
            {
                maxPercentFill = 0.25;
            }

            if (FlightGlobals.ActiveVessel == null)
            {
                return;
            }

            if (elapsed.CompareTo(DateTime.Now) < 0)
            {
                elapsed = DateTime.Now.AddSeconds(3);
            }
            else
            {
                return;
            }

            var vessel = FlightGlobals.ActiveVessel;
            if (vessel == null || vessel.state != Vessel.State.ACTIVE)
            {
                return;
            }

            var currentPower = Utilities.GetResource(power);
            var resourceToWatch = Utilities.GetResource(watch);

            //Debug("EC = {0},  {1} = {2}", currentPower, watch, resourceToWatch);

            if (currentPower < low || resourceToWatch >= maxPercentFill)
            {
                Debug("Turn off generator, {0} < {1} || {2} = {3} >= {4}", Math.Round(currentPower, 1), low, watch, Math.Round(resourceToWatch, 1), maxPercentFill);
                ToggleGenerator(false);
                return;
            }

            if (currentPower >= high && resourceToWatch < 0.99)
            {
                Debug("Turn on generator EC {0} > {1} && {2} = {3} < 0.99", Math.Round(currentPower, 1), high, watch, Math.Round(resourceToWatch, 1));
                ToggleGenerator(true);
                return;
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
                    ModuleGenerator mg = m as ModuleGenerator;
                    if (mg != null)
                    {
                        //Debug("Found a module");
                        if (mg.isAlwaysActive)
                        {
                            //Debug("Always active, quiting");
                        }
                        else
                        {
                            if (onOff)
                            {
                                //Debug("Turning on generator");
                                mg.Activate();
                            }
                            else
                            {
                                if (mg.isActiveAndEnabled)
                                {
                                    //Debug("Turning off generator");
                                    mg.Shutdown();
                                }
                            }
                        }
                    }
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
    }
}