using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tychaia.RuntimeGeneration.Weapons.Modifiers
{
    // Spell power per hit (goes away over time, stays for X seconds)
    // AKA: 5 sp per hit for 10 seconds, if you hit 10 times in 5 seconds then cast spells you'll have 5 seconds with 10 stacks, then lose them gradually over the next 5 seconds.
    [Rarity(0.25)]
    public class EmpoweringModifier : WeaponModifier
    {
        public override string ToString()
        {
            return "Empowering";
        }
    }
}
