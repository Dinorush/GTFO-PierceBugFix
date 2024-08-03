using HarmonyLib;
using static Weapon;
using UnityEngine;
using System.Reflection;

namespace PierceBugFix
{
    [HarmonyPatch]
    internal static class PierceSpreadPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase FindWeaponRayFunc(Harmony harmony)
        {
            return AccessTools.Method(
                typeof(Weapon),
                nameof(Weapon.CastWeaponRay),
                new Type[] { typeof(Transform), typeof(Weapon.WeaponHitData).MakeByRefType(), typeof(Vector3), typeof(int) }
                );
        }

        [HarmonyPostfix]
        private static void PostRayCallback(ref WeaponHitData weaponRayData)
        {
            // Reset shotgun spread data so that shotguns don't re-evaluate spread every pierce.
            weaponRayData.randomSpread = 0;
            weaponRayData.angOffsetX = 0;
            weaponRayData.angOffsetY = 0;
        }
    }
}
