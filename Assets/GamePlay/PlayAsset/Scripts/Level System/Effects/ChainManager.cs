using System.Collections.Generic;

namespace Watermelon
{
    public static class ChainManager
    {
        private static List<IChainElement> registeredElements;

        public static void Init()
        {
            registeredElements = new List<IChainElement>();
        }

        public static void RegisterElement(IChainElement element)
        {
            registeredElements.Add(element);
        }

        public static void UnregisterElement(IChainElement element)
        {
            registeredElements.Remove(element);
        }

        public static IChainElement GetChainElement()
        {
            IChainElement result = null;
            int minKeysLeft = int.MaxValue;

            foreach (IChainElement element in registeredElements)
            {
                if (element.KeysLeft > 0 && element.KeysLeft < minKeysLeft)
                {
                    minKeysLeft = element.KeysLeft;
                    result = element;
                }
            }

            return result;
        }
    }
}
