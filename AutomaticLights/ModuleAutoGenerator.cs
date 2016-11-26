using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomaticLights
{
    public class AutoGeneratorModule : PartModule
    {
        [KSPField()]
        public double low;

        [KSPField]
        public double high;
        
        [KSPField]
        public string watch;

        [KSPField]
        public string power;

        private static bool isDebug;

        [KSPEvent(guiActive = true, guiName = "Toggle debug", active = true)]
        public void ToggleMode()
        {
            isDebug = !isDebug;
            SendMessageToScreen("Debug mode is " + (isDebug ? " on" : "off"));
        }


        public static int counter = 1;

        public override string GetInfo()
        {
            return "GEH auto fuel cell, and RTG";
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
            if (FlightGlobals.ActiveVessel == null)
            {
                return;
            }

            var vessel = FlightGlobals.ActiveVessel;
            if (vessel == null || vessel.state != Vessel.State.ACTIVE)
            {
                return;
            }

            var currentPower = GetResource(power);
            var resourceToWatch = GetResource(watch);

            //Debug("EC = {0},  {1} = {2}", currentPower, watch, resourceToWatch);

            if(currentPower < low || resourceToWatch >= 0.99)
            {
                Debug("Turn off generator, {0} < {1} || {2} = {3} >= 0.99", currentPower, low, watch, resourceToWatch);
                ToggleGenerator(false);
            }

            if(currentPower > high && resourceToWatch < 0.99)
            {
                Debug("Turn on generator EC {0} > {1} && {2} = {3} < 0.99", currentPower, high, watch, resourceToWatch);
                ToggleGenerator(true);
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
                                //Debug("Turning off generator");
                                mg.Shutdown();
                            }
                        }
                    }
                }
            }
        }

        private double GetResource(string res)
        {
            var vessel = FlightGlobals.ActiveVessel;
            if(vessel == null || vessel.state != Vessel.State.ACTIVE)
            {
                //Debug("Vessel state not active");
                return 1;
            }

            var activeResources = vessel.GetActiveResources();
            foreach(var r in activeResources)
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
                if(message != lastMessage)
                {
                    SendMessageToScreen(message);
                    lastMessage = message;
                }
            }
        }
    }
}
