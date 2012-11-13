﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tychaia.RuntimeGeneration.Elements
{
    public class ForceElement : Element
    {
        public const double Weight = 0.5;

        public override string[] PresentTense
        {
            get
            {
                return new string[] { "Battering", "Bleeding", "Shredding", "Hemmorhaging" };
            }
        }

        public override string[] ItemPrefix
        {
            get
            {
                return new string[] { "Protective", "Padded", "Shielded", "Defended", "Defensive", "Protected" };
            }
        }

        public override string[] ElementName
        {
            get
            {
                return new string[] { "Force", "Physical", "Steel", "Arcane" };
            }
        }

        public override string ToString()
        {
            return "Force";
        }
    }
}