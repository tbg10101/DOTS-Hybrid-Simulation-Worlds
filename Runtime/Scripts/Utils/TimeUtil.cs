using Unity.Burst;
using UnityEngine;

namespace Software10101.DOTS.Utils {
    public class TimeUtil {
        public static float FixedDeltaTime => Time.fixedDeltaTime;
        public static float NextFixedTime => Time.fixedTime + InnerTimeOffset.Data;
        public static float PreviousFixedTime => NextFixedTime - FixedDeltaTime + InnerTimeOffset.Data;
        public static float PresentationTime => Time.time - FixedDeltaTime + InnerTimeOffset.Data;
        public static float PresentationTimeFraction => (PresentationTime - PreviousFixedTime) / (NextFixedTime - PreviousFixedTime);
        private static readonly SharedStatic<float> InnerTimeOffset = SharedStatic<float>.GetOrCreate<TimeUtil, FloatFieldKey>();

        public static float TimeOffset {
            get => InnerTimeOffset.Data;
            set => InnerTimeOffset.Data = value;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class FloatFieldKey { }
    }
}
