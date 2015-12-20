using System;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP;

using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KRnD
{
    // This class stores all types of upgrades a part can have.
    public class KRnDUpgrade
    {
        public int ispVac = 0;
        public int ispAtm = 0;
        public int dryMass = 0;
        public int fuelFlow = 0;
        public int torque = 0;
        public int chargeRate = 0;

        public const String ISP_VAC = "ispVac";
        public const String ISP_ATM = "ispAtm";
        public const String DRY_MASS = "dryMass";
        public const String FUEL_FLOW = "fuelFlow";
        public const String TORQUE = "torque";
        public const String CHARGE_RATE = "chargeRate";

        public override string ToString()
        {
            return "KRnDUpgrade(" +
                ISP_VAC+":" + this.ispVac.ToString() + "," +
                ISP_ATM+":" + this.ispAtm.ToString() + "," +
                DRY_MASS+":" + this.dryMass.ToString() + "," +
                FUEL_FLOW+":" + this.fuelFlow.ToString() + "," +
                TORQUE+":" + this.torque.ToString() + "," +
                CHARGE_RATE+":" + this.chargeRate.ToString() +
                ")";
        }

        public ConfigNode createConfigNode(string name)
        {
            ConfigNode node = new ConfigNode(name);
            if (this.ispVac > 0) node.AddValue(ISP_VAC, this.ispVac.ToString());
            if (this.ispAtm > 0) node.AddValue(ISP_ATM, this.ispAtm.ToString());
            if (this.dryMass > 0) node.AddValue(DRY_MASS, this.dryMass.ToString());
            if (this.fuelFlow > 0) node.AddValue(FUEL_FLOW, this.fuelFlow.ToString());
            if (this.torque > 0) node.AddValue(TORQUE, this.torque.ToString());
            if (this.chargeRate > 0) node.AddValue(CHARGE_RATE, this.chargeRate.ToString());
            return node;
        }

        public static KRnDUpgrade createFromConfigNode(ConfigNode node)
        {
            KRnDUpgrade upgrade = new KRnDUpgrade();
            if (node.HasValue(ISP_VAC)) upgrade.ispVac = Int32.Parse(node.GetValue(ISP_VAC));
            if (node.HasValue(ISP_ATM)) upgrade.ispAtm = Int32.Parse(node.GetValue(ISP_ATM));
            if (node.HasValue(DRY_MASS)) upgrade.dryMass = Int32.Parse(node.GetValue(DRY_MASS));
            if (node.HasValue(FUEL_FLOW)) upgrade.fuelFlow = Int32.Parse(node.GetValue(FUEL_FLOW));
            if (node.HasValue(TORQUE)) upgrade.torque = Int32.Parse(node.GetValue(TORQUE));
            if (node.HasValue(CHARGE_RATE)) upgrade.chargeRate = Int32.Parse(node.GetValue(CHARGE_RATE));
            return upgrade;
        }

        public KRnDUpgrade clone()
        {
            KRnDUpgrade copy = new KRnDUpgrade();
            copy.ispVac = this.ispVac;
            copy.ispAtm = this.ispAtm;
            copy.dryMass = this.dryMass;
            copy.fuelFlow = this.fuelFlow;
            copy.torque = this.torque;
            copy.chargeRate = this.chargeRate;
            return copy;
        }
    }

    // This class is used to store all relevant base-stats of a part used to calculate all other stats with
    // incrementel upgrades as well as a backup for resoting the original stats (eg after loading a savegame).
    public class PartStats
    {
        public float mass = 0;
        public float maxFuelFlow = 0;
        public FloatCurve atmosphereCurve = null;
        public float torque = 0;
        public float chargeRate = 0;

        public PartStats(Part part)
        {
            this.mass = part.mass;

            // There should only be one or the other, engines or RCS:
            ModuleEngines engineModule = KRnD.getEnginesModule(part);
            ModuleRCS rcsModule = KRnD.getRcsModule(part);
            if (engineModule)
            {
                this.maxFuelFlow = engineModule.maxFuelFlow;
                this.atmosphereCurve = new FloatCurve();
                for (int i = 0; i < engineModule.atmosphereCurve.Curve.length; i++)
                {
                    Keyframe frame = engineModule.atmosphereCurve.Curve[i];
                    this.atmosphereCurve.Add(frame.time, frame.value);
                }
            }
            else if (rcsModule)
            {
                this.maxFuelFlow = rcsModule.thrusterPower;
                this.atmosphereCurve = new FloatCurve();
                for (int i = 0; i < rcsModule.atmosphereCurve.Curve.length; i++)
                {
                    Keyframe frame = rcsModule.atmosphereCurve.Curve[i];
                    this.atmosphereCurve.Add(frame.time, frame.value);
                }
            }

            ModuleReactionWheel reactionWheel = KRnD.getReactionWheelModule(part);
            if (reactionWheel)
            {
                this.torque = reactionWheel.RollTorque; // There is also pitch- and yaw-torque, but they should all be the same
            }

            ModuleDeployableSolarPanel solarPanel = KRnD.getSolarPanelModule(part);
            if (solarPanel)
            {
                this.chargeRate = solarPanel.chargeRate;
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class KRnD : UnityEngine.MonoBehaviour
    {
        private static bool initialized = false;
        public static Dictionary<string, PartStats> originalStats = null;

        public static Dictionary<string, KRnDUpgrade> upgrades = new Dictionary<string, KRnDUpgrade>();

        public static KRnDModule getKRnDModule(Part part)
        {
            foreach (PartModule partModule in part.Modules)
            {
                if (partModule.moduleName == "KRnDModule") return (KRnDModule)partModule;
            }
            return null;
        }

        public static ModuleEngines getEnginesModule(Part part)
        {
            foreach (PartModule partModule in part.Modules)
            {
                if (partModule.moduleName == "ModuleEngines" || partModule.moduleName == "ModuleEnginesFX") return (ModuleEngines)partModule;
            }
            return null;
        }

        public static ModuleRCS getRcsModule(Part part)
        {
            foreach (PartModule partModule in part.Modules)
            {
                if (partModule.moduleName == "ModuleRCS") return (ModuleRCS)partModule;
            }
            return null;
        }

        public static ModuleReactionWheel getReactionWheelModule(Part part)
        {
            foreach (PartModule partModule in part.Modules)
            {
                if (partModule.moduleName == "ModuleReactionWheel") return (ModuleReactionWheel)partModule;
            }
            return null;
        }

        public static ModuleDeployableSolarPanel getSolarPanelModule(Part part)
        {
            foreach (PartModule partModule in part.Modules)
            {
                if (partModule.moduleName == "ModuleDeployableSolarPanel") return (ModuleDeployableSolarPanel)partModule;
            }
            return null;
        }

        public static float calculateImprovementFactor(float baseImprovement, float improvementScale, int upgrades)
        {
            float factor = 0;
            if (upgrades < 0) upgrades = 0;
            for (int i = 0; i < upgrades; i++)
            {
                if (i == 0) factor += baseImprovement;
                else factor += baseImprovement * (float)Math.Pow(improvementScale, i-1);
            }
            if (baseImprovement < 0 && factor < -0.9) factor = -0.9f;
            return (float) Math.Round(factor, 4);
        }

        public static int calculateScienceCost(int baseCost, float costScale, int upgrades)
        {
            float cost = 0;
            if (upgrades < 0) upgrades = 0;
            for (int i = 0; i < upgrades; i++)
            {
                if (i == 0) cost = baseCost;
                else cost += baseCost * (float) Math.Pow(costScale, i-1);
            }
            return (int) Math.Round(cost);
        }

        // Updates the global dictionary of available parts with the current set of upgrades (should be
        // executed for example when a new game starts or an existing game is loaded).
        public static int updateGlobalParts()
        {
            int upgradesApplied = 0;
            try
            {
                if (KRnD.upgrades == null) throw new Exception("upgrades-dictionary missing");
                foreach (AvailablePart part in PartLoader.Instance.parts)
                {
                    KRnDUpgrade upgrade;
                    if (!KRnD.upgrades.TryGetValue(part.name, out upgrade)) upgrade = new KRnDUpgrade(); // If there are no upgrades, reset the part.

                    // Udate the part to its latest model:
                    KRnD.updatePart(part.partPrefab, true);

                    // Rebuild the info-screen:
                    foreach (AvailablePart.ModuleInfo info in part.moduleInfos)
                    {
                        if (info.moduleName.ToLower() == "engine")
                        {
                            ModuleEngines engines = KRnD.getEnginesModule(part.partPrefab);
                            info.info = engines.GetInfo();
                            info.primaryInfo = engines.GetPrimaryField();
                        }
                        else if (info.moduleName.ToLower() == "rcs")
                        {
                            ModuleRCS rcs = KRnD.getRcsModule(part.partPrefab);
                            info.info = rcs.GetInfo();
                        }
                        else if (info.moduleName.ToLower() == "reaction wheel")
                        {
                            ModuleReactionWheel reactionWheel = KRnD.getReactionWheelModule(part.partPrefab);
                            info.info = reactionWheel.GetInfo();
                        }
                        else if (info.moduleName.ToLower() == "deployable solar panel")
                        {
                            ModuleDeployableSolarPanel solarPanel = KRnD.getSolarPanelModule(part.partPrefab);
                            info.info = solarPanel.GetInfo();
                        }
                    }
                    upgradesApplied++;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] updateGlobalParts(): " + e.ToString());
            }
            return upgradesApplied;
        }

        // Updates all parts in the vessel that is currently active in the editor.
        public static void updateEditorVessel(Part rootPart=null)
        {
            if (rootPart == null) rootPart = EditorLogic.RootPart;
            if (!rootPart) return;
            KRnD.updatePart(rootPart, true); // Update to the latest model
            foreach (Part childPart in rootPart.children)
            {
                KRnD.updateEditorVessel(childPart);
            }
        }

        // Updates the given part either to the latest model (updateToLatestModel=TRUE) or to the model defined by its
        // KRnDModule.
        public static void updatePart(Part part, bool updateToLatestModel)
        {
            KRnDUpgrade upgradesToApply;
            if (updateToLatestModel)
            {
                if (KRnD.upgrades.TryGetValue(part.name, out upgradesToApply))
                {
                    // Apply upgrades from global list:
                    KRnD.updatePart(part, upgradesToApply);
                }
                else
                {
                    // No Upgrades found, applay base-stats:
                    upgradesToApply = new KRnDUpgrade();
                    KRnD.updatePart(part, upgradesToApply);
                }
            }
            else
            {
                // Extract current upgrades of the part and set thoes stats:
                KRnDModule rndModule = KRnD.getKRnDModule(part);
                if (rndModule != null && (upgradesToApply = rndModule.getCurrentUpgrades()) != null)
                {
                    // Apply upgrades from the RnD-Module:
                    KRnD.updatePart(part, upgradesToApply);
                }
                else
                {
                    // No Upgrades found, applay base-stats:
                    upgradesToApply = new KRnDUpgrade();
                    KRnD.updatePart(part, upgradesToApply);
                }
            }
        }

        // Updates the given part with all upgrades provided in "upgradesToApply".
        public static void updatePart(Part part, KRnDUpgrade upgradesToApply)
        {
            try
            {
                // Find all relevant modules of this part:
                KRnDModule rndModule = KRnD.getKRnDModule(part);
                ModuleEngines engineModule = KRnD.getEnginesModule(part);
                ModuleRCS rcsModule = KRnD.getRcsModule(part);
                ModuleReactionWheel reactionWheel = KRnD.getReactionWheelModule(part);
                ModuleDeployableSolarPanel solarPanel = KRnD.getSolarPanelModule(part);
                if (rndModule == null) return;
                if (KRnD.upgrades == null) throw new Exception("upgrades-dictionary missing");
                if (KRnD.originalStats == null) throw new Exception("original-stats-dictionary missing");

                // Get the part-name (sometimes the name of the root-part of a vessel is extended by the vessel-name like "Mk1Pod (X-Bird)"):
                String partName = part.name;
                partName = Regex.Replace(partName, @" \(.*\)$", "");

                // Get the original part-stats:
                PartStats originalStats;
                if (!KRnD.originalStats.TryGetValue(partName, out originalStats)) throw new Exception("no origional-stats for part '" + partName + "'");

                KRnDUpgrade latestModel;
                if (!KRnD.upgrades.TryGetValue(partName, out latestModel)) latestModel = null;


                // Dry Mass:
                rndModule.dryMass_upgrades = upgradesToApply.dryMass;
                part.mass = originalStats.mass * ( 1 + KRnD.calculateImprovementFactor(rndModule.dryMass_improvement, rndModule.dryMass_improvementScale, upgradesToApply.dryMass) );
                
                // Fuel Flow:
                if (engineModule || rcsModule)
                {
                    rndModule.fuelFlow_upgrades = upgradesToApply.fuelFlow;
                    float maxFuelFlow  = originalStats.maxFuelFlow * ( 1 + KRnD.calculateImprovementFactor(rndModule.fuelFlow_improvement, rndModule.fuelFlow_improvementScale, upgradesToApply.fuelFlow) );
                    if (engineModule) engineModule.maxFuelFlow = maxFuelFlow;
                    else if (rcsModule) rcsModule.thrusterPower = maxFuelFlow;
                }
                else
                {
                    rndModule.fuelFlow_upgrades = 0;
                }

                // ISP Vac & Atm:
                if (engineModule || rcsModule)
                {
                    rndModule.ispVac_upgrades = upgradesToApply.ispVac;
                    rndModule.ispAtm_upgrades = upgradesToApply.ispAtm;
                    float improvementFactorVac = 1 + KRnD.calculateImprovementFactor(rndModule.ispVac_improvement, rndModule.ispVac_improvementScale, upgradesToApply.ispVac);
                    float improvementFactorAtm = 1 + KRnD.calculateImprovementFactor(rndModule.ispAtm_improvement, rndModule.ispAtm_improvementScale, upgradesToApply.ispAtm);

                    FloatCurve fc = new FloatCurve();
                    for (int v = 0; v < originalStats.atmosphereCurve.Curve.length; v++)
                    {
                        Keyframe frame = originalStats.atmosphereCurve.Curve[v];

                        float pressure = frame.time;
                        float factorAtThisPressure = 1;
                        if (pressure == 0) factorAtThisPressure = improvementFactorVac; // In complete vacuum
                        else if (pressure >= 1) factorAtThisPressure = improvementFactorAtm; // At lowest kerbal atmosphere
                        else
                        {
                            factorAtThisPressure = (1 - pressure) * improvementFactorVac + pressure * improvementFactorAtm; // Mix both
                        }
                        float newValue = frame.value * factorAtThisPressure;
                        fc.Add(pressure, newValue);
                    }
                    if (engineModule) engineModule.atmosphereCurve = fc;
                    else if (rcsModule) rcsModule.atmosphereCurve = fc;
                }
                else
                {
                    rndModule.ispVac_upgrades = 0;
                    rndModule.ispAtm_upgrades = 0;
                }

                // Torque:
                if (reactionWheel)
                {
                    rndModule.torque_upgrades = upgradesToApply.torque;
                    float torque = originalStats.torque * (1 + KRnD.calculateImprovementFactor(rndModule.torque_improvement, rndModule.torque_improvementScale, upgradesToApply.torque));
                    reactionWheel.PitchTorque = torque;
                    reactionWheel.YawTorque = torque;
                    reactionWheel.RollTorque = torque;
                }
                else
                {
                    rndModule.torque_upgrades = 0;
                }

                // Charge Rate:
                if (solarPanel)
                {
                    rndModule.chargeRate_upgrades = upgradesToApply.chargeRate;
                    float chargeRate = originalStats.chargeRate * (1 + KRnD.calculateImprovementFactor(rndModule.chargeRate_improvement, rndModule.chargeRate_improvementScale, upgradesToApply.chargeRate));
                    solarPanel.chargeRate = chargeRate;
                }
                else
                {
                    rndModule.chargeRate_upgrades = 0;
                }

            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] updatePart(): " + e.ToString());
            }
        }

        // Updates all parts of the given vessel according to their RnD-Moudle settings (should be executed
        // when the vessel is loaded to make sure, that the vessel uses its own, historic upgrades and not
        // the global part-upgrades).
        public static void updateVessel(Vessel vessel)
        {
            try
            {
                if (!vessel.isActiveVessel) return; // Only the currently active vessel matters, the others are not simulated anyway.
                if (KRnD.upgrades == null) throw new Exception("upgrades-dictionary missing");
                Debug.Log("[KRnD] updating vessel '" + vessel.vesselName.ToString() + "'");

                // Iterate through all parts:
                foreach (Part part in vessel.parts)
                {
                    // We only have to update parts which have the RnD-Module:
                    bool hasRnd = false;
                    foreach (PartModule partModule in part.Modules)
                    {
                        if (partModule.moduleName == "KRnDModule")
                        {
                            hasRnd = true;
                            break;
                        }
                    }
                    if (!hasRnd) continue;

                    if (vessel.situation == Vessel.Situations.PRELAUNCH)
                    {
                        // Update the part with the latest model while on the launchpad:
                        KRnD.updatePart(part, true);
                    }
                    else
                    {
                        // Update this part with its own stats:
                        KRnD.updatePart(part, false);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] updateVesselActive(): " + e.ToString());
            }
        }

        // Is called every time the active vessel changes (on entering a scene, switching the vessel or on docking).
        private void OnVesselChange(Vessel vessel)
        {
            try
            {
                KRnD.updateVessel(vessel);
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] OnVesselChange(): " + e.ToString());
            }
        }

        // Is called when we interact with a part in the editor.
        private void EditorPartEvent(ConstructionEventType ev, Part part)
        {
            try
            {
                if (ev != ConstructionEventType.PartCreated && ev != ConstructionEventType.PartDetached && ev != ConstructionEventType.PartAttached) return;
                KRnDGUI.selectedPart = part;
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] EditorPartEvent(): " + e.ToString());
            }
        }

        // Is called when this Addon is first loaded to initializes all values (eg registration of event-handlers and creation
        // of original-stats library).
        public void Awake()
        {
            try
            {
                // Create a backup of all unmodivied parts before we update them. We will later use these backup-parts
                // for all calculations of upgraded stats.
                if (KRnD.originalStats == null)
                {
                    KRnD.originalStats = new Dictionary<string, PartStats>();
                    foreach (AvailablePart aPart in PartLoader.Instance.parts)
                    {
                        Part part = aPart.partPrefab;

                        // Backup this part, if it has the RnD-Module:
                        if (KRnD.getKRnDModule(part) != null)
                        {
                            PartStats duplicate;
                            if (originalStats.TryGetValue(part.name, out duplicate))
                            {
                                Debug.LogError("[KRnD] Awake(): duplicate part-name: " + part.name.ToString());
                            }
                            else
                            {
                                originalStats.Add(part.name, new PartStats(part));
                            }
                        }
                    }
                }

                // Execute the following code only once:
                if (KRnD.initialized) return;
                DontDestroyOnLoad(this);

                // Register event-handlers:
                GameEvents.onVesselChange.Add(this.OnVesselChange);
                GameEvents.onEditorPartEvent.Add(this.EditorPartEvent);

                KRnD.initialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] Awake(): " + e.ToString());
            }
        }
    }

    // This class handels load- and save-operations.
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    class KRnDScenarioModule : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            try
            {
                double time = DateTime.Now.Ticks;
                ConfigNode upgradeNodes = new ConfigNode("upgrades");
                foreach (string upgradeName in KRnD.upgrades.Keys)
                {
                    KRnDUpgrade upgrade;
                    if (!KRnD.upgrades.TryGetValue(upgradeName, out upgrade)) continue;
                    upgradeNodes.AddNode(upgrade.createConfigNode(upgradeName));
                    Debug.Log("[KRnD] saved: " + upgradeName + " " + upgrade.ToString());
                }
                node.AddNode(upgradeNodes);

                time = (DateTime.Now.Ticks - time) / TimeSpan.TicksPerSecond;
                Debug.Log("[KRnD] saved " + upgradeNodes.CountNodes.ToString() + " upgrades in " + time.ToString("0.000s"));

                ConfigNode guiSettings = new ConfigNode("gui");
                guiSettings.AddValue("left", KRnDGUI.windowPosition.xMin);
                guiSettings.AddValue("top", KRnDGUI.windowPosition.yMin);
                node.AddNode(guiSettings);
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] OnSave(): " + e.ToString());
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            try
            {
                double time = DateTime.Now.Ticks;
                int upgradesApplied = 0;

                KRnD.upgrades.Clear();

                ConfigNode upgradeNodes = node.GetNode("upgrades");
                if (upgradeNodes != null)
                {
                    foreach (ConfigNode upgradeNode in upgradeNodes.GetNodes())
                    {
                        KRnDUpgrade upgrade = KRnDUpgrade.createFromConfigNode(upgradeNode);
                        KRnD.upgrades.Add(upgradeNode.name, upgrade);
                        Debug.Log("[KRnD] loaded: " + upgradeNode.name + " " + upgrade.ToString());
                    }

                    // Update global part-list with new upgrades from the savegame:
                    upgradesApplied = KRnD.updateGlobalParts();

                    // If we started with an active vessel, update that vessel:
                    Vessel vessel = FlightGlobals.ActiveVessel;
                    if (vessel)
                    {
                        KRnD.updateVessel(vessel);
                    }

                    time = (DateTime.Now.Ticks - time) / TimeSpan.TicksPerSecond;
                    Debug.Log("[KRnD] retrieved and applied " + upgradesApplied.ToString() + " upgrades in " + time.ToString("0.000s"));
                }

                ConfigNode guiSettings = node.GetNode("gui");
                if (guiSettings != null)
                {
                    if (guiSettings.HasValue("left")) KRnDGUI.windowPosition.xMin = (float) Double.Parse(guiSettings.GetValue("left"));
                    if (guiSettings.HasValue("top")) KRnDGUI.windowPosition.yMin = (float) Double.Parse(guiSettings.GetValue("top"));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] OnLoad(): " + e.ToString());
            }
        }
    }

}
