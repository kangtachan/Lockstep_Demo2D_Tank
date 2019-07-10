//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    static readonly Lockstep.ECS.Game.DestroyedComponent destroyedComponent = new Lockstep.ECS.Game.DestroyedComponent();

    public bool isDestroyed {
        get { return HasComponent(GameComponentsLookup.Destroyed); }
        set {
            if (value != isDestroyed) {
                var index = GameComponentsLookup.Destroyed;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : destroyedComponent;

                    AddComponent(index, component);
                } else {
                    RemoveComponent(index);
                }
            }
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class GameMatcher {

    static Entitas.IMatcher<GameEntity> _matcherDestroyed;

    public static Entitas.IMatcher<GameEntity> Destroyed {
        get {
            if (_matcherDestroyed == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.Destroyed);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherDestroyed = matcher;
            }

            return _matcherDestroyed;
        }
    }
}
