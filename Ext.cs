using System.Runtime.InteropServices;
using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace HitMarker;

[StructLayout(LayoutKind.Sequential)]
public struct variant_t
{
    public nint valuePtr;
    public fieldtype_t fieldType;
    public ushort m_flags;
}

public static class ExtendedFunctions
{
    private static readonly MemoryFunctionVoid<nint, string, nint, nint, nint, int> AcceptInputFunc =
        new(GameData.GetSignature("CEntityInstance_AcceptInput"));

    private static readonly Action<nint, string, nint, nint, nint, int> AcceptInput = AcceptInputFunc.Invoke;

    public static unsafe void FireInput(this CBaseEntity? ent, string input, string param = "",
        CBaseEntity? activator = null, CBaseEntity? caller = null)
    {
        if (ent == null || !ent.IsValid)
            throw new ArgumentNullException(nameof(ent));

        var strBytes = Encoding.ASCII.GetBytes(param + "\0");

        var secondParam = (variant_t*)Marshal.AllocHGlobal(0xB);
        var paramStrPtr = Marshal.AllocHGlobal(strBytes.Length);

        secondParam->fieldType = fieldtype_t.FIELD_STRING;
        secondParam->valuePtr = paramStrPtr;

        Marshal.Copy(strBytes, 0, paramStrPtr, strBytes.Length);

        AcceptInput(ent!.Handle, input, activator?.Handle ?? 0, caller?.Handle ?? 0, (nint)secondParam, 0);

        Marshal.FreeHGlobal(paramStrPtr);
        Marshal.FreeHGlobal((nint)secondParam);
    }
}