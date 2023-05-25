using UnityEngine;

namespace Assets.Mod_Test
{
    internal class Test:MonoBehaviour
    {
        public void A()
        {
            TestStatic.hps += 10;
            Debug.Log($"Hps:{TestStatic.hps}");
        }

        public void B()
        {
            TestStatic.hps -= 10;
            Debug.Log($"Hps:{TestStatic.hps}");
        }
    }
}
