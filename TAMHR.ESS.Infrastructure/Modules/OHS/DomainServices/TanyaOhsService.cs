using Agit.Domain;
using Agit.Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.Modules.OHS.DomainServices
{
    public class TanyaOhsService : GenericDomainServiceBase<TanyaOhs>
    {
        public TanyaOhsService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        protected override string[] Properties => new[] {
            "Nama",
            "Noreg",
            "Keluhan",
            "Tanggal",
            "Solve",
            "Rating",
            "Feedback",
            "Reply_Feedback",
        };
    }
}
