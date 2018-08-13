namespace Spotlight
{
    using System;

    internal static class VehiclesUpdateHook
    {
        public delegate void VehiclesUpdateEventHandler();
        private delegate byte VehiclesUpdateDelegate(IntPtr unk);

        private const int FuncCount = 60;
        private static IntPtr[] replacedFuncLocations;
        private static IntPtr[] replacedFuncs;
        private static VehiclesUpdateDelegate[] replacedFuncDelegates;

        private static IntPtr[] detourFuncs;
        private static VehiclesUpdateDelegate[] detourDelegates;

        public static event VehiclesUpdateEventHandler VehiclesUpdate;

        public static unsafe void Hook()
        {
            replacedFuncLocations = new IntPtr[FuncCount];
            replacedFuncs = new IntPtr[FuncCount];
            replacedFuncDelegates = new VehiclesUpdateDelegate[FuncCount];
            detourFuncs = new IntPtr[FuncCount];
            detourDelegates = new VehiclesUpdateDelegate[FuncCount];
            
            for (int i = 0; i < FuncCount; i++)
            {
                int indexCopy = i;
                IntPtr funcAddress = Core.Memory.GameMemory.VehiclesUpdateHook + (0xA0 * i);
                replacedFuncs[i] = *(IntPtr*)funcAddress;
                replacedFuncLocations[i] = funcAddress;
                replacedFuncDelegates[i] = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<VehiclesUpdateDelegate>(replacedFuncs[i]);

                detourDelegates[i] = (IntPtr ptr) =>
                {
                    byte result = replacedFuncDelegates[indexCopy](ptr);
                    VehiclesUpdate?.Invoke();
                    return result;
                };
                detourFuncs[i] = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(detourDelegates[i]);

                *(IntPtr*)funcAddress = detourFuncs[i];
            }
        }

        public static unsafe void Unhook()
        {
            if (replacedFuncLocations != null && replacedFuncs != null)
            {
                for (int i = 0; i < FuncCount; i++)
                {
                    *(IntPtr*)replacedFuncLocations[i] = replacedFuncs[i];
                }
            }

            replacedFuncLocations = null;
            replacedFuncs = null;
            replacedFuncDelegates = null;
            detourFuncs = null;
            detourDelegates = null;
        }
    }
}
