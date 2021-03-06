﻿using System;

namespace Panda.EverythingLauncher.Interop
{
    internal static class ShellHelper
    {
        /// <summary>
        ///     Retrieves the High Word of a WParam of a WindowMessage
        /// </summary>
        /// <param name="ptr">The pointer to the WParam</param>
        /// <returns>The unsigned integer for the High Word</returns>
        public static uint HiWord(IntPtr ptr)
        {
            if (((uint) ptr & 0x80000000) == 0x80000000)
                return (uint) ptr >> 16;
            return ((uint) ptr >> 16) & 0xffff;
        }

        /// <summary>
        ///     Retrieves the Low Word of a WParam of a WindowMessage
        /// </summary>
        /// <param name="ptr">The pointer to the WParam</param>
        /// <returns>The unsigned integer for the Low Word</returns>
        public static uint LoWord(IntPtr ptr)
        {
            return (uint) ptr & 0xffff;
        }
    }
}