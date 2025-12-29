using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_FAVOURITE_MENU")]
    public partial class FavouriteMenu : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        public Guid MenuId { get; set; }

        public int MenuOrder { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool RowStatus { get; set; }

        public Menu Menu { get; set; }

        public static FavouriteMenu Create(string noreg, Guid menuId, int menuOrder)
        {
            var favouriteMenu = new FavouriteMenu();

            favouriteMenu.NoReg = noreg;
            favouriteMenu.MenuId = menuId;
            favouriteMenu.MenuOrder = menuOrder;

            return favouriteMenu;
        }
    }
}
