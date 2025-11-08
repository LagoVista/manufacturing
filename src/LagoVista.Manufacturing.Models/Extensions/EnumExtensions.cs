// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 18ca57235917173553e18e31f86eec25511f5e2c4055db5fe72ddcfa54bb7385
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista
{
    public static class EnumExtensions
    {
        public static double ToDouble(this EntityHeader<TapePitches> value, TapePitches defaultValue = TapePitches.FourMM)
        {
            var enumValue = EntityHeader.IsNullOrEmpty(value) ? defaultValue : value.Value;

            switch (enumValue)
            {
                case TapePitches.TwoMM: return 2.0;
                case TapePitches.FourMM: return 4.0;
                case TapePitches.EightMM: return 8.0;
                case TapePitches.TwelveMM: return 12.0;
                case TapePitches.SixteenMM: return 16.0;
                case TapePitches.TwentyMM: return 20.0;
                case TapePitches.TwentyFourMM: return 24.0;
                case TapePitches.TwentyEightMM: return 28.0;
                case TapePitches.ThirtyTwoMM: return 32.0;                
            }

            throw new Exception($"Unsupported tape pitch: {value}");
        }
        public static double ToDouble(this EntityHeader<TapeSizes> value, TapeSizes defaultValue = TapeSizes.EightMM)
        {
            var enumValue = EntityHeader.IsNullOrEmpty(value) ? defaultValue : value.Value;

            switch (enumValue)
            {
                case TapeSizes.EightMM: return 8;
                case TapeSizes.TwelveMM: return 12;
                case TapeSizes.SixteenMM: return 16;
                case TapeSizes.TwentyMM: return 20;
                case TapeSizes.TwentyFourMM: return 24;
                case TapeSizes.ThirtyTwoMM: return 32;
                case TapeSizes.FortyFourMM: return 44;
            }

            throw new Exception($"Unsupported tape size: {value}");
        }
    }
}
