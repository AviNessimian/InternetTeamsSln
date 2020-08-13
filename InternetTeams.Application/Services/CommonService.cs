﻿using InternetTeams.Application.Exceptions;
using InternetTeams.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetTeams.Application.Services
{
    public class CommonService : ICommonService
    {
        private readonly IDomainValueRepository _domainValueRepository;

        public CommonService(IDomainValueRepository domainValueRepository)
        {
            _domainValueRepository = domainValueRepository;
        }

        public async Task<List<string>> GetDomainNames()
        {
            var domains = await _domainValueRepository.GetDomainValueCollectionsNames();
            var listOfDomainNames = domains.ToList();
            return listOfDomainNames;
        }


        public async Task<string> ValidateCollectionsName(string collactionName)
        {
            var collectionsNames = await _domainValueRepository.GetDomainValueCollectionsNames();

            var collacationName = collectionsNames.FirstOrDefault(x => x.ToLower() == collactionName.ToLower());
            if (collacationName == null)
            {
                throw new NotFoundException($"{collactionName} collaction not found");
            }

            return collacationName;
        }

    }
}