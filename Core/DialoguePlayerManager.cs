// ===== Enhanced Framework - https://github.com/TetsuoYoshima/EnhancedFramework-Dialogues ===== //
// 
// Notes:
//
// ============================================================================================= //

using EnhancedFramework.Core;
using System;
using System.Collections.Generic;

namespace EnhancedFramework.Dialogues {
    // ===== Pool ===== \\

    /// <summary>
    /// Base non-generic class for a <see cref="DialoguePlayer"/> pool.
    /// </summary>
    internal abstract class DialoguePlayerPool {
        #region Pool
        /// <summary>
        /// <see cref="DialoguePlayer"/> type of this pool content.
        /// </summary>
        public abstract Type TargetType { get; }

        // -----------------------

        /// <summary>
        /// Initializes this object pool.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Get a <see cref="DialoguePlayer"/> instance from this pool.
        /// </summary>
        public abstract DialoguePlayer GetPoolInstance();

        /// <summary>
        /// Releases a <see cref="DialoguePlayer"/> instance and send it back to this pool.
        /// </summary>
        public abstract bool ReleasePoolInstance(DialoguePlayer _instance);

        /// <summary>
        /// Clears this pool content.
        /// </summary>
        public abstract void ClearPool(int _capacity = 1);
        #endregion
    }

    /// <summary>
    /// Generic version of <see cref="DialoguePlayerPool"/>, used to manage a target <see cref="DialoguePlayer"/> type pool.
    /// </summary>
    internal sealed class DialoguePlayerPool<T> : DialoguePlayerPool, IObjectPoolManager<T> where T : DialoguePlayer {
        #region Pool
        private readonly ObjectPool<T> pool = new ObjectPool<T>();

        /// <inheritdoc/>
        public override Type TargetType {
            get { return typeof(T); }
        }

        // -----------------------

        /// <inheritdoc/>
        public override void Initialize() {
            pool.Initialize(this);
        }

        /// <inheritdoc/>
        public override DialoguePlayer GetPoolInstance() {
            return pool.GetPoolInstance();
        }

        /// <inheritdoc/>
        public override bool ReleasePoolInstance(DialoguePlayer _instance) {
            return pool.ReleasePoolInstance(_instance as T);
        }

        /// <inheritdoc/>
        public override void ClearPool(int _capacity = 1) {
            pool.ClearPool(_capacity);
        }

        // -------------------------------------------
        // Manager
        // -------------------------------------------

        T IObjectPool<T>.GetPoolInstance() {
            return GetPoolInstance() as T;
        }

        bool IObjectPool<T>.ReleasePoolInstance(T _instance) {
            return ReleasePoolInstance(_instance);
        }

        void IObjectPool.ClearPool(int _capacity) {
            ClearPool(_capacity);
        }

        T IObjectPoolManager<T>.CreateInstance() {
            return Activator.CreateInstance<T>();
        }

        void IObjectPoolManager<T>.DestroyInstance(T _instance) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }
        #endregion
    }

    // ===== Manager ===== \\

    /// <summary>
    /// <see cref="DialoguePlayer"/>-related manager class, especially used for pooling.
    /// </summary>
    public static class DialoguePlayerManager {
        #region Pool
        private static List<DialoguePlayerPool> pools = new List<DialoguePlayerPool>();

        // -------------------------------------------
        // Core
        // -------------------------------------------

        /// <summary>
        /// Get a <see cref="DialoguePlayer"/> from the pool of a given type.
        /// </summary>
        /// <param name="_type"><see cref="DialoguePlayer"/> type to get.</param>
        /// <returns>The matching <see cref="DialoguePlayer"/> from the pool.</returns>
        public static DialoguePlayer GetPoolInstance(Type _type) {
            return GetPool(_type).GetPoolInstance();
        }

        /// <summary>
        /// Releases a given <see cref="DialoguePlayer"/> instance and send it back to the pool.
        /// </summary>
        /// <param name="_instance">The <see cref="DialoguePlayer"/> instance to release.</param>
        /// <returns>True if the instance could be successfully sent back to the pool, false otherwise.</returns>
        public static bool ReleasePoolInstance(DialoguePlayer _instance) {
            return GetPool(_instance.GetType()).ReleasePoolInstance(_instance);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private static DialoguePlayerPool GetPool(Type _type) {
            // From existing pools.
            ref List<DialoguePlayerPool> _pools = ref pools;
            for (int i = _pools.Count; i-- > 0;) {

                DialoguePlayerPool _pool = _pools[i];
                if (_pool.TargetType == _type)
                    return _pool;
            }

            // Create new pool.
            DialoguePlayerPool _newPool = Activator.CreateInstance(typeof(DialoguePlayerPool<>).MakeGenericType(_type)) as DialoguePlayerPool;
            _newPool.Initialize();

            _pools.Add(_newPool);
            return _newPool;
        }
        #endregion
    }
}
