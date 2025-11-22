using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.ViewModels
{
    public class BioViewModel
    {
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; }=null!;
        public string LocationUrl { get; set; } = null!;
    }

    public class BioCreateViewModel { }

    public class BioUpdateViewModel { }

  
}
