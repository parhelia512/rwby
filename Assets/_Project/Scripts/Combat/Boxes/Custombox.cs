using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class Custombox : Fusion.Hitbox
    {
        public NetworkObject ownerNetworkObject;
        public HurtboxGroup hurtboxGroup;
    }
}