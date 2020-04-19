using UnityEngine;

namespace Software10101.DOTS.Utils {
    public static class TimeUtil {
        public static float FixedDeltaTime => Time.fixedDeltaTime;
        public static float NextFixedTime => Time.fixedTime;
        public static float PreviousFixedTime => NextFixedTime - FixedDeltaTime;
        public static float PresentationTime => Time.time - FixedDeltaTime;
        public static float PresentationTimeFraction => (PresentationTime - PreviousFixedTime) / (NextFixedTime - PreviousFixedTime);
    }
}
