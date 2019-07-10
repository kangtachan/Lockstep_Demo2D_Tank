//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.EventSystemGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed class LifeEventSystem : Entitas.ReactiveSystem<ActorEntity> {

    readonly System.Collections.Generic.List<ILifeListener> _listenerBuffer;

    public LifeEventSystem(Contexts contexts) : base(contexts.actor) {
        _listenerBuffer = new System.Collections.Generic.List<ILifeListener>();
    }

    protected override Entitas.ICollector<ActorEntity> GetTrigger(Entitas.IContext<ActorEntity> context) {
        return Entitas.CollectorContextExtension.CreateCollector(
            context, Entitas.TriggerOnEventMatcherExtension.Added(ActorMatcher.Life)
        );
    }

    protected override bool Filter(ActorEntity entity) {
        return entity.hasLife && entity.hasLifeListener;
    }

    protected override void Execute(System.Collections.Generic.List<ActorEntity> entities) {
        foreach (var e in entities) {
            var component = e.life;
            _listenerBuffer.Clear();
            _listenerBuffer.AddRange(e.lifeListener.value);
            foreach (var listener in _listenerBuffer) {
                listener.OnLife(e, component.value);
            }
        }
    }
}
