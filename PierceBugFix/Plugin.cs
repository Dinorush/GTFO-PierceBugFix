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
    public override unsafe void Load()
    {
        Logger.SetupFromInit(this.Log);
        Logger.Info("PierceBugFix is loading...");
        INativeClassStruct classStruct = UnityVersionHandler.Wrap((Il2CppClass*) Il2CppClassPointerStore<BulletWeapon>.NativeClassPtr);
        for (int i = 0; i < classStruct.MethodCount; ++i)
        {
            INativeMethodInfoStruct methodInfoStruct = UnityVersionHandler.Wrap(classStruct.Methods[i]);
            if (Marshal.PtrToStringAnsi(methodInfoStruct.Name) == "Fire")
            {
                IntPtr methodPointer = methodInfoStruct.MethodPointer;






                Logger.Info("------------------------------------------------------------------");
                StreamCodeReader streamCodeReader = new((Stream)new UnmanagedMemoryStream((byte*)methodPointer, 65536L, 65536L, (FileAccess)1));
                Decoder decoder = Decoder.Create(sizeof(void*) * 8, streamCodeReader);
                decoder.IP = (ulong)(long)methodPointer;
                Instruction instruction = new();
                decoder.Decode(out instruction);
                while (instruction.Mnemonic != Mnemonic.Int3)
                {
                    Logger.Info($"{instruction.Code} {instruction} {instruction.IP:X}");
                    decoder.Decode(out instruction);
                }
                streamCodeReader.Stream.Dispose();
                Logger.Info("------------------------------------------------------------------");





                IntPtr instructionIP = PierceBugDisassembler.FindThirdInc(methodInfoStruct.MethodPointer);

                using (new MemoryProtectionCookie(instructionIP, Kernel32.MemoryProtectionConstant.ExecuteReadWrite, new IntPtr(16)))
                {
                    *(byte*)(instructionIP) = 0x90;
                    *(byte*)((ulong)instructionIP + (ulong) new IntPtr(1)) = 0x90;
                    *(byte*)((ulong)instructionIP + (ulong) new IntPtr(2)) = 0x90;
                }







                Logger.Info("------------------------------------------------------------------");
                StreamCodeReader streamCodeReader2 = new((Stream)new UnmanagedMemoryStream((byte*)methodPointer, 65536L, 65536L, (FileAccess)1));
                Decoder decoder2 = Decoder.Create(sizeof(void*) * 8, streamCodeReader2);
                decoder2.IP = (ulong)(long)methodPointer;
                Instruction instruction2 = new();
                decoder2.Decode(out instruction2);
                while (instruction2.Mnemonic != Mnemonic.Int3)
                {
                    Logger.Info($"{instruction2.Code} {instruction2} {instruction2.IP:X}");
                    decoder2.Decode(out instruction2);
                }
                streamCodeReader2.Stream.Dispose();
                Logger.Info("------------------------------------------------------------------");



                // Done!
                Logger.Info("PierceBugFix is loaded!");
                return;
            }
        }
        Logger.Error("PierceBugFix failed to find BulletWeapon.Fire");
        Environment.FailFast("PierceBugFix failed to find BulletWeapon.Fire");
    }
}
