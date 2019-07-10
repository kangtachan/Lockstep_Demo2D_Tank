//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public Lockstep.ECS.Game.BulletComponent bullet { get { return (Lockstep.ECS.Game.BulletComponent)GetComponent(GameComponentsLookup.Bullet); } }
    public bool hasBullet { get { return HasComponent(GameComponentsLookup.Bullet); } }

    public void AddBullet(bool newCanDestoryIron, bool newCanDestoryGrass, uint newOwnerLocalId) {
        var index = GameComponentsLookup.Bullet;
        var component = (Lockstep.ECS.Game.BulletComponent)CreateComponent(index, typeof(Lockstep.ECS.Game.BulletComponent));
        component.canDestoryIron = newCanDestoryIron;
        component.canDestoryGrass = newCanDestoryGrass;
        component.ownerLocalId = newOwnerLocalId;
        AddComponent(index, component);
    }

    public void ReplaceBullet(bool newCanDestoryIron, bool newCanDestoryGrass, uint newOwnerLocalId) {
        var index = GameComponentsLookup.Bullet;
        var component = (Lockstep.ECS.Game.BulletComponent)CreateComponent(index, typeof(Lockstep.ECS.Game.BulletComponent));
        component.canDestoryIron = newCanDestoryIron;
        component.canDestoryGrass = newCanDestoryGrass;
        component.ownerLocalId = newOwnerLocalId;
        ReplaceComponent(index, component);
    }

    public void RemoveBullet() {
        RemoveComponent(GameComponentsLookup.Bullet);
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

    static Entitas.IMatcher<GameEntity> _matcherBullet;

    public static Entitas.IMatcher<GameEntity> Bullet {
        get {
            if (_matcherBullet == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.Bullet);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherBullet = matcher;
            }

            return _matcherBullet;
        }
    }
}
