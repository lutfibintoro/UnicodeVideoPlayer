using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.LocalDb
{
    public class FrameModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Frame { get; set; } = null!;
    }
}
