using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Software10101.DOTS.Example.Data {
    [Serializable]
    public struct RotationComponentData : IComponentData {
        public quaternion PreviousValue;
        public quaternion PresentationValue;
        public quaternion NextValue;
    }
}
