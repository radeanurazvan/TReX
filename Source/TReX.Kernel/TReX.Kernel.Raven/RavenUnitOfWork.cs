﻿using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using EnsureThat;
using Raven.Client.Documents.Session;
using TReX.Kernel.Shared;
using TReX.Kernel.Shared.Bus;
using TReX.Kernel.Shared.Domain;

namespace TReX.Kernel.Raven
{
    public sealed class RavenUnitOfWork : IUnitOfWork
    {
        private readonly IAsyncDocumentSession session;
        private readonly IMessageBus bus;
        private readonly ILogger logger;
        private readonly AggregateTracker tracker;

        public RavenUnitOfWork(IAsyncDocumentSession session, IMessageBus bus, ILogger logger, AggregateTracker tracker)
        {
            EnsureArg.IsNotNull(session);
            EnsureArg.IsNotNull(bus);
            EnsureArg.IsNotNull(logger);
            EnsureArg.IsNotNull(tracker);
            this.session = session;
            this.bus = bus;
            this.logger = logger;
            this.tracker = tracker;
        }

        public async Task<Result> CommitAsync()
        {
            var events = this.tracker.DumpEvents();
            return await Extensions.TryAsync(() => this.session.SaveChangesAsync())
                .OnSuccess(() => this.bus.PublishMessages(events))
                .OnFailure(e => this.logger.Log(e));
        }
    }
}