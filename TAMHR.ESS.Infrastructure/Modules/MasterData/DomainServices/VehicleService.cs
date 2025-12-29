using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle vehicle master data.
    /// </summary>
    public class VehicleService : GenericDomainServiceBase<Vehicle>
    {
        #region Domain Repositories
        /// <summary>
        /// Vehicle matrix repository object.
        /// </summary>
        protected IRepository<VehicleMatrix> VehicleMatrixRepository => UnitOfWork.GetRepository<VehicleMatrix>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated.
        /// </summary>
        protected override string[] Properties => new[] {
            "TypeName",
            "Type",
            "ModelCode",
            "COP",
            "CPP",
            "SCP",
            "Suffix",
            "FinalPrice",
            "Colors"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public VehicleService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public void SaveMatrix(Vehicle vehicle)
        {
            UnitOfWork.Transact(trans =>
            {
                VehicleMatrixRepository.Fetch().Where(x => x.VehicleId == vehicle.Id).Delete();

                if (vehicle.Matrix != null && vehicle.Matrix.Length > 0 && vehicle.IsUpgrade != null )
                {
                    vehicle.Matrix.ForEach(x =>
                    {
                        var splitter = x.Split('|');
                        var employeeSubgroup = splitter[0];
                        var employeeSubgroupText = splitter[1];
                        vehicle.IsUpgrade.ForEach(y =>
                        {
                            var splitter1 = y.Split('|');
                            var employeeSubgroup1 = splitter1[0];
                            var employeeSubgroupText1 = splitter1[1];
                            /*var IsUpgrade1 = splitter1[2];*/
                            if (employeeSubgroup == employeeSubgroup1 && employeeSubgroupText == employeeSubgroupText1)
                            {
                                VehicleMatrixRepository.Add(new VehicleMatrix
                                {
                                    VehicleId = vehicle.Id,
                                    SequenceClass = employeeSubgroup1,
                                    GroupSequence = 0,
                                    Class = employeeSubgroupText1,
                                    IsUpgrade = true
                                });
                                UnitOfWork.SaveChanges();
                            }
                        });
                        string exists = VehicleMatrixRepository.Fetch().AsNoTracking().Any(z => z.VehicleId == vehicle.Id && z.Class == employeeSubgroupText) ? "true" : "false" ;
                        if (exists == "false")
                        {
                            VehicleMatrixRepository.Add(new VehicleMatrix
                            {
                                VehicleId = vehicle.Id,
                                SequenceClass = employeeSubgroup,
                                GroupSequence = 0,
                                Class = employeeSubgroupText,
                                IsUpgrade = false
                           
                            });
                            UnitOfWork.SaveChanges();
                        }
                        
                    });

                }
                else if (vehicle.Matrix != null && vehicle.Matrix.Length > 0 )
                {
                    vehicle.Matrix.ForEach(x =>
                    {
                        var splitter = x.Split('|');
                        var employeeSubgroup = splitter[0];
                        var employeeSubgroupText = splitter[1];

                        VehicleMatrixRepository.Add(new VehicleMatrix
                        {
                            VehicleId = vehicle.Id,
                            SequenceClass = employeeSubgroup,
                            GroupSequence = 0,
                            Class = employeeSubgroupText
                        });
                    });
                }

                UnitOfWork.SaveChanges();
            });
        }

        public IEnumerable<object> GetMatrix(Guid vehicleId)
        {
            var configService = new ConfigService(UnitOfWork);
            var employeeSubgroups = configService.GetGeneralCategories("EmployeeSubgroup");
            var vehicleMatrix = VehicleMatrixRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.VehicleId == vehicleId)
                .ToList();

            var output = from eg in employeeSubgroups
                         join vm in vehicleMatrix on eg.Code.Substring(3) equals vm.SequenceClass into egvm
                         from lj in egvm.DefaultIfEmpty()
                         select new
                         {
                             EmployeeSubgroup = eg.Code.Substring(3),
                             EmployeeSubgroupText = eg.Name,
                             Active = lj != null,
                             Position = lj != null ? lj.Position : string.Empty,
                             IsUpgrade = lj == null ? false : lj.IsUpgrade
                         };

            return output;
        }
        #endregion
    }
}
