using System.Collections;
using System.Reflection;
using UnityEngine;

// To use, reference the setting using the following:
//
//  HighLogic.CurrentGame.Parameters.CustomParams<LandingAim_Options>().
//
//
namespace LandingAim
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class LandingAim_Options : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Default Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Landing Aim"; } }
        public override string DisplaySection { get { return "Landing Aim"; } }
        public override int SectionOrder { get { return 1; } }


        [GameParameters.CustomParameterUI("Require minimum pilot experience level",
            toolTip = "if set to yes, then you will need to have a pilot onboard with at least a specified level of stars")]
        public bool needsMinimumPilotLevel = true;


        [GameParameters.CustomIntParameterUI("Minimum pilot level required", minValue = 0, maxValue = 5,
            toolTip = "Require a pilot with a minimum number of stars to have this active")]
        public int minPilotLevel = 0;


        [GameParameters.CustomParameterUI("Require Prograde/Retrograde")]
            public bool needsProgradeRetrograd = false;

        [GameParameters.CustomParameterUI("Require Normal/Anti-normal/Radial",
            toolTip = "Also requires Prograde/Retrograde")]
        public bool needsNormalAntinormal = false;

        [GameParameters.CustomParameterUI("Require Target/Anti-target",
            toolTip = "Also requires Normal/Anti-normal/Radial")]
        public bool needsTargetAntitarget = false;

        [GameParameters.CustomParameterUI("Require Full SAS (incl. maneuver hold)",
            toolTip = "Also requires Target/Anti-target")]
        public bool needsFullSAS = false;


#if true
        public override bool HasPresets { get { return true; } }
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    minPilotLevel = 0;
                    needsProgradeRetrograd = false;
                    needsNormalAntinormal = false;
                    needsTargetAntitarget = false;
                    needsFullSAS = false;

                    break;

                case GameParameters.Preset.Normal:
                    minPilotLevel = 1;
                    needsProgradeRetrograd = false;
                    needsNormalAntinormal = false;
                    needsTargetAntitarget = false;
                    needsFullSAS = false;
                    break;

                case GameParameters.Preset.Moderate:
                    minPilotLevel = 3;
                    needsProgradeRetrograd = false;
                    needsNormalAntinormal = false;
                    needsTargetAntitarget = false;
                    needsFullSAS = false;
                    break;

                case GameParameters.Preset.Hard:
                    minPilotLevel = 4;
                    needsProgradeRetrograd = false;
                    needsNormalAntinormal = false;
                    needsTargetAntitarget = false;
                    needsFullSAS = false;
                    break;
            }
        }

#else
        public override bool HasPresets { get { return false; } }
        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
#endif

        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }
        public override bool Interactible(MemberInfo member, GameParameters parameters) 
        {
            if (needsFullSAS)
                needsTargetAntitarget = true;
            if (needsTargetAntitarget)
                needsNormalAntinormal = true;
            if (needsNormalAntinormal)
                needsProgradeRetrograd = true;
            return true; 
        }
        public override IList ValidValues(MemberInfo member) { return null; }
    }
}

