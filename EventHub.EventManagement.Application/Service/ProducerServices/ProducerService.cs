using AutoMapper;
using EventHub.EventManagement.Application.Contracts.Infrastructure;
using EventHub.EventManagement.Application.Contracts.links;
using EventHub.EventManagement.Application.Contracts.Persistance;
using EventHub.EventManagement.Application.Contracts.Service.ProducerServices;
using EventHub.EventManagement.Application.DTOs.ProducerDto;
using EventHub.EventManagement.Application.Exceptions;
using EventHub.EventManagement.Application.Models.LinkModels;
using EventHub.EventManagement.Application.RequestFeatures.Paging;
using EventHub.EventManagement.Domain.Entities.ProducerEntities;

namespace EventHub.EventManagement.Application.Service.ProducerServices
{
   internal sealed class ProducerService : IProducerService
   {
      private readonly IRepositoryManager _repository;
      private readonly ILoggerManager _logger;
      private readonly IMapper _mapper;
      private readonly IEntitiesLinkGeneratorManager _entitiesLinkGenerator;

      public ProducerService
         (IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IEntitiesLinkGeneratorManager entitiesLinkGenerator)
      {
         _repository = repository ?? throw new ArgumentNullException(nameof(repository));
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
         _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
         _entitiesLinkGenerator = entitiesLinkGenerator ?? throw new ArgumentNullException(nameof(entitiesLinkGenerator));
      }

      public async Task<ProducerDto> CreateProducerAsync(ProducerForCreationDto producer)
      {
         var producerEntity = _mapper
            .Map<Producer>(producer);

         _repository
             .ProducerRepository
             .CreateProducer(producerEntity);

         await _repository.SaveAsync();

         var producerToReturn = _mapper
            .Map<ProducerDto>(producerEntity);

         return producerToReturn;
      }

      public async Task<LinkResponse> GetProducerAsync(Guid producerId, ProducerLinkParams linkParams, bool trackChanges)
      {
         var producer =
            await GetProducerAndCheckIfItExists(producerId, trackChanges);

         var producerDto = _mapper
            .Map<ProducerDto>(producer);

         var linkResponse = _entitiesLinkGenerator.ProducerLinks
            .TryGetEntityLinks(producerDto, linkParams.producerParams.Fields!, linkParams.HttpContext);

         return linkResponse;
      }

      public async Task<(LinkResponse link, MetaData metaData)>
         GetAllProducersAsync(ProducerLinkParams producerLinkParams, bool trackChanges)
      {
         var producersWithMetaData = await _repository
            .ProducerRepository
            .GetAllProducersAsync(producerLinkParams.producerParams, trackChanges);

         var producersDto = _mapper
            .Map<IEnumerable<ProducerDto>>(producersWithMetaData);

         var linkedProducers = _entitiesLinkGenerator.ProducerLinks.TryGetEntitiesLinks
            (producersDto, producerLinkParams.producerParams.Fields!, producerLinkParams.HttpContext);

         return (link: linkedProducers, metaData: producersWithMetaData.MetaData);
      }

      public async Task RemoveProducerAsync(Guid producerId, bool trackChanges)
      {
         var producer =
            await GetProducerAndCheckIfItExists(producerId, trackChanges);

         _repository
            .ProducerRepository
            .RemoveProducer(producer);

         await _repository.SaveAsync();
      }


      public async Task UpdateProducerAsync(Guid producerId, ProducerForUpdateDto producerForUpdate,
         bool trackChanges)
      {
         var producer =
            await GetProducerAndCheckIfItExists(producerId, trackChanges);

         _mapper.Map(producerForUpdate, producer);

         await _repository.SaveAsync();

      }

      private async Task<Producer> GetProducerAndCheckIfItExists(Guid producerId, bool trackChanges)
      {
         var producer = await _repository
            .ProducerRepository
            .GetProducerAsync(producerId, trackChanges);

         if (producer is null)
            throw new ProducerNotFound("id", producerId);

         return producer;
      }
   }
}
