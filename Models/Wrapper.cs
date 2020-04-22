using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace The_Wall.Models
{
    public class Wrapper
    {
        public Message Message {get;set;}
        public List<Message> Messages {get;set;}
        public Comment Comment {get;set;}
    }
}