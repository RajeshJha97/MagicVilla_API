using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaNumberAPI")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        private readonly IVillaNumberRepository _dbVillaNumber;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber, IMapper mapper)
        {
            _dbVillaNumber = dbVillaNumber;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try 
            {
                IEnumerable<VillaNumber> villaNumbers = await _dbVillaNumber.GetAllAsync();
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumbers);
                return Ok(_response);
            }
            catch (Exception ex) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessage=new List<string> { ex.Message.ToString()};
                return BadRequest(_response);   
            }
        }

        [HttpGet("{villaNo:Int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int villaNo)
        {
            try 
            {
                if (villaNo == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage = new List<string> { $"You are searching with id: {villaNo}" };
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                VillaNumber villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == villaNo, false);
                if (villaNumber == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage = new List<string> { $"Not available with id: {villaNo}" };
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessage = new List<string> { ex.Message.ToString()};
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {
            try
            {
                if (createDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess=false;
                    _response.ErrorMessage = new List<string> { $"Data is null" };
                    return BadRequest(_response);

                }
                if (_dbVillaNumber.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessage = new List<string> { $"villa number :{createDTO.VillaNo} already exist" };
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                var villaNumber = _mapper.Map<VillaNumber>(createDTO);
               
                await _dbVillaNumber.CreateAsync(villaNumber);
                
                return CreatedAtRoute("GetVillaNumber", new { villaNo = villaNumber.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessage = new List<string> { ex.Message.ToString() };
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
        }

        [HttpDelete("{villaNumber:Int}", Name="DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int villaNumber)
        {
            try 
            {
                if (villaNumber == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess=false;
                    _response.ErrorMessage = new List<string> { "Villa Number: 0" };
                    return BadRequest(_response);
                }
                VillaNumber villa =await _dbVillaNumber.GetAsync(u => u.VillaNo == villaNumber, false);
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessage = new List<string> { $"Villa Number is null with {villaNumber}" };
                    return BadRequest(_response);
                }
               await _dbVillaNumber.RemoveAsync(villa);
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<VillaNumberDTO>(villa);
                return Ok(_response);
            }
            catch (Exception ex) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { ex.Message.ToString() };
                return BadRequest(_response);
            }
        }

        [HttpPut("villaNumber:Int",Name ="UpdateVillaNumber")]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int villaNumber,[FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (villaNumber == 0 || updateDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessage = new List<string> { "villa number and Request body can't be null" };
                    return BadRequest(_response);
                }
                if (villaNumber != updateDTO.VillaNo)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessage = new List<string> { "Id mismatch of request body and parameter" };
                    return BadRequest(_response);
                }
                
               await _dbVillaNumber.UpdateAsync(_mapper.Map<VillaNumber>(updateDTO));
               _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<VillaNumberDTO>(updateDTO);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> {ex.Message.ToString() };
                return BadRequest(_response);
            }
        }
    }
}
