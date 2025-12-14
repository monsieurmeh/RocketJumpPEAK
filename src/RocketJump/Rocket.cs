using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RocketJump
{
    public class Rocket : MonoBehaviour
    {
        public float blastRadius = 5f;
        public float fallTime = 0.5f;
        public float knockback = 25f;
        public float minFactor = 0.2f;
        public float factorPow = 1f;
        public float itemKnockbackModifier = 1f;
    }
}
