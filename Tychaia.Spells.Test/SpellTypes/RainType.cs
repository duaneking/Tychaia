﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tychaia.RuntimeGeneration.Spells.Types
{
    public class RainType : SpellType
    {
        public const double Weight = 0.1;

        public override string ToString()
        {
            return "Rain";
        }
    }
}
