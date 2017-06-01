using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Models
{
    public class TalkLinksResolver : IValueResolver<Talk, TalkModel, ICollection<LinkModel>>
    {
        public ICollection<LinkModel> Resolve(Talk source, TalkModel destination, ICollection<LinkModel> destMember, ResolutionContext context)
        {
            return null;
        }
    }
}
