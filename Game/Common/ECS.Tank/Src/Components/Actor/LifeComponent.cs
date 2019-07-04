﻿using Lockstep.Math;
using Entitas;
using Entitas.CodeGeneration.Attributes;


namespace Lockstep.ECS.Game
{
    [Actor,Event(EventTarget.Self)]
    public partial class LifeComponent : IComponent {
        public int value;
    }
}