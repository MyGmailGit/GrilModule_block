using UnityEngine;

namespace Watermelon
{
    public interface IChainElement
    {
        public int KeysLeft { get; }
        public Transform Transform { get; }
        public ChainVisualsBehavior ChainVisualsBehavior { get; }

        void OnKeyLinked();
        void OnKeyReached();
    }
}
