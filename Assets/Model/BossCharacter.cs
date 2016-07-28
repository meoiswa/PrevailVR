using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Prevail.Model
{
    public class BossCharacter : Character
    {

        public float TeleportCooldown { get; private set; }


        public BossCharacter() : base(1000)
        {
        }
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Bullet")
            {
                Damage(10f);
            }
        }
    }
}
