using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Models
{
    // This class works with the AutoMapper profile to resolve
    // an url for a particular camp. 
    public class CampUrlResolver : IValueResolver<Camp, CampModel, string> // source, destination, type we're returning
    {
        private IHttpContextAccessor _httpContextAccessor;

        public CampUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Camp source, CampModel destination, string destMember, ResolutionContext context)
        {
            // We don't want to hard code an url string like api/camp/get/{...} so 
            // we have to dynamically generate it by using an "UrlHelper" that represents
            // the particular url from the "CampGet" action in the CampsController.
            var url = (IUrlHelper) _httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            return url.Link("CampGet", new { id = source.Id });
        }
    }
}
