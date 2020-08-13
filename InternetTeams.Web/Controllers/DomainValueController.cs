﻿using InternetTeams.Application.Interfaces;
using InternetTeams.Application.Models;
using InternetTeams.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternetTeams.Web.Controllers
{
    public class DomainValuesController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<DomainValue>>> Get(
            [FromServices] IGetAllDomainValuesInteractor Interactor,
            [FromQuery] GetAllDomainValuesRequest request) => Ok(await Interactor.Handle(request));


        [HttpGet("ByName")]
        public async Task<ActionResult<List<DomainValue>>> Get(
            [FromServices] IGetDomainValuesByNameInteractor Interactor,
            [FromQuery] GetDomainValuesByNameRequest request) => Ok(await Interactor.Handle(request));


        [HttpGet("DomainNames")]
        public async Task<ActionResult<List<string>>> Get(
              [FromServices] ICommonService commonService) => Ok(await commonService.GetDomainNames());


        [HttpGet("TimepointsAverage")]
        public async Task<IActionResult> Get(
            [FromServices] ICalculateTimepointsAverageInteractor Interactor,
            [FromQuery] CalculateTimepointsAverageRequest request) => Ok(await Interactor.Handle(request));

    }
}