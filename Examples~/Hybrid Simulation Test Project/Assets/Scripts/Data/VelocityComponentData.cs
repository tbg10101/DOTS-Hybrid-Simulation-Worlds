using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Software10101.DOTS.Example.Data {
    [Serializable]
    public struct VelocityComponentData : IComponentData {
        public float3 Value;
    }
}
