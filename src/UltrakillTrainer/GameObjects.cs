using System;
using System.Globalization;

namespace UltrakillTrainer
{
    public class GameObjects
    {
        // Stamina pointer chain
        // "UnityPlayer.dll"+017B6DF8,A0,40,60,28,130,28,288
        public const string STAMINA_PTR =
            "\"UnityPlayer.dll\"+017B6DF8,A0,40,60,28,130,28,288";
        public const float MAX_STAMINA = 300f;

        private Memory mem;
        private IntPtr staminaAddr = IntPtr.Zero;

        public GameObjects(Memory memory) { mem = memory; }

        public void ApplyInfiniteStamina()
        {
            if (staminaAddr == IntPtr.Zero)
            {
                staminaAddr = ResolvePointer(STAMINA_PTR);
                // Console.WriteLine("[STAM] Resolved = 0x" + staminaAddr.ToString("X"));
            }
            if (staminaAddr == IntPtr.Zero) return;

            try
            {
                float st = mem.ReadFloat(staminaAddr);
                if (float.IsNaN(st) || st < -10f || st > 500f) return;
                mem.WriteFloat(staminaAddr, MAX_STAMINA);
            }
            catch { }
        }

        private IntPtr ResolvePointer(string ptrStr)
        {
            try
            {
                int q = ptrStr.IndexOf('"', 1);
                string module = ptrStr.Substring(1, q - 1);
                string rest = ptrStr.Substring(q + 1).TrimStart('+');
                string[] parts = rest.Split(',');

                int baseOffset = int.Parse(parts[0], NumberStyles.HexNumber);
                int[] offsets = new int[parts.Length - 1];
                for (int i = 1; i < parts.Length; i++)
                    offsets[i - 1] = int.Parse(parts[i], NumberStyles.HexNumber);

                IntPtr moduleBase = mem.GetModuleBase(module);
                if (moduleBase == IntPtr.Zero) return IntPtr.Zero;

                IntPtr addr = new IntPtr((long)moduleBase + baseOffset);
                return mem.FindDMAAddy(addr, offsets);
            }
            catch { return IntPtr.Zero; }
        }
    }
}