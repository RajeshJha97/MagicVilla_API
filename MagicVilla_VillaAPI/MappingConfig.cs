using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;

namespace MagicVilla_VillaAPI
{
    public class MappingConfig:Profile
    {
        public MappingConfig()
        {
            //CreateMap<Source,destination>

            //below codes are for villa-->VillaDTO mapping
            CreateMap<Villa, VillaDTO>();

            //below codes are for VillaDTO--> mapping
            CreateMap<VillaDTO, Villa>();


            //Below codes will do the bothways mapping
            CreateMap<VillaDTO, VillaCreateDTO>().ReverseMap();
            CreateMap<VillaDTO, VillaUpdateDTO>().ReverseMap();


            CreateMap<Villa, VillaCreateDTO>().ReverseMap();
            CreateMap<Villa, VillaUpdateDTO>().ReverseMap();


        }
    }
}
