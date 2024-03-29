//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public Lockstep.ECS.Game.DelayCallComponent delayCall { get { return (Lockstep.ECS.Game.DelayCallComponent)GetComponent(GameComponentsLookup.DelayCall); } }
    public bool hasDelayCall { get { return HasComponent(GameComponentsLookup.DelayCall); } }

    public void AddDelayCall(Lockstep.Math.LFloat newDelayTimer, System.Action newCallBack) {
        var index = GameComponentsLookup.DelayCall;
        var component = (Lockstep.ECS.Game.DelayCallComponent)CreateComponent(index, typeof(Lockstep.ECS.Game.DelayCallComponent));
        component.delayTimer = newDelayTimer;
        component.callBack = newCallBack;
        AddComponent(index, component);
    }

    public void ReplaceDelayCall(Lockstep.Math.LFloat newDelayTimer, System.Action newCallBack) {
        var index = GameComponentsLookup.DelayCall;
        var component = (Lockstep.ECS.Game.DelayCallComponent)CreateComponent(index, typeof(Lockstep.ECS.Game.DelayCallComponent));
        component.delayTimer = newDelayTimer;
        component.callBack = newCallBack;
        ReplaceComponent(index, component);
    }

    public void RemoveDelayCall() {
        RemoveComponent(GameComponentsLookup.DelayCall);
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

    static Entitas.IMatcher<GameEntity> _matcherDelayCall;

    public static Entitas.IMatcher<GameEntity> DelayCall {
        get {
            if (_matcherDelayCall == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.DelayCall);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherDelayCall = matcher;
            }

            return _matcherDelayCall;
        }
    }
}
