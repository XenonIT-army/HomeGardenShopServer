using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeGardenShopDAL.Entities
{
    [Table("Category")]
    public partial class CategoryDB
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string Name { get; set; }
        [Required]
        [StringLength(40)]
        public string NameUA { get; set; }
        [Required]
        [StringLength(40)]
        public string NameEN { get; set; }
    }
}
