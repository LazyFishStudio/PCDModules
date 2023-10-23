using UnityEngine;

namespace InteractSystem
{
    public interface IAttackable
    {
        /// <summary>
        /// melee, ranged, bullet, punch and so on.
        /// </summary>
        public void AttackedBy(GameObject attacker, GameObject weapon);
    }
}
