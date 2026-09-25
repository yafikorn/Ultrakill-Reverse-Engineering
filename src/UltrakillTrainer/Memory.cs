using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UltrakillTrainer
{
    public class Memory
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x0001,
            CreateThread = 0x0002,
            VMOperation = 0x0008,
            VMRead = 0x0010,
            VMWrite = 0x0020,
            DupHandle = 0x0040,
            CreateProcess = 0x0080,
            SetQuota = 0x0100,
            SetInformation = 0x0200,
            QueryInformation = 0x0400,
            QueryLimitedInformation = 0x1000,
            Synchronize = 0x00100000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        const uint MEM_COMMIT = 0x1000;
        const uint PAGE_NOACCESS = 0x01;
        const uint PAGE_GUARD = 0x100;

        const uint TH32CS_SNAPMODULE = 0x00000008;
        const uint TH32CS_SNAPMODULE32 = 0x00000010;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, int th32ProcessID);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        private IntPtr hProcess = IntPtr.Zero;
        private Process process = null;

        public bool OpenProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                return false;
            }

            process = processes[0];
            hProcess = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            return hProcess != IntPtr.Zero;
        }

        public IntPtr GetModuleBase(string moduleName)
        {
            if (process == null) return IntPtr.Zero;

            IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, process.Id);
            if (hSnapshot != IntPtr.Zero && hSnapshot != new IntPtr(-1))
            {
                MODULEENTRY32 modEntry = new MODULEENTRY32();
                modEntry.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));

                if (Module32First(hSnapshot, ref modEntry))
                {
                    do
                    {
                        if (modEntry.szModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                        {
                            CloseHandle(hSnapshot);
                            return modEntry.modBaseAddr;
                        }
                    } while (Module32Next(hSnapshot, ref modEntry));
                }
                CloseHandle(hSnapshot);
            }

            try
            {
                foreach (ProcessModule module in process.Modules)
                {
                    if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        return module.BaseAddress;
                    }
                }
            }
            catch { }

            return IntPtr.Zero;
        }

        public IntPtr FindDMAAddy(IntPtr ptr, int[] offsets)
        {
            IntPtr addr = ptr;
            byte[] buffer = new byte[8];
            for (int i = 0; i < offsets.Length; i++)
            {
                IntPtr bytesRead;
                if (!ReadProcessMemory(hProcess, addr, buffer, buffer.Length, out bytesRead) || bytesRead == IntPtr.Zero)
                    return IntPtr.Zero;

                long baseAddr = BitConverter.ToInt64(buffer, 0);
                if (baseAddr == 0) return IntPtr.Zero;

                addr = new IntPtr(baseAddr + offsets[i]);
            }
            return addr;
        }

        public byte ReadByte(IntPtr address)
        {
            byte[] buffer = new byte[1];
            IntPtr bytesRead;
            ReadProcessMemory(hProcess, address, buffer, 1, out bytesRead);
            return buffer[0];
        }

        public int ReadInt32(IntPtr address)
        {
            byte[] buffer = new byte[4];
            IntPtr bytesRead;
            ReadProcessMemory(hProcess, address, buffer, 4, out bytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        public long ReadInt64(IntPtr address)
        {
            byte[] buffer = new byte[8];
            IntPtr bytesRead;
            ReadProcessMemory(hProcess, address, buffer, 8, out bytesRead);
            return BitConverter.ToInt64(buffer, 0);
        }

        public float ReadFloat(IntPtr address)
        {
            byte[] buffer = new byte[4];
            IntPtr bytesRead;
            ReadProcessMemory(hProcess, address, buffer, 4, out bytesRead);
            return BitConverter.ToSingle(buffer, 0);
        }

        public bool WriteInt32(IntPtr address, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            IntPtr bytesWritten;
            return WriteProcessMemory(hProcess, address, buffer, buffer.Length, out bytesWritten);
        }

        public bool WriteFloat(IntPtr address, float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            IntPtr bytesWritten;
            return WriteProcessMemory(hProcess, address, buffer, buffer.Length, out bytesWritten);
        }

        public List<IntPtr> ScanBytePattern(byte[] pattern)
        {
            List<IntPtr> matches = new List<IntPtr>();
            if (hProcess == IntPtr.Zero || pattern == null || pattern.Length == 0)
                return matches;

            long minAddr = 0x10000;
            long maxAddr = 0x7FFFFFFFFFFF; // 64-bit address space range

            IntPtr address = new IntPtr(minAddr);
            MEMORY_BASIC_INFORMATION mbi;
            uint mbiSize = (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));

            while ((long)address < maxAddr && VirtualQueryEx(hProcess, address, out mbi, mbiSize) != 0)
            {
                bool isReadable = (mbi.State == MEM_COMMIT) &&
                                 ((mbi.Protect & PAGE_GUARD) == 0) &&
                                 ((mbi.Protect & PAGE_NOACCESS) == 0);

                if (isReadable)
                {
                    long regionSize = (long)mbi.RegionSize;
                    if (regionSize > 0 && regionSize <= 0x10000000) // max 256MB per region read chunk
                    {
                        byte[] buffer = new byte[regionSize];
                        IntPtr bytesRead;
                        if (ReadProcessMemory(hProcess, mbi.BaseAddress, buffer, (int)regionSize, out bytesRead))
                        {
                            int readLen = (int)bytesRead;
                            int patternLen = pattern.Length;

                            for (int i = 0; i <= readLen - patternLen; i++)
                            {
                                bool match = true;
                                for (int j = 0; j < patternLen; j++)
                                {
                                    if (buffer[i + j] != pattern[j])
                                    {
                                        match = false;
                                        break;
                                    }
                                }

                                if (match)
                                {
                                    matches.Add(new IntPtr((long)mbi.BaseAddress + i));
                                }
                            }
                        }
                    }
                }

                long nextAddr = (long)mbi.BaseAddress + (long)mbi.RegionSize;
                if (nextAddr <= (long)address) break; // Avoid infinite loop
                address = new IntPtr(nextAddr);
            }

            return matches;
        }

        ~Memory()
        {
            if (hProcess != IntPtr.Zero)
            {
                CloseHandle(hProcess);
            }
        }
    }
}