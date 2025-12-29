using Agit.Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.Modules.OHS.DomainServices
{
    public class TanyaOhsChatService : GenericDomainServiceBase<TanyaOhs>
    {
        public TanyaOhsChatService(IUnitOfWork unitOfWork) : base(unitOfWork)
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
