using BepInEx;
using BepInEx.Unity.IL2CPP;
using Gear;
using Iced.Intel;
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
        // Look for `BulletWeapon.Fire`.
        INativeClassStruct classStruct = UnityVersionHandler.Wrap((Il2CppClass*) Il2CppClassPointerStore<BulletWeapon>.NativeClassPtr);
        for (int i = 0; i < classStruct.MethodCount; ++i)
        {
            INativeMethodInfoStruct methodInfoStruct = UnityVersionHandler.Wrap(classStruct.Methods[i]);
            if (Marshal.PtrToStringAnsi(methodInfoStruct.Name) == "Fire")
            {
                // Found `BulletWeapon.Fire`, now find the address of the instruction we want to change.
                IntPtr methodPointer = methodInfoStruct.MethodPointer;
                IntPtr instructionIP = PierceBugDisassembler.FindThirdInc(methodPointer);

                // Change that instruction into `NOP`s.
                using (new MemoryProtectionCookie(instructionIP, Kernel32.MemoryProtectionConstant.ExecuteReadWrite, new IntPtr(16)))
                {
                    for (int j = 0; j < PierceBugDisassembler.WIDTH_EXPECTED; ++j)
                    {
                        *(byte*)((ulong)instructionIP + (ulong)new IntPtr(j)) = NOP;
                    }
                }

                // Done!
                Logger.Info("PierceBugFix is loaded!");
                return;
            }
        }
        Logger.Error("PierceBugFix failed to find `BulletWeapon.Fire`");
        Environment.FailFast("PierceBugFix failed to find `BulletWeapon.Fire`");
    }
}
