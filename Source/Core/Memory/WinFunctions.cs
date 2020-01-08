namespace Spotlight.Core.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static unsafe class WinFunctions
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr moduleHandle, string procName);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccess desiredAccess, bool inheritHandle, int threadId);
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        public delegate int NtQueryInformationThreadDelegate(IntPtr threadHandle, uint threadInformationClass, THREAD_BASIC_INFORMATION* outThreadInformation, ulong threadInformationLength, ulong* returnLength);
        public static NtQueryInformationThreadDelegate NtQueryInformationThread { get; }

        static WinFunctions()
        {
            IntPtr ntdllHandle = GetModuleHandle("ntdll.dll");
            NtQueryInformationThread = Marshal.GetDelegateForFunctionPointer<NtQueryInformationThreadDelegate>(GetProcAddress(ntdllHandle, "NtQueryInformationThread"));
        }

        public static int GetProcessMainThreadId()
        {
            long lowestStartTime = long.MaxValue;
            ProcessThread lowestStartTimeThread = null;
            foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
            {
                long startTime = thread.StartTime.Ticks;
                if (startTime < lowestStartTime)
                {
                    lowestStartTime = startTime;
                    lowestStartTimeThread = thread;
                }
            }

            return lowestStartTimeThread == null ? -1 : lowestStartTimeThread.Id;
        }


        public static void CopyTlsValues(IntPtr sourceThreadHandle, IntPtr targetThreadHandle, params int[] valuesOffsets)
        {
            THREAD_BASIC_INFORMATION sourceThreadInfo = new THREAD_BASIC_INFORMATION();
            THREAD_BASIC_INFORMATION targetThreadInfo = new THREAD_BASIC_INFORMATION();

            int sourceStatus = NtQueryInformationThread(sourceThreadHandle, 0, &sourceThreadInfo, (ulong)sizeof(THREAD_BASIC_INFORMATION), null);
            if (sourceStatus != 0)
            {
                Rage.Game.LogTrivialDebug($"Source Thread Invalid Query Status: {sourceStatus}");
                return;
            }

            int targetStatus = NtQueryInformationThread(targetThreadHandle, 0, &targetThreadInfo, (ulong)sizeof(THREAD_BASIC_INFORMATION), null);
            if (targetStatus != 0)
            {
                Rage.Game.LogTrivialDebug($"Target Thread Invalid Query Status: {targetStatus}");
                return;
            }

            TEB* sourceTeb = (TEB*)sourceThreadInfo.TebBaseAddress;
            TEB* targetTeb = (TEB*)targetThreadInfo.TebBaseAddress;

            foreach (int offset in valuesOffsets)
            {
                *(long*)(*(byte**)(targetTeb->ThreadLocalStoragePointer) + offset) = *(long*)(*(byte**)(sourceTeb->ThreadLocalStoragePointer) + offset);
            }
        }

        public static void CopyTlsValues(int sourceThreadId, int targetThreadId, params int[] valuesOffsets)
        {
            IntPtr sourceThreadHandle = IntPtr.Zero, targetThreadHandle = IntPtr.Zero;
            try
            {
                sourceThreadHandle = OpenThread(ThreadAccess.QUERY_INFORMATION, false, sourceThreadId);
                targetThreadHandle = OpenThread(ThreadAccess.QUERY_INFORMATION, false, targetThreadId);

                CopyTlsValues(sourceThreadHandle, targetThreadHandle, valuesOffsets);
            }
            finally
            {
                if (sourceThreadHandle != IntPtr.Zero)
                    CloseHandle(sourceThreadHandle);
                if (targetThreadHandle != IntPtr.Zero)
                    CloseHandle(targetThreadHandle);
            }
        }

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x30)]
        public struct THREAD_BASIC_INFORMATION
        {
            [FieldOffset(0x0000)] public int ExitStatus;
            [FieldOffset(0x0008)] public IntPtr TebBaseAddress;
        }

        // http://msdn.moonsols.com/win7rtm_x64/TEB.html
        [StructLayout(LayoutKind.Explicit, Size = 0x1818)]
        public struct TEB
        {
            [FieldOffset(0x0058)] public IntPtr ThreadLocalStoragePointer;
        }
    }
}
