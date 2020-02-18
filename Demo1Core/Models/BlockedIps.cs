using System;
using System.Collections.Generic;

namespace Demo1.Models
{
    public partial class BlockedIps
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public DateTime Date { get; set; }
    }
}
