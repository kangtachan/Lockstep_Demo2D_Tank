using Entitas;

namespace Lockstep.Game {
    public interface  IClone {
        IComponent Clone();
    }
}