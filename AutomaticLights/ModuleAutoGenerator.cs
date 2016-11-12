using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomaticLights
{
    public class ModuleAutoGenerator : PartModule
    {
        [KSPField()]
        public double low;

        [KSPField]
        public double high;
        
        [KSPField]
        public string watch;

        [KSPField]
        public string power;

        public override void OnUpdate()
        {
            base.OnUpdate();

            var currentPower = GetRosource(power);
            var resourceToWatch = GetResource(watch);

            if(currentPower < low || resourceToWatch >= 99)
            {
                ToggleGenerator(false);
            }

            if(currentPower > high && resourceToWatch < 100)
            {
                ToggleGenerator(true);
            }
        }

        private void ToggleGenerator(bool onOff)
        {
            var parent = this.part;
            foreach(var m in parent.Modules)
            {
                ModuleGenerator mg = m as ModuleGenerator;
                if(mg != null)
                {
                    if(mg.generatorIsActive != onOff)
                    {
                        if(onOff)
                        {
                            mg.Activate();
                        }
                        else
                        {
                            mg.Shutdown();
                        }
                    }
                }
            }

            throw new NotImplementedException();
        }

        private double GetResource(string res)
        {
            var vessel = FlightGlobals.ActiveVessel;
            if(vessel.state != Vessel.State.ACTIVE)
            {
                return 0;
            }

            var activeResources = vessel.GetActiveResources();
            foreach(var r in activeResources)
            {
                if (r.info.name == res.ValueOrDefault("ElectricCharge"))
                {
                    return r.amount;
                }
            }

            return 0;
        }
    }
}
