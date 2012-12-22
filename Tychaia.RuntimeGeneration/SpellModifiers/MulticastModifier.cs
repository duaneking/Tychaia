﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tychaia.RuntimeGeneration.Spells.Modifiers
{
    public class MulticastModifier : SpellModifier
    {
        public override double Rarity = 0.001;

        public override string ToString()
        {
            return "Multicasting";
        }
    }
}
