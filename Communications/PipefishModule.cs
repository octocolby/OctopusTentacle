﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Features.Metadata;
using NuGet;
using Octopus.Platform.Deployment;
using Octopus.Platform.Deployment.Configuration;
using Octopus.Platform.Diagnostics;
using Octopus.Platform.Util;
using Octopus.Shared.Communications.Encryption;
using Octopus.Shared.Communications.Integration;
using Octopus.Shared.FileTransfer;
using Pipefish;
using Pipefish.Core;
using Pipefish.Hosting;
using Pipefish.Persistence;
using Pipefish.Persistence.Filesystem;
using Pipefish.Standard;
using Pipefish.Transport;
using Pipefish.Transport.Filesystem;
using Pipefish.Util.Storage;
using Pipefish.WellKnown.Dispatch;
using Module = Autofac.Module;

namespace Octopus.Shared.Communications
{
    public class InMemoryMessageStore : IMessageStore
    {
        readonly ConcurrentDictionary<string, IMessageStoreSubscription> queues = new ConcurrentDictionary<string, IMessageStoreSubscription>();

        public void Subscribe(IMessageStoreSubscription subscription)
        {
            if (!queues.TryAdd(subscription.Name, subscription))
                throw new InvalidOperationException("Space already subscribed");
        }

        public bool Unsubscribe(string space)
        {
            IMessageStoreSubscription queue;
            return queues.TryRemove(space, out queue);
        }

        public bool TryRevoke(Guid messageId, out Message revokedMessage)
        {
            foreach (var messageStoreSubscription in queues.Values)
            {
                if (messageStoreSubscription.TryRevoke(messageId, out revokedMessage))
                    return true;
            }

            revokedMessage = null;
            return false;
        }

        public void ConfirmAccepted(Message message)
        {
            // Do nothing, we don't support persistent messages
        }

        public void Store(Message message)
        {
            IMessageStoreSubscription subscribedQueue;
            if (!queues.TryGetValue(message.To.Space, out subscribedQueue))
            {
                return;
            }

            subscribedQueue.Accept(message);
        }
    }

    public class PipefishModule : Module
    {
        readonly Assembly[] assemblies;

        public PipefishModule(params Assembly[] assemblies)
        {
            this.assemblies = assemblies.Concat(new[] { typeof(Actor).Assembly, typeof(FileSender).Assembly }).ToArray();
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var logger = Log.Octopus();
            Pipefish.Diagnostics.Log.OnDebug(logger.Trace);
            Pipefish.Diagnostics.Log.OnError((e, m) => logger.Error(e.UnpackFromContainers(), m));

            builder.RegisterAssemblyTypes(assemblies)
                .Where(IsCreatedByMessage)
                .As(t => t
                    .GetInterfaces()
                    .Where(IsCreatedByMessage)
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

            builder.RegisterAssemblyTypes(assemblies)
                .As<IAspect>()
                .As(t =>
                {
                    var implements = t.GetCustomAttribute<AspectImplementsAttribute>();
                    if (implements == null)
                        return new Type[0];

                    return implements.ImplementedTypes;
                })
                .AsSelf()
                .InstancePerDependency();

            builder.Register(c => new ActivitySpace(c.Resolve<ICommunicationsConfiguration>().Squid, c.Resolve<IMessageStore>(), c.ResolveNamed<IMessageInspector>("collection")))
                .AsSelf()
                .As<IActivitySpace>()
                .OnActivating(e =>
                {
                    // This can probably get baked into a built-in class.

                    Log.Octopus().VerboseFormat("Resolving activity space infrastructure for {0}", e.Instance.Name);

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

                        e.Instance.Attach(actorName, actor.Value, () => {});
                    }
                })
                .SingleInstance();

            builder.RegisterType<ActivitySpaceStarter>()
                .As<IActivitySpaceStarter>()
                .SingleInstance();

            builder.Register(c => new InMemoryMessageStore())
                .As<IMessageStore>()
                .SingleInstance();

            builder.Register(c => new DirectoryActorStorage(
                    c.Resolve<ICommunicationsConfiguration>().ActorStateDirectory,
                    c.Resolve<IStorageStreamTransform>()))
                .As<IActorStorage>()
                .SingleInstance();

            builder.RegisterType<EncryptedStorageStream>().As<IStorageStreamTransform>();

            builder.RegisterType<ShutdownToken>().SingleInstance();
        }

        static bool IsCreatedByMessage(Type t)
        {
            return t.IsClosedTypeOf(typeof(ICreatedBy<>)) || t.IsClosedTypeOf(typeof(ICreatedByAsync<>));
        }
    }
}
