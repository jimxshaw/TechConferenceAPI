﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Models
{
    public class CampModel
    {
        public string Url { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string Moniker { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Length { get; set; }

        [Required]
        [MinLength(25)]
        [MaxLength(1024)]
        public string Description { get; set; }

        // We're using AutoMapper's profile class to map Camp to CampModel. Camp has
        // a Location object which contains the below properties. AutoMapper cleverly knows
        // how to map a Location object to its associated properties when we add that
        // object's name in front of those properties. 
        public string LocationAddress1 { get; set; }
        public string LocationAddress2 { get; set; }
        public string LocationAddress3 { get; set; }
        public string LocationCityTown { get; set; }
        public string LocationStateProvince { get; set; }
        public string LocationPostalCode { get; set; }
        public string LocationCountry { get; set; }
    }
}
