using MagicVilla_VillaAPI.Models.DTO;

namespace MagicVilla_VillaAPI.Data
{
    public static class VillaStore
    {
        public static List<VillaDTO> VillaList = new List<VillaDTO>
        {
            new VillaDTO{ Id=1,Name="Pool Villa",Occupancy=4,Sqrft=200},
            new VillaDTO{ Id=2,Name="Beach Villa",Occupancy=3,Sqrft=600}
        };
    }
}
