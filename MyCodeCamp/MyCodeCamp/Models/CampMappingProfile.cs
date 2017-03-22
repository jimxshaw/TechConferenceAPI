using AutoMapper;
using MyCodeCamp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Models
{
    // In addition to registering AutoMapper as a service in Startup, we must also define an 
    // AutoMapper profile in our project that shows how one type connects to another type.
    public class CampMappingProfile : Profile
    {
        public CampMappingProfile()
        {
            CreateMap<Camp, CampModel>();
        }
    }
}
