using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Software10101.DOTS.Data {
    // Inspired by the https://github.com/maraudical/StatusEffectsFramework-Unity implementation:
    // https://github.com/maraudical/StatusEffectsFramework-Unity/blob/af14bc247dc08bac31a7955bd4e3635a61993025/Entities/Runtime/Scripts/Classes/StatusEffectSystemGroup.cs#L23
    // Usage:
    // var commandBufferParallel = SystemAPI.GetSingleton<EntityCommandBufferSystemSingleton<NameEntityCommandBufferSystem>>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
    // ReSharper disable once UnusedTypeParameter
    // the '_' type isn't used but it is needed to differentiate the types for each ECB system
    public unsafe struct EntityCommandBufferSystemSingleton<_> : IComponentData, IECBSingleton where _ : EntityCommandBufferSystem
    {
        private UnsafeList<EntityCommandBuffer>* _pendingBuffers;
        private AllocatorManager.AllocatorHandle _allocator;

        public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
        {
            return EntityCommandBufferSystem.CreateCommandBuffer(ref *_pendingBuffers, _allocator, world);
        }

        public void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
        {
            _pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf(ref buffers);
        }

        public void SetAllocator(Allocator allocatorIn)
        {
            _allocator = allocatorIn;
        }

        public void SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
        {
            _allocator = allocatorIn;
        }
    }
}
