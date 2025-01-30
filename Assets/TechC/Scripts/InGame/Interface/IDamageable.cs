using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Interface
{
    public interface IDamageable
    {
        void TakeDamage(int damage);
        void Death();

    }
}
