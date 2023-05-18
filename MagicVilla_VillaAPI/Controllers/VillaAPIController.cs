
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {

        private readonly ILogger<VillaAPIController> _logger;
        //private readonly ApplicationDbContext _db;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        public VillaAPIController(ILogger<VillaAPIController> logger, IVillaRepository dbVilla, IMapper mapper)
        {
            _logger = logger;
            _dbVilla = dbVilla;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            _logger.LogInformation("Retrieve all the Villas");
            try 
            {
                IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
                return Ok(_response);
            }
            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessage=new List<string> { ex.Message.ToString() };
                return Ok(_response);
            }
            
        }


        [HttpGet("{id:int}", Name = "GetVilla")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError($"Villa is not available with Id: {id}");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess=false;
                    return NotFound(_response);
                }
                _logger.LogInformation($"Villa: {villa.Name}");
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<VillaDTO>(villa);
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.ErrorMessage = new List<string> { ex.Message.ToString() };
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return Ok(_response);
            }
            
        }



        [HttpPost]
        [Authorize(Roles ="Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    //ModelState.AddModelError("CustomError", "Villa already exist");
                    _response.ErrorMessage = new List<string> { $"Villa {createDTO.Name} already exist" };
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    //return BadRequest(ModelState);
                    return BadRequest(_response);
                }
                if (createDTO == null)
                {
                    _response.ErrorMessage = new List<string> { "You are paasing a blank data object" };
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                Villa model = _mapper.Map<Villa>(createDTO);

                //Villa model = new Villa()
                //{
                //    Amenity = createDTO.Amenity,
                //    Details = createDTO.Details,
                //    ImageUrl = createDTO.ImageUrl,
                //    Name = createDTO.Name,
                //    Occupancy = createDTO.Occupancy,
                //    Rate = createDTO.Rate,
                //    Sqft = createDTO.Sqft
                //};
                //villa.Id = VillaStore.VillaList.OrderByDescending(u=>u.Id).FirstOrDefault().Id+1;
                await _dbVilla.CreateAsync(model);

                _response.Result = model;
                _response.StatusCode = HttpStatusCode.Created;
                //return Ok(villa);
                //In order to pass that where the data has been created

                return CreatedAtRoute("GetVilla", new { id = model.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.ErrorMessage = new List<string> { ex.Message.ToString()};
                _response.StatusCode=HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            
        }


        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [Authorize(Roles ="CUSTOM")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                Villa villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }

                await _dbVilla.RemoveAsync(villa);

                _response.Result= villa;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.ErrorMessage = new List<string> { ex.Message.ToString()};
                _response.StatusCode =HttpStatusCode.BadRequest;   
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            
        }


        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
                    _response.IsSuccess= false;
                    _response.StatusCode=HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                //VillaDTO villa=VillaStore.VillaList.FirstOrDefault(u=> u.Id==id);
                //villa.Name = villaDTO.Name;
                //villa.Occupancy= villaDTO.Occupancy;
                //villa.Sqrft= villaDTO.Sqrft;

                var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);

                if (villa == null)
                {
                    _logger.LogError("Error", $"Data is not available with Id: {id}");
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessage = new List<string> { $"Data is not available with Id: {id}" };
                    _response.IsSuccess = false;
                    return NotFound(_response);
                }


                Villa model = _mapper.Map<Villa>(updateDTO);

                //Villa model = new Villa()
                //{
                //    Id = updateDTO.Id,
                //    Name = updateDTO.Name,
                //    Amenity = updateDTO.Amenity,
                //    Details = updateDTO.Details,
                //    ImageUrl = updateDTO.ImageUrl,
                //    Occupancy = updateDTO.Occupancy,
                //    Rate = updateDTO.Rate,
                //    Sqft = updateDTO.Sqft,
                //    UpdatedDate = DateTime.Now
                //};

                await _dbVilla.UpdateAsync(model);
                _response.Result = model;
                _response.StatusCode=HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.ErrorMessage = new List<string> { ex.Message.ToString()};
                _response.StatusCode=HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
           
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patch)
        {
            if (id == 0 || patch == null)
            {
                return BadRequest();
            }
            Villa villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);

            if (villa == null)
            {
                return NotFound();
            }

            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

            //VillaUpdateDTO villaDTO = new VillaUpdateDTO()
            //{
            //    Id = villa.Id,
            //    Amenity = villa.Amenity,
            //    Details = villa.Details,
            //    ImageUrl = villa.ImageUrl,
            //    Name = villa.Name,
            //    Occupancy = villa.Occupancy,
            //    Rate = villa.Rate,
            //    Sqft = villa.Sqft
            //};
            patch.ApplyTo(villaDTO, ModelState);

            Villa model = _mapper.Map<Villa>(villaDTO);
            //Villa model = new Villa()
            //{
            //    Id = villaDTO.Id,
            //    Name = villaDTO.Name,
            //    Amenity = villaDTO.Amenity,
            //    Details = villaDTO.Details,
            //    ImageUrl = villaDTO.ImageUrl,
            //    Occupancy = villaDTO.Occupancy,
            //    Rate = villaDTO.Rate,
            //    Sqft = villaDTO.Sqft,
            //    UpdatedDate = DateTime.Now
            //};

            await _dbVilla.UpdateAsync(model);


            /* format to consume from POSTMAN
             [
              {
                "path": "/name",
                "op": "replace",               
                "value": "test villa"
              }
            ]
             */

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent();
        }

    }
}
