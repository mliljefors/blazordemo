using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazordemo.Data
{
    public class OneTimePassword
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Identifier { get; set; }
        public string TimeStamp { get; set; }
        public int Status { get; set; }
    }
}
