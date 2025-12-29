using TAMHR.ESS.Domain;
using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class MenuViewModel
    {
        public string Category { get; }
        public int OrderIndex { get; }
        public bool FavouriteCategory { get; }
        public IEnumerable<IMenu> Menus { get; }

        public MenuViewModel(string category, int orderIndex, IEnumerable<IMenu> menus, bool favouriteCategory = false)
        {
            Category = category;
            OrderIndex = orderIndex;
            Menus = menus;
            FavouriteCategory = favouriteCategory;
        }
    }
}
