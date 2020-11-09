using RimWorld;
using UnityEngine;
using Verse;

namespace RWHS
{
    /* First step: see the patch which adds data to the xml comp data
     *
     * Second step of this mod: adding this
     * Displaying this information
    */

    [StaticConstructorOnStartup]
    public static class RWHS_TempControl_RoomExchange
    {
        public static float GetCurrentEfficiency(StatRequest req, bool applyPostProcess = true)
        {
            Thing tempController = req.Thing;

            IntVec3 intVec3_2 = tempController.Position + IntVec3.North.RotatedBy(tempController.Rotation);
            IntVec3 intVec3_1 = tempController.Position + IntVec3.South.RotatedBy(tempController.Rotation);

            float cooledRoomTemp = intVec3_1.GetTemperature(tempController.Map);
            float extRoomTemp = intVec3_2.GetTemperature(tempController.Map);
            float efficiencyLossPerDegree = 1.0f / 130.0f; // SOS2 internal value, means loss of efficiency for each degree above targettemp, lose 50% at 65C above targetTemp, 100% at 130+
            float sidesTempGradient = (extRoomTemp - cooledRoomTemp);
            if (extRoomTemp - 40f > sidesTempGradient)
            {
                sidesTempGradient = extRoomTemp - 40f;
            }
            float efficiency = (1f - sidesTempGradient * efficiencyLossPerDegree);
            return efficiency;
        }


        public static float GetCurrentACPerSecond(StatRequest req, bool applyPostProcess = true)
        {
            CompTempControl tempControl = req.Thing.TryGetComp<CompTempControl>();
            Thing tempController = req.Thing;

            IntVec3 intVec3_1 = tempController.Position + IntVec3.South.RotatedBy(tempController.Rotation);

            float cooledRoomTemp = intVec3_1.GetTemperature(tempController.Map);
            float targetTemp = tempControl.targetTemperature;
            float targetTempDiff = targetTemp - cooledRoomTemp;
            float maxACPerSecond = GetMaxACPerSecond(req); // max cooling power possible
            bool isHeater = tempControl.Props.energyPerSecond > 0;
            if (isHeater)
            {
                return Mathf.Max(Mathf.Min(targetTempDiff, maxACPerSecond), 0);
            }
            else
            {
                return Mathf.Min(Mathf.Max(targetTempDiff, maxACPerSecond), 0);
            }
        }

        public static float GetMaxACPerSecond(StatRequest req, bool applyPostProcess = true)
        {
            CompTempControl tempControl = req.Thing.TryGetComp<CompTempControl>();
            Thing tempController = req.Thing;

            IntVec3 intVec3_1 = tempController.Position + IntVec3.South.RotatedBy(tempController.Rotation);

            float energyPerSecond = tempControl.Props.energyPerSecond; // the power of the radiator
            float roomSurface = intVec3_1.GetRoomGroup(tempController.Map).CellCount;
            float coolingConversionRate = 4.16666651f; // Celsius cooled per Joules*Second*Meter^2  conversion rate
            float efficiency = GetCurrentEfficiency(req);
            float maxACPerSecond = energyPerSecond * efficiency / roomSurface * coolingConversionRate; // max cooling power possible
            return maxACPerSecond;
        }
    }
}
