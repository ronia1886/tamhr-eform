using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using Xunit;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    /// <summary>
    /// Versi service untuk test: tak memanggil extension UspQuery asli.
    /// Kita sediakan 2 hook virtual: ExecUspQuery & ExecUspCommand.
    /// Test akan menurunkan class ini (SpyService) untuk meng-capture parameter.
    /// </summary>
    internal class TestableBdjkClaimService
    {
        public const string SP_GET = "SP_BDJK_CLAIM_GET";
        public const string SP_UPSERT = "SP_BDJK_CLAIM_UPSERT";

        protected readonly IUnitOfWork Uow;
        public TestableBdjkClaimService(IUnitOfWork uow) => Uow = uow;

        // HOOK — DI TEST AKAN DIOVERRIDE
        protected virtual IEnumerable<T> ExecUspQuery<T>(string procName, object args) =>
            throw new NotImplementedException("Hook ExecUspQuery tidak di-override di test.");

        protected virtual void ExecUspCommand(string procName, object args, IDbTransaction tran) =>
            throw new NotImplementedException("Hook ExecUspCommand tidak di-override di test.");

        public virtual IEnumerable<object> GetClaims(string noReg, DateTime period)
        {
            var args = new { NoReg = noReg, Period = period };
            return ExecUspQuery<object>(SP_GET, args);
        }

        public virtual void Upsert(BdjkRequestUpdateViewModel vm)
        {
            Uow.Transact(tran =>
            {
                var args = new
                {
                    vm.Id,
                    vm.WorkingDate,
                    vm.BdjkCode,
                    vm.ActivityCode,
                    vm.BdjkReason,
                    vm.Taxi,
                    vm.UangMakanDinas,
                    vm.DocumentApprovalId,
                    vm.ParentId
                };

                ExecUspCommand(SP_UPSERT, args, tran);
                Uow.SaveChanges();
            });
        }
    }

    public class BdjkClaimServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow = new Mock<IUnitOfWork>();

        public BdjkClaimServiceTest()
        {
            // Jalankan action transaksi langsung; tran boleh null
            _uow.Setup(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()))
                .Callback<Action<IDbTransaction>, IsolationLevel>((act, _) => act((IDbTransaction)null));

            _uow.Setup(u => u.SaveChanges()).Returns(1);
        }

        [Fact]
        public void GetClaims_Memanggil_SP_Get_Dengan_Param_Benar()
        {
            string capturedSp = null;
            object capturedArgs = null;

            var svc = new SpyService(
                _uow.Object,
                onQuery: (sp, args) =>
                {
                    capturedSp = sp;
                    capturedArgs = args;
                    return new List<object> { new { ok = true } };
                },
                onCommand: null
            );

            var period = new DateTime(2025, 1, 1);

            var result = svc.GetClaims("101213", period).ToList();

            Assert.Equal(TestableBdjkClaimService.SP_GET, capturedSp);
            Assert.NotNull(capturedArgs);
            Assert.Equal("101213", capturedArgs.GetType().GetProperty("NoReg")?.GetValue(capturedArgs)?.ToString());
            Assert.Equal(period, capturedArgs.GetType().GetProperty("Period")?.GetValue(capturedArgs));
            Assert.Single(result);
        }

        [Fact]
        public void Upsert_Memanggil_SP_Upsert_Dan_SaveChanges()
        {
            string capturedSp = null;
            object capturedArgs = null;
            bool commandCalled = false;

            var svc = new SpyService(
                _uow.Object,
                onQuery: null,
                onCommand: (sp, args, tran) =>
                {
                    capturedSp = sp;
                    capturedArgs = args;
                    commandCalled = true;
                }
            );

            var vm = new BdjkRequestUpdateViewModel
            {
                Id = Guid.NewGuid(),
                WorkingDate = new DateTime(2025, 2, 3),
                BdjkCode = "BD01",
                ActivityCode = "ACT01",
                BdjkReason = "Lembur",
                Taxi = true,
                UangMakanDinas = false,
                DocumentApprovalId = Guid.NewGuid(),
                ParentId = Guid.NewGuid()
            };

            svc.Upsert(vm);

            Assert.True(commandCalled);
            Assert.Equal(TestableBdjkClaimService.SP_UPSERT, capturedSp);
            Assert.NotNull(capturedArgs);
            Assert.Equal(vm.BdjkCode, capturedArgs.GetType().GetProperty("BdjkCode")?.GetValue(capturedArgs));
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        private sealed class SpyService : TestableBdjkClaimService
        {
            private readonly Func<string, object, IEnumerable<object>> _onQuery;
            private readonly Action<string, object, IDbTransaction> _onCommand;

            public SpyService(
                IUnitOfWork uow,
                Func<string, object, IEnumerable<object>> onQuery,
                Action<string, object, IDbTransaction> onCommand
            ) : base(uow)
            {
                _onQuery = onQuery;
                _onCommand = onCommand;
            }

            protected override IEnumerable<T> ExecUspQuery<T>(string procName, object args)
            {
                if (_onQuery != null)
                    return _onQuery(procName, args).Cast<T>();
                return base.ExecUspQuery<T>(procName, args);
            }

            protected override void ExecUspCommand(string procName, object args, IDbTransaction tran)
            {
                if (_onCommand != null)
                {
                    _onCommand(procName, args, tran);
                    return;
                }
                base.ExecUspCommand(procName, args, tran);
            }
        }
    }
}
