using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Software10101.DOTS.Example.Data {
    [Serializable]
    public struct PositionComponentData : IComponentData {
        public float3 PreviousValue;
        public float3 PresentationValue;
        public float3 NextValue;
    }
}
