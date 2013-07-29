﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Features.Metadata;
using Octopus.Shared.Communications.Stub;
using Octopus.Shared.Diagnostics;
using Pipefish;
using Pipefish.Hosting;
using Pipefish.Persistence;
using Pipefish.Standard;
using Pipefish.Transport;
using Pipefish.WellKnown.Dispatch;
using Module = Autofac.Module;

namespace Octopus.Shared.Communications
{
    public class PipefishModule : Module
    {
        readonly string spaceName;
        readonly Assembly[] assemblies;

        public PipefishModule(string spaceName, params Assembly[] assemblies)
        {
            this.spaceName = spaceName;
            this.assemblies = assemblies.Concat(new[] { typeof(Actor).Assembly }).ToArray();
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var logger = Log.Octopus();
            Pipefish.Diagnostics.Log.OnDebug(logger.Debug);
            Pipefish.Diagnostics.Log.OnError((e, m) => logger.Error(e, m));

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.IsClosedTypeOf(typeof(ICreatedBy<>)))
                .As(t => t
                    .GetInterfaces()
                    .Where(i => i.IsClosedTypeOf(typeof(ICreatedBy<>)))
                    .Select(i => new KeyedService(
                        MessageTypeName.For(i.GetGenericArguments()[0]),
                        typeof(IActor))));

            builder.RegisterType<AutofacActorFactory>().As<IActorFactory>();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.GetCustomAttributes(typeof(WellKnownActorAttribute), false).Any())
                .As<IActor>()
                .WithMetadataFrom<WellKnownActorAttribute>();

            builder.RegisterType<MessageInspectorCollection>()
                .Named<IMessageInspector>("collection");

            builder.Register(c => new ActivitySpace(spaceName, c.Resolve<IMessageStore>(), c.ResolveNamed<IMessageInspector>("collection")))
                .AsSelf()
                .As<IActivitySpace>()
                .OnActivating(e =>
                {
                    // This can probably get baked into a built-in class.

                    var storage = e.Context.Resolve<IActorStorage>();
                    foreach (var actor in e.Context.Resolve<IEnumerable<Meta<IActor>>>())
                    {
                        var actorName = (string)actor.Metadata["Name"];

                        var peristent = actor.Value as IPersistentActor;
                        if (peristent != null)
                        {
                            var state = storage.GetStorageFor(actorName);
                            peristent.AttachStorage(state);
                        }

                        e.Instance.Attach(actorName, actor.Value);
                    }
                })
                .SingleInstance();

            builder.RegisterType<ActivitySpaceStarter>()
                .As<IActivitySpaceStarter>()
                .SingleInstance();

            builder.RegisterModule<StubModule>();
        }
    }
}
