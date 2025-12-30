// File: VesselPilotAbility.cs
// KSP1-safe, no LINQ, no internal type references

using Experience;
using System;
using System.Collections.Generic;

public static class VesselPilotAbility
{
    public static int GetHighestPilotLevel(Vessel vessel)
    {
        int highestPilotLevel = 0;

        foreach (var pcm in vessel.GetVesselCrew())
        {
            if (pcm.experienceTrait.TypeName == "Pilot")
                highestPilotLevel = Math.Max(highestPilotLevel, pcm.experienceLevel);
        }
        return highestPilotLevel;
    }

    /// <summary>
    /// Returns the highest SAS (FlightControlState) level
    /// provided by any PILOT aboard the vessel.
    /// </summary>
    public static int GetHighestPilotSasLevel(Vessel vessel)
    {
        if (vessel == null)
            return 0;

        int best = 0;

        // Works for both loaded and unloaded vessels
        List<ProtoCrewMember> crew = vessel.GetVesselCrew();
        if (crew == null)
            return 0;

        for (int i = 0; i < crew.Count; i++)
        {
            int level = GetPilotSasLevel(crew[i]);
            if (level > best)
                best = level;
        }

        return best;
    }

    /// <summary>
    /// Returns the SAS tier for a single kerbal.
    /// Non-pilots return 0.
    /// </summary>
    private static int GetPilotSasLevel(ProtoCrewMember pcm)
    {
        if (pcm == null)
            return 0;

        // Stock pilot trait name
        if (!string.Equals(
                pcm.experienceTrait.TypeName,
                "Pilot",
                StringComparison.OrdinalIgnoreCase))
            return 0;

        List<ExperienceEffect> effects = pcm.experienceTrait.Effects;
        if (effects == null)
            return 0;

        for (int i = 0; i < effects.Count; i++)
        {
            ExperienceEffect effect = effects[i];
            if (effect == null)
                continue;

            // THIS is the key line — string comparison, not type check
            if (effect.Name == "FlightControlState")
            {
                return effect.Level;
            }
        }

        return 0;
    }
}
