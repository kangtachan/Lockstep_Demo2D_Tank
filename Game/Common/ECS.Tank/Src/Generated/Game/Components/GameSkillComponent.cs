//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public Lockstep.ECS.Game.SkillComponent skill { get { return (Lockstep.ECS.Game.SkillComponent)GetComponent(GameComponentsLookup.Skill); } }
    public bool hasSkill { get { return HasComponent(GameComponentsLookup.Skill); } }

    public void AddSkill(Lockstep.Math.LFloat newCd, Lockstep.Math.LFloat newCdTimer, int newBulletId, bool newIsNeedFire) {
        var index = GameComponentsLookup.Skill;
        var component = (Lockstep.ECS.Game.SkillComponent)CreateComponent(index, typeof(Lockstep.ECS.Game.SkillComponent));
        component.cd = newCd;
        component.cdTimer = newCdTimer;
        component.bulletId = newBulletId;
        component.isNeedFire = newIsNeedFire;
        AddComponent(index, component);
    }

    public void ReplaceSkill(Lockstep.Math.LFloat newCd, Lockstep.Math.LFloat newCdTimer, int newBulletId, bool newIsNeedFire) {
        var index = GameComponentsLookup.Skill;
        var component = (Lockstep.ECS.Game.SkillComponent)CreateComponent(index, typeof(Lockstep.ECS.Game.SkillComponent));
        component.cd = newCd;
        component.cdTimer = newCdTimer;
        component.bulletId = newBulletId;
        component.isNeedFire = newIsNeedFire;
        ReplaceComponent(index, component);
    }

    public void RemoveSkill() {
        RemoveComponent(GameComponentsLookup.Skill);
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

    static Entitas.IMatcher<GameEntity> _matcherSkill;

    public static Entitas.IMatcher<GameEntity> Skill {
        get {
            if (_matcherSkill == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.Skill);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherSkill = matcher;
            }

            return _matcherSkill;
        }
    }
}
