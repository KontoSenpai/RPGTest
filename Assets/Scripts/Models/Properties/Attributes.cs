﻿
using RPGTest.Enums;

namespace RPGTest.Models
{
    public class Attributes
    {
        public int MaxHP { get; set; }

        public int MaxMP { get; set; }

        public int MaxStamina { get; set; }

        public int Attack { get; set; }

        public int Magic { get; set; }

        public int Defense { get; set; }

        public int Resistance { get; set; }

        public int Speed { get; set; }

        public int Accuracy { get; set; }

        public int Hit { get; set; } = 1;
    }

    public class Buff
    {
        public string Id { get; set; }

        public int Duration { get; set; }

        public RemovalType RemovalType { get; set; }
    }

    public class Status
    {
        public StatusEffect StatusEffect { get; set; }

        public int Duration { get; set; }

        public RemovalType RemovalType { get; set; }
    }
}
