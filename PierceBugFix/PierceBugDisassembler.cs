using Iced.Intel;
using UnityEngine.Assertions;

namespace PierceBugFix;

public class PierceBugDisassembler
{
    public const int INC_WE_WANT = 3;
    public const int INC_EXPECTED = 4;
    public const int WIDTH_EXPECTED = 3;
    public static unsafe IntPtr FindThirdInc(IntPtr methodPointer)
    {
        IntPtr instructionIP = IntPtr.Zero;  // Return value, initialized to null.

        // Set up the decoder to go through the instructions.
        StreamCodeReader streamCodeReader = new((Stream)new UnmanagedMemoryStream((byte*)methodPointer, 65536L, 65536L, (FileAccess)1));
        Decoder decoder = Decoder.Create(sizeof(void*) * 8, streamCodeReader);
        decoder.IP = (ulong)(long)methodPointer;
        Instruction instruction = new();
        decoder.Decode(out instruction);

        // We are looking for the third `Inc` instruction in this method.
        int incCount = 0;
        while (instruction.Mnemonic != Mnemonic.Int3)
        // `Int3` is an opcode that is sometimes used to halt execution for a debugger. We
        // can be reasonably sure that it will appear after our method and never be inside
        // it.
        {
            if (instruction.Mnemonic == Mnemonic.Inc)
            {
                ++incCount;
                if (incCount == INC_WE_WANT)
                {
                    instructionIP = (IntPtr)(long)instruction.IP;

                    // Error handling.
                    if ((instruction.NextIP - instruction.IP) != WIDTH_EXPECTED)
                    {
                        Logger.Error("PierceBugFix found an instruction with an unexpected width, this probably means the method has changed in some way and we should avoid changing it.");
                        Environment.FailFast("PierceBugFix found an instruction with an unexpected width, this probably means the method has changed in some way and we should avoid changing it.");
                    }
                }
            }
            decoder.Decode(out instruction);
        }
        streamCodeReader.Stream.Dispose();

        // Error handling.
        if (incCount != INC_EXPECTED)
        {
            Logger.Error("PierceBugFix didn't find the correct number of `Inc` instructions in `BulletWeapon.Fire`, this probably means the method has changed in some way and we should avoid changing it.");
            Environment.FailFast("PierceBugFix didn't find the correct number of `Inc` instructions in `BulletWeapon.Fire`, this probably means the method has changed in some way and we should avoid changing it.");
        }
        if (instructionIP == IntPtr.Zero)
        {
            Logger.Error("PierceBugFix found a zero instruction int pointer for our `Inc` instruction, something has gone terribly wrong.");
            Environment.FailFast("PierceBugFix found a zero instruction int pointer for our `Inc` instruction, something has gone terribly wrong.");
        }
        return instructionIP;
    }
}
