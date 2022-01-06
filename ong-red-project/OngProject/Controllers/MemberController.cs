﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OngProject.Common;
using OngProject.Core.DTOs;
using OngProject.Core.Helper.Pagination;
using OngProject.Core.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OngProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        #region Objects and Constructor

        private readonly IMemberServices _memberServices;

        public MemberController(IMemberServices memberServices)
        {
            _memberServices = memberServices;
        }

        #endregion Objects and Constructor

        #region Documentation

        /// <summary>
        /// Endpoint para listar los miembros.
        /// </summary>
        /// <response code="200">Tarea ejecutada con exito devuelve la lista de miembros.</response>
        /// <response code="404">No se encontraron datos para mostrar.</response>

        #endregion Documentation

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery]int? page)
        {
            try
            {
                if (User.IsInRole("Administrator"))
                    return Ok(await _memberServices.GetAllAsync());
                if (!page.HasValue)
                    return Unauthorized("Usted no esta autorizado.");

                return Ok(await _memberServices.GetAllByPaginationAsync((int)page));
            }
            catch (Exception e)
            {
                return BadRequest(new Result().Fail(e.Message));
            }
        }

        #region Documentation

        /// <summary>
        /// Endpoint para crear un miembro.
        /// </summary>
        /// <response code="200">Tarea ejecutada con exito devuelve un mensaje satisfactorio.</response>
        /// <response code="400">Errores de validacion o excepciones.</response>

        #endregion Documentation

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MemberInsertDTO member)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return Ok(await _memberServices.CreateAsync(member));
                }
                catch (Exception ex)
                {
                    return BadRequest(new Result().Fail($"Ocurrio un error al momento de intentar ingresar los datos - {ex.Message}"));
                }
            }

            return BadRequest(new Result().Fail("Algo salio mal."));
        }

        #region Documentation

        /// <summary>
        /// Endpoint para actualizar un miembro.
        /// </summary>
        /// <response code="200">Tarea ejecutada con exito devuelve un mensaje satisfactorio.</response>
        /// <response code="400">Errores de validacion o excepciones.</response>

        #endregion Documentation

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromForm] MemberUpdateDTO memberUpdate, int id)
        {
            if (id != memberUpdate.Id)
            {
                return BadRequest(new Result().Fail("Los ids deben coincidir."));
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    return Ok(await _memberServices.UpdateAsync(memberUpdate));
                }
                catch (Exception ex)
                {
                    return BadRequest(new Result().Fail($"Ocurrio un error al momento de intentar actulizar los datos - {ex.Message}"));
                }
            }

            return BadRequest(new Result().Fail("Algo salio mal."));
        }
    }
}
