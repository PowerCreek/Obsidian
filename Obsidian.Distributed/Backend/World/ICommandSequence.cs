using System.Runtime.CompilerServices;

namespace Obsidian.Distributed.Backend.World;

public interface ICommandSequence { }

public static class ICommandSequenceExt
{
    public static unsafe byte[] Serialize<T>(this T t) where T : ICommandSequence
    {
        int structSize = Unsafe.SizeOf<T>();
        byte[] data = new byte[structSize];
        Span<byte> dataSpan = new Span<byte>(data);
        fixed (byte* pData = data)
        {
            Unsafe.Write(pData, t);
        }
        return data;
    }

    public static unsafe T Deserialize<T>(this byte[] data) where T : ICommandSequence
    {
        T result;
        fixed (byte* pData = data)
        {
            result = Unsafe.Read<T>(pData);
        }
        return result;
    }
}
