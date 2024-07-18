using Iced.Intel;

namespace PierceBugFix;

public class PierceBugDisassembler
{
    public static unsafe IntPtr FindThirdInc(IntPtr methodPointer)
    {
        StreamCodeReader streamCodeReader = new((Stream)new UnmanagedMemoryStream((byte*)methodPointer, 65536L, 65536L, (FileAccess)1));
        Decoder decoder = Decoder.Create(sizeof(void*) * 8, streamCodeReader);
        decoder.IP = (ulong)(long)methodPointer;

        int incCount = 0;
        Instruction instruction = new();
        decoder.Decode(out instruction);
        while (instruction.Mnemonic != Mnemonic.Int3)
        // `Int3` is an opcode that is sometimes used to halt execution for a debugger. We
        // can be reasonably sure that it will appear after our method and never be inside
        // it.
        {
            //Logger.Info($"{instruction.Code} {instruction} {instruction.IP:X2} {*(ulong*)(instruction.IP):X2}");
            if (instruction.Mnemonic == Mnemonic.Inc)
            {
                ++incCount;
                if (incCount == 3)
                {
                    streamCodeReader.Stream.Dispose();
                    return (IntPtr)(long)instruction.IP;
                }
            }
            decoder.Decode(out instruction);
        }
        streamCodeReader.Stream.Dispose();
        return IntPtr.Zero;
    }
}
