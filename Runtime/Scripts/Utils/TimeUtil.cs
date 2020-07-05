using UnityEngine;

namespace Software10101.DOTS.Utils {
    public static class TimeUtil {
        public static float FixedDeltaTime => Time.fixedDeltaTime;
        public static float NextFixedTime => Time.fixedTime + TimeOffset;
        public static float PreviousFixedTime => NextFixedTime - FixedDeltaTime + TimeOffset;
        public static float PresentationTime => Time.time - FixedDeltaTime + TimeOffset;
        public static float PresentationTimeFraction => (PresentationTime - PreviousFixedTime) / (NextFixedTime - PreviousFixedTime);
        public static float TimeOffset = 0.0f;
    }
}
