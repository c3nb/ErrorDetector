using System.Reflection;
using UnityModManagerNet;

namespace ErrorDetector
{
    public class Call
    {
        public readonly UnityModManager.ModEntry Mod;
        public readonly MethodBase CallingMethod;
        public Call(UnityModManager.ModEntry mod, MethodBase callingMethod)
        {
            Mod = mod;
            CallingMethod = callingMethod;
        }
        public override string ToString() => $"{(Mod != null ? $"\"{Mod.Info.DisplayName}\" Method" : "Internal Method")} {Error.FormatMethod(CallingMethod)}";
    }
}
