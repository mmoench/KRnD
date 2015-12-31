using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

using System.Text.RegularExpressions;

namespace KRnD
{
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class KRnDGUI : MonoBehaviour
    {
        private ApplicationLauncherButton button = null;
        public static Rect windowPosition = new Rect(300, 60, 450, 400);
        private GUIStyle windowStyle = new GUIStyle(HighLogic.Skin.window) { fixedWidth = 500f, fixedHeight = 300f };
        private GUIStyle labelStyle = new GUIStyle(HighLogic.Skin.label);
        private GUIStyle labelStyleSmall = new GUIStyle(HighLogic.Skin.label) { fontSize = 12 };
        private GUIStyle buttonStyle = new GUIStyle(HighLogic.Skin.button);
        private GUIStyle scrollStyle = new GUIStyle(HighLogic.Skin.scrollView);
        private Vector2 scrollPos = Vector2.zero;
        private static Texture2D texture = null;

        // The part that was last selected in the editor:
        public static Part selectedPart = null;

        private int selectedUpgradeOption = 0;

        void Awake()
        {
            if (texture == null)
            {
                texture = new Texture2D(36, 36, TextureFormat.RGBA32, false);
                var textureFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "R&D_icon.truecolor");
                texture.LoadImage(File.ReadAllBytes(textureFile));
            }

            // Add event-handlers to create and destroy our button:
            GameEvents.onGUIApplicationLauncherReady.Remove(ReadyEvent);
            GameEvents.onGUIApplicationLauncherReady.Add(ReadyEvent);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(DestroyEvent);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(DestroyEvent);
        }

        // Fires when a scene is ready so we can install our button.
        public void ReadyEvent()
        {
            if (ApplicationLauncher.Ready && button == null)
            {
                var visibleScense = ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB;
                button = ApplicationLauncher.Instance.AddModApplication(GuiOn, GuiOff, null, null, null, null, visibleScense, texture);
            }
        }

        // Fires when a scene is unloaded and we should destroy our button:
        public void DestroyEvent()
        {
            if (button == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(button);
            RenderingManager.RemoveFromPostDrawQueue(144, Ondraw);
            button = null;
            selectedPart = null;
        }

        private void GuiOn()
        {
            // Draw window:
            RenderingManager.AddToPostDrawQueue(100, Ondraw);
        }

        private void GuiOff()
        {
            // Hide window:
            RenderingManager.RemoveFromPostDrawQueue(100, Ondraw);
        }

        private void Ondraw()
        {
            windowPosition = GUILayout.Window(100, windowPosition, OnWindow, "", windowStyle);
        }

        private void OnWindow(int windowId)
        {
            GenerateWindow();
        }

        public static int UpgradeIspVac(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.ispVac++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeIspVac(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeIspAtm(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.ispAtm++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeIspAtm(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeDryMass(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.dryMass++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeDryMass(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeFuelFlow(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.fuelFlow++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeFuelFlow(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeTorque(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.torque++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeTorque(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeChargeRate(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.chargeRate++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeChargeRate(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeCrashTolerance(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.crashTolerance++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeCrashTolerance(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeBatteryCharge(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.batteryCharge++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeBatteryCharge(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeGeneratorEfficiency(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.generatorEfficiency++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeGeneratorEfficiency(): " + e.ToString());
            }
            return 0;
        }

        public static int UpgradeConverterEfficiency(Part part)
        {
            try
            {
                KRnDUpgrade store = null;
                if (!KRnD.upgrades.TryGetValue(part.name, out store))
                {
                    store = new KRnDUpgrade();
                    KRnD.upgrades.Add(part.name, store);
                }
                store.converterEfficiency++;
                KRnD.updateGlobalParts();
                KRnD.updateEditorVessel();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] UpgradeConverterEfficiency(): " + e.ToString());
            }
            return 0;
        }

        // Returns the info-text of the given part with the given upgrades to be displayed in the GUI-comparison.
        private String getPartInfo(Part part, KRnDUpgrade upgradesToApply=null)
        {
            String info = "";
            KRnDUpgrade originalUpgrades = null;
            try
            {
                KRnDModule rndModule = KRnD.getKRnDModule(part);
                if (rndModule == null || (originalUpgrades = rndModule.getCurrentUpgrades()) == null) return info;

                // Upgrade the part to get the correct info, we revert it back to its previous values in the finally block below:
                KRnD.updatePart(part, upgradesToApply);
                ModuleEngines enginesModule = KRnD.getEnginesModule(part);
                ModuleRCS rcsModule = KRnD.getRcsModule(part);
                ModuleReactionWheel reactionWheelModule = KRnD.getReactionWheelModule(part);
                ModuleDeployableSolarPanel solarPanelModule = KRnD.getSolarPanelModule(part);
                ModuleLandingLeg landingLegModule = KRnD.getLandingLegModule(part);
                PartResource electricChargeResource = KRnD.getChargeResource(part);
                ModuleGenerator generatorModule = KRnD.getGeneratorModule(part);
                List<ModuleResourceConverter> converterModules = KRnD.getConverterModules(part);

                // Basic stats:
                info = "<color=#FFFFFF><b>Dry Mass:</b> "+ part.mass.ToString("0.#### t") +"\n";
                if (landingLegModule) info += "<b>Crash Tolerance:</b> " + part.crashTolerance.ToString("0.#### m/s") + "\n";
                if (electricChargeResource) info += "<b>Electric Charge:</b> " + electricChargeResource.maxAmount.ToString() + "\n";

                // Module stats:
                info += "\n";
                if (enginesModule) info += "<color=#99FF00><b>Engine:</b></color>\n" + enginesModule.GetInfo();
                if (rcsModule) info += "<color=#99FF00><b>RCS:</b></color>\n" + rcsModule.GetInfo();
                if (reactionWheelModule) info += "<color=#99FF00><b>Reaction Wheel:</b></color>\n" + reactionWheelModule.GetInfo();
                if (solarPanelModule) info += "<color=#99FF00><b>Solar Panel:</b></color>\n" + solarPanelModule.GetInfo();
                if (generatorModule) info += "<color=#99FF00><b>Generator:</b></color>\n" + generatorModule.GetInfo();
                if (converterModules != null)
                {
                    foreach (ModuleResourceConverter converterModule in converterModules)
                    {
                        info += "<color=#99FF00><b>Converter " + converterModule.ConverterName + ":</b></color>\n" + converterModule.GetInfo() + "\n";
                    }
                }
                info += "</color>";
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnDGUI] getPartInfo(): " + e.ToString());
            }
            finally
            {
                try
                {
                    if (originalUpgrades != null)
                    {
                        KRnD.updatePart(part, originalUpgrades);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("[KRnDGUI] getPartInfo() restore of part failed: " + e.ToString());
                }
            }
            return info;
        }

        // Highlights differences between the two given texts, assuming they contain the same number of words.
        private String highlightChanges(String originalText, String newText, String color="00FF00")
        {
            String highlightedText = "";
            try
            {
                // Split as whitespaces and tags, we only need normal words and numbers:
                String[] set1 = Regex.Split(originalText, @"([\s<>])");
                String[] set2 = Regex.Split(newText, @"([\s<>])");
                for (int i=0; i<set2.Length; i++)
                {
                    String oldWord = "";
                    if (i < set1.Length) oldWord = set1[i];
                    String newWord = set2[i];

                    if (oldWord != newWord) newWord = "<color=#" + color + "><b>" + newWord + "</b></color>";
                    highlightedText += newWord;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnDGUI] highlightChanges(): " + e.ToString());
            }
            if (highlightedText == "") return newText;
            return highlightedText;
        }

        private void GenerateWindow()
        {
            try
            {
                GUILayout.BeginVertical();

                // Get all modules of the selected part:
                String partTitle = "";
                Part part = null;
                KRnDModule rndModule = null;
                ModuleEngines enginesModule = null;
                ModuleRCS rcsModule = null;
                ModuleReactionWheel reactionWheelModule = null;
                ModuleDeployableSolarPanel solarPanelModule = null;
                ModuleLandingLeg landingLegModule = null;
                PartResource electricChargeResource = null;
                ModuleGenerator generatorModule = null;
                List<ModuleResourceConverter> converterModules = null;
                if (selectedPart != null)
                {
                    foreach (AvailablePart aPart in PartLoader.Instance.parts)
                    {
                        if (aPart.partPrefab.name == selectedPart.name)
                        {
                            part = aPart.partPrefab;
                            partTitle = aPart.title;
                            break;
                        }
                    }
                    if (part)
                    {
                        rndModule = KRnD.getKRnDModule(part);
                        enginesModule = KRnD.getEnginesModule(part);
                        rcsModule = KRnD.getRcsModule(part);
                        reactionWheelModule = KRnD.getReactionWheelModule(part);
                        solarPanelModule = KRnD.getSolarPanelModule(part);
                        landingLegModule = KRnD.getLandingLegModule(part);
                        electricChargeResource = KRnD.getChargeResource(part);
                        generatorModule = KRnD.getGeneratorModule(part);
                        converterModules = KRnD.getConverterModules(part);
                    }
                }
                if (!part || !rndModule)
                {
                    // No part selected:
                    GUILayout.BeginArea(new Rect(10, 5, windowStyle.fixedWidth, 20));
                    GUILayout.Label("<b>Kerbal R&D: Select a part to improve</b>", this.labelStyle);
                    GUILayout.EndArea();
                    GUI.DragWindow();
                    return;
                }

                // Get stats of the current version of the selected part:
                KRnDUpgrade currentUpgrade;
                if (!KRnD.upgrades.TryGetValue(part.name, out currentUpgrade)) currentUpgrade = new KRnDUpgrade();
                String currentInfo = getPartInfo(part, currentUpgrade);

                // Create a copy of the part-stats which we can use to mock an upgrade further below:
                KRnDUpgrade nextUpgrade = currentUpgrade.clone();

                // Title:
                GUILayout.BeginArea(new Rect(10, 5, windowStyle.fixedWidth, 20));
                String version = rndModule.getVersion();
                if (version != "") version = " - " + version;
                GUILayout.Label("<b>" + partTitle + version + "</b>", this.labelStyle);
                GUILayout.EndArea();

                // List with upgrade-options:
                float optionsWidth = 100;
                float optionsHeight = this.windowStyle.fixedHeight - 30 - 30 - 20;
                GUILayout.BeginArea(new Rect(10, 30 + 20, optionsWidth, optionsHeight));
                
                List<String> options = new List<String>();
                options.Add("Dry Mass");
                if (enginesModule || rcsModule)
                {
                    options.Add("ISP Vac");
                    options.Add("ISP Atm");
                    options.Add("Fuel Flow");
                }
                if (reactionWheelModule)
                {
                    options.Add("Torque");
                }
                if (solarPanelModule)
                {
                    options.Add("Charge Rate");
                }
                if (landingLegModule)
                {
                    options.Add("Crash Tolerance");
                }
                if (electricChargeResource)
                {
                    options.Add("Battery");
                }
                if (generatorModule)
                {
                    options.Add("Generator");
                }
                if (converterModules != null)
                {
                    options.Add("Converter");
                }
                if (this.selectedUpgradeOption >= options.Count) this.selectedUpgradeOption = 0;
                this.selectedUpgradeOption = GUILayout.SelectionGrid(this.selectedUpgradeOption, options.ToArray(), 1, this.buttonStyle);
                GUILayout.EndArea();
                
                String selectedUpgradeOption = options.ToArray()[this.selectedUpgradeOption];
                int currentUpgradeCount = 0;
                int nextUpgradeCount = 0;
                int scienceCost = 0;
                float currentImprovement = 0;
                float nextImprovement = 0;
                Func<Part,int> upgradeFunction = null;
                if (selectedUpgradeOption == "ISP Vac")
                {
                    upgradeFunction = KRnDGUI.UpgradeIspVac;
                    currentUpgradeCount = currentUpgrade.ispVac;
                    nextUpgradeCount = ++nextUpgrade.ispVac;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.ispVac_improvement, rndModule.ispVac_improvementScale, currentUpgrade.ispVac);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.ispVac_improvement, rndModule.ispVac_improvementScale, nextUpgrade.ispVac);
                    scienceCost = KRnD.calculateScienceCost(rndModule.ispVac_scienceCost, rndModule.ispVac_costScale, nextUpgrade.ispVac);
                }
                else if (selectedUpgradeOption == "ISP Atm")
                {
                    upgradeFunction = KRnDGUI.UpgradeIspAtm;
                    currentUpgradeCount = currentUpgrade.ispAtm;
                    nextUpgradeCount = ++nextUpgrade.ispAtm;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.ispAtm_improvement, rndModule.ispAtm_improvementScale, currentUpgrade.ispAtm);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.ispAtm_improvement, rndModule.ispAtm_improvementScale, nextUpgrade.ispAtm);
                    scienceCost = KRnD.calculateScienceCost(rndModule.ispAtm_scienceCost, rndModule.ispAtm_costScale, nextUpgrade.ispAtm);
                }
                else if (selectedUpgradeOption == "Fuel Flow")
                {
                    upgradeFunction = KRnDGUI.UpgradeFuelFlow;
                    currentUpgradeCount = currentUpgrade.fuelFlow;
                    nextUpgradeCount = ++nextUpgrade.fuelFlow;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.fuelFlow_improvement, rndModule.fuelFlow_improvementScale, currentUpgrade.fuelFlow);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.fuelFlow_improvement, rndModule.fuelFlow_improvementScale, nextUpgrade.fuelFlow);
                    scienceCost = KRnD.calculateScienceCost(rndModule.fuelFlow_scienceCost, rndModule.fuelFlow_costScale, nextUpgrade.fuelFlow);
                }
                else if (selectedUpgradeOption == "Dry Mass")
                {
                    upgradeFunction = KRnDGUI.UpgradeDryMass;
                    currentUpgradeCount = currentUpgrade.dryMass;
                    nextUpgradeCount = ++nextUpgrade.dryMass;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.dryMass_improvement, rndModule.dryMass_improvementScale, currentUpgrade.dryMass);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.dryMass_improvement, rndModule.dryMass_improvementScale, nextUpgrade.dryMass);

                    // Scale science cost with original mass:
                    PartStats originalStats;
                    if (!KRnD.originalStats.TryGetValue(part.name, out originalStats)) throw new Exception("no origional-stats for part '" + part.name + "'");
                    float scaleReferenceFactor = 1;
                    if (rndModule.dryMass_costScaleReference > 0) scaleReferenceFactor = originalStats.mass / rndModule.dryMass_costScaleReference;
                    int scaledCost = (int) Math.Round(rndModule.dryMass_scienceCost * scaleReferenceFactor);
                    if (scaledCost < 1) scaledCost = 1;
                    scienceCost = KRnD.calculateScienceCost(scaledCost, rndModule.dryMass_costScale, nextUpgrade.dryMass);
                }
                else if (selectedUpgradeOption == "Torque")
                {
                    upgradeFunction = KRnDGUI.UpgradeTorque;
                    currentUpgradeCount = currentUpgrade.torque;
                    nextUpgradeCount = ++nextUpgrade.torque;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.torque_improvement, rndModule.torque_improvementScale, currentUpgrade.torque);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.torque_improvement, rndModule.torque_improvementScale, nextUpgrade.torque);
                    scienceCost = KRnD.calculateScienceCost(rndModule.torque_scienceCost, rndModule.torque_costScale, nextUpgrade.torque);
                }
                else if (selectedUpgradeOption == "Charge Rate")
                {
                    upgradeFunction = KRnDGUI.UpgradeChargeRate;
                    currentUpgradeCount = currentUpgrade.chargeRate;
                    nextUpgradeCount = ++nextUpgrade.chargeRate;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.chargeRate_improvement, rndModule.chargeRate_improvementScale, currentUpgrade.chargeRate);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.chargeRate_improvement, rndModule.chargeRate_improvementScale, nextUpgrade.chargeRate);
                    scienceCost = KRnD.calculateScienceCost(rndModule.chargeRate_scienceCost, rndModule.chargeRate_costScale, nextUpgrade.chargeRate);
                }
                else if (selectedUpgradeOption == "Crash Tolerance")
                {
                    upgradeFunction = KRnDGUI.UpgradeCrashTolerance;
                    currentUpgradeCount = currentUpgrade.crashTolerance;
                    nextUpgradeCount = ++nextUpgrade.crashTolerance;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.crashTolerance_improvement, rndModule.crashTolerance_improvementScale, currentUpgrade.crashTolerance);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.crashTolerance_improvement, rndModule.crashTolerance_improvementScale, nextUpgrade.crashTolerance);
                    scienceCost = KRnD.calculateScienceCost(rndModule.crashTolerance_scienceCost, rndModule.crashTolerance_costScale, nextUpgrade.crashTolerance);
                }
                else if (selectedUpgradeOption == "Battery")
                {
                    upgradeFunction = KRnDGUI.UpgradeBatteryCharge;
                    currentUpgradeCount = currentUpgrade.batteryCharge;
                    nextUpgradeCount = ++nextUpgrade.batteryCharge;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.batteryCharge_improvement, rndModule.batteryCharge_improvementScale, currentUpgrade.batteryCharge);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.batteryCharge_improvement, rndModule.batteryCharge_improvementScale, nextUpgrade.batteryCharge);

                    // Scale science cost with original battery charge:
                    PartStats originalStats;
                    if (!KRnD.originalStats.TryGetValue(part.name, out originalStats)) throw new Exception("no origional-stats for part '" + part.name + "'");
                    double scaleReferenceFactor = 1;
                    if (rndModule.batteryCharge_costScaleReference > 0) scaleReferenceFactor = originalStats.batteryCharge / rndModule.batteryCharge_costScaleReference;
                    int scaledCost = (int)Math.Round(rndModule.batteryCharge_scienceCost * scaleReferenceFactor);
                    if (scaledCost < 1) scaledCost = 1;
                    scienceCost = KRnD.calculateScienceCost(scaledCost, rndModule.batteryCharge_costScale, nextUpgrade.batteryCharge);
                }
                else if (selectedUpgradeOption == "Generator")
                {
                    upgradeFunction = KRnDGUI.UpgradeGeneratorEfficiency;
                    currentUpgradeCount = currentUpgrade.generatorEfficiency;
                    nextUpgradeCount = ++nextUpgrade.generatorEfficiency;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.generatorEfficiency_improvement, rndModule.generatorEfficiency_improvementScale, currentUpgrade.generatorEfficiency);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.generatorEfficiency_improvement, rndModule.generatorEfficiency_improvementScale, nextUpgrade.generatorEfficiency);
                    scienceCost = KRnD.calculateScienceCost(rndModule.generatorEfficiency_scienceCost, rndModule.generatorEfficiency_costScale, nextUpgrade.generatorEfficiency);
                }
                else if (selectedUpgradeOption == "Converter")
                {
                    upgradeFunction = KRnDGUI.UpgradeConverterEfficiency;
                    currentUpgradeCount = currentUpgrade.converterEfficiency;
                    nextUpgradeCount = ++nextUpgrade.converterEfficiency;
                    currentImprovement = KRnD.calculateImprovementFactor(rndModule.converterEfficiency_improvement, rndModule.converterEfficiency_improvementScale, currentUpgrade.converterEfficiency);
                    nextImprovement = KRnD.calculateImprovementFactor(rndModule.converterEfficiency_improvement, rndModule.converterEfficiency_improvementScale, nextUpgrade.converterEfficiency);
                    scienceCost = KRnD.calculateScienceCost(rndModule.converterEfficiency_scienceCost, rndModule.converterEfficiency_costScale, nextUpgrade.converterEfficiency);
                }
                else throw new Exception("unexpected option '" + selectedUpgradeOption + "'");
                String newInfo = getPartInfo(part, nextUpgrade); // Calculate part-info if the selected stat was upgraded.
                newInfo = highlightChanges(currentInfo, newInfo);

                // Current stats:
                GUILayout.BeginArea(new Rect(10 + optionsWidth + 10, 30, windowStyle.fixedWidth, 20));
                GUILayout.Label("<color=#FFFFFF><b>Current:</b> " + currentUpgradeCount.ToString() + " (" + currentImprovement.ToString("+0.##%;-0.##%;-") + ")</color>", this.labelStyle);
                GUILayout.EndArea();

                float areaWidth = ( this.windowStyle.fixedWidth - 20 - optionsWidth ) / 2;
                float areaHeight = optionsHeight;
                GUILayout.BeginArea(new Rect(10 + optionsWidth, 30 + 20, areaWidth, areaHeight));
                this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, this.scrollStyle, GUILayout.Width(areaWidth), GUILayout.Height(areaHeight));

                GUILayout.Label(currentInfo, this.labelStyleSmall);
                GUILayout.EndScrollView();
                GUILayout.EndArea();

                // Next stats:
                GUILayout.BeginArea(new Rect(10 + optionsWidth + areaWidth + 10, 30, windowStyle.fixedWidth, 20));
                GUILayout.Label("<color=#FFFFFF><b>Next upgrade:</b> " + nextUpgradeCount.ToString() + " (" + nextImprovement.ToString("+0.##%;-0.##%;-") + ")</color>", this.labelStyle);
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(10 + optionsWidth + areaWidth, 30 + 20, areaWidth, areaHeight));
                this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, this.scrollStyle, GUILayout.Width(areaWidth), GUILayout.Height(areaHeight));
                GUILayout.Label(newInfo, this.labelStyleSmall);
                GUILayout.EndScrollView();
                GUILayout.EndArea();

                // Bottom-line (display only if the upgrade would have an effect):
                if (currentImprovement != nextImprovement)
                {
                    GUILayout.BeginArea(new Rect(10, windowStyle.fixedHeight - 25, windowStyle.fixedWidth, 30));
                    float currentScience = 0;
                    if (ResearchAndDevelopment.Instance != null) currentScience = ResearchAndDevelopment.Instance.Science;
                    String color = "FF0000";
                    if (currentScience >= scienceCost) color = "00FF00";
                    GUILayout.Label("<b>Science: <color=#" + color + ">" + scienceCost.ToString() + " / " + Math.Floor(currentScience).ToString() + "</color></b>", this.labelStyle);
                    GUILayout.EndArea();
                    if (currentScience >= scienceCost && ResearchAndDevelopment.Instance != null && upgradeFunction != null)
                    {
                        GUILayout.BeginArea(new Rect(windowStyle.fixedWidth - 110, windowStyle.fixedHeight - 30, 100, 30));
                        if (GUILayout.Button("Research", this.buttonStyle))
                        {
                            upgradeFunction(part);
                            ResearchAndDevelopment.Instance.AddScience(-scienceCost, TransactionReasons.RnDTechResearch);
                        }
                        GUILayout.EndArea();
                    }
                }

                GUILayout.EndVertical();
                GUI.DragWindow();
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] GenerateWindow(): " + e.ToString());
            }
        }
    }
}
