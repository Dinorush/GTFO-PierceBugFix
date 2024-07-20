using BepInEx;
using BepInEx.Unity.IL2CPP;
using Gear;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.Class;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.MethodInfo;
using PierceBugFix.Imports;
using System.Runtime.InteropServices;

namespace PierceBugFix;

[BepInPlugin("tru0067.PierceBugFix", "PierceBugFix", "1.0.0")]
public class Plugin : BasePlugin
{
    public const int NOP = 0x90;
    public override unsafe void Load()
    {
        Logger.SetupFromInit(this.Log);
        Logger.Info("PierceBugFix is loading...");

        PatchBulletWeaponFire<BulletWeapon>(3, 4, 3);
        PatchBulletWeaponFire<Shotgun>(3, 5, 3);

        Logger.Info("PierceBugFix is loaded!");
    }

    private unsafe void PatchBulletWeaponFire<T>(int incIndex, int incTotalExpected, int incWidthExpected) where T : BulletWeapon
    {
        string debugName = typeof(T).Name;

        Logger.Info($"Patching `{debugName}.Fire`...");
        // Look for the appropriate `Fire` method.
        INativeClassStruct classStruct = UnityVersionHandler.Wrap((Il2CppClass*)Il2CppClassPointerStore<T>.NativeClassPtr);
        for (int i = 0; i < classStruct.MethodCount; ++i)
        {
            INativeMethodInfoStruct methodInfoStruct = UnityVersionHandler.Wrap(classStruct.Methods[i]);
            if (Marshal.PtrToStringAnsi(methodInfoStruct.Name) == "Fire")
            {
                // Found the `Fire` method, now find the address of the instruction we want to change.
                IntPtr methodPointer = methodInfoStruct.MethodPointer;
                IntPtr instructionIP = PierceBugDisassembler.FindInc(methodPointer, incIndex, incTotalExpected, incWidthExpected);

                // Change that instruction into `NOP`s.
                using (new MemoryProtectionCookie(instructionIP, Kernel32.MemoryProtectionConstant.ExecuteReadWrite, new IntPtr(16)))
                {
                    for (int j = 0; j < incWidthExpected; ++j)
                    {
                        *(byte*)((ulong)instructionIP + (ulong)new IntPtr(j)) = NOP;
                    }
                }

                // Done!
                Logger.Info($"`{debugName}.Fire` is patched!");
                return;
            }
        }
        Logger.Error($"PierceBugFix failed to find `{debugName}.Fire`");
        Environment.FailFast($"PierceBugFix failed to find `{debugName}.Fire`");
    }
}
