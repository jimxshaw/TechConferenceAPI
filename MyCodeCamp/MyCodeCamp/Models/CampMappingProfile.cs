using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
            CreateMap<Camp, CampModel>()
                .ForMember(c => c.StartDate, options => options.MapFrom(camp => camp.EventDate)) // The StartDate is the EventDate.
                .ForMember(c => c.EndDate, options => options.ResolveUsing(camp => camp.EventDate.AddDays(camp.Length - 1))) // The EndDate is calculated as however many days are in the event - 1 day.
                .ForMember(c => c.Url, options => options.ResolveUsing((camp, model, unused, resolutionContext) =>
                {
                    // We don't want to hard code an url string like api/camp/get/{...} so 
                    // we have to dynamically generate it by using an "UrlHelper" that represents
                    // the particular url from the "CampGet" action in the CampsController.
                    var url = (IUrlHelper)resolutionContext.Items["UrlHelper"]; 
                    return url.Link("CampGet", new { id = camp.Id });
                }));
        }
    }
}
