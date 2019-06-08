﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using EnsureThat;
using TReX.Discovery.Code.Domain;
using TReX.Discovery.Shared.Business;
using TReX.Discovery.Shared.Business.Commands;
using TReX.Kernel.Shared;
using TReX.Kernel.Shared.Bus;
using TReX.Kernel.Shared.Domain;

namespace TReX.Discovery.Code.Archeology
{
    public abstract class Archeolog<TLecture> : IArcheolog
        where TLecture : AggregateRoot, ICodeLecture
    {
        protected readonly IWriteRepository<TLecture> writeRepository;
        protected readonly IMessageBus bus;

        protected Archeolog(IWriteRepository<TLecture> writeRepository, IMessageBus bus)
        {
            EnsureArg.IsNotNull(writeRepository);
            EnsureArg.IsNotNull(bus);
            this.writeRepository = writeRepository;
            this.bus = bus;
        }

        public async Task<Result> Study(StudyCommand command)
        {
            var studiesResult = await GetLectures(command.Topic);
            return await studiesResult.OnSuccess(s => PersistStudies(s))
                .OnSuccess(() => PublishStudies(command.DiscoveryId, studiesResult.Value));
        }

        protected abstract Task<Result<IEnumerable<TLecture>>> GetLectures(string topic);

        protected async Task<Result> PersistStudies(IEnumerable<TLecture> studies)
        {
            var storeTasks = studies.Select(s => Extensions.TryAsync(() => this.writeRepository.CreateAsync(s)));
            var storeResults = await Task.WhenAll(storeTasks);

            return Result.Combine(storeResults);
        }

        protected async Task<Result> PublishStudies(string discoveryId, IEnumerable<TLecture> studies)
        {
            var messages = studies.Select(s => new CodeResourceDiscovered(discoveryId, s.ToCodeLecture()));
            return await this.bus.PublishMessages(messages);
        }
    }
}