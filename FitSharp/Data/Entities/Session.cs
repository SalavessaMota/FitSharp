﻿using System;
using System.ComponentModel.DataAnnotations;

namespace FitSharp.Data.Entities
{
    public class Session : IEntity
    {
        public int Id { get; set; }

        [Display(Name = "Description")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Room")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a room.")]
        public int RoomId { get; set; }

        public virtual Room Room { get; set; }

        [Required]
        [Display(Name = "Class Type")]
        public int ClassTypeId { get; set; }

        public virtual ClassType ClassType { get; set; }

        [Required]
        [Display(Name = "Instructor")]
        public int InstructorId { get; set; }

        public virtual Instructor Instructor { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Informations")]
        public string Informations { get; set; }
    }
}