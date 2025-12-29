using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class AbsenceServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<Absence>> _absenceRepo;
        private readonly Mock<IRepository<AbsenceSummary>> _summaryRepo;

        private readonly AbsenceService _service;

        public AbsenceServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _absenceRepo = new Mock<IRepository<Absence>>();
            _summaryRepo = new Mock<IRepository<AbsenceSummary>>();

            // Map repository yang dipakai service
            _uow.Setup(u => u.GetRepository<Absence>()).Returns(_absenceRepo.Object);
            _uow.Setup(u => u.GetRepository<AbsenceSummary>()).Returns(_summaryRepo.Object);

            _uow.Setup(u => u.SaveChanges());

            _service = new AbsenceService(_uow.Object);
        }

        [Fact]
        public void Upsert_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            var model = new Absence
            {
                Id = Guid.NewGuid(),
                Code = "sakit-default",
                Name = "Sakit",
                CodePresensi = 10,
                RowStatus = true
            };

            _service.Upsert(model);

            _absenceRepo.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_ShouldCallRepositoryDeleteById_AndSaveChanges()
        {
            var id = Guid.NewGuid();

            _service.DeleteById(id);

            _absenceRepo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetDefaultCategories_ShouldFilterNullCodePresensi_AndPreferUpDashOnDuplicates()
        {
            // Arrange: duplikat (CodePresensi=10, Name=Sakit) -> pilih yang Code diawali "up-"
            var a1 = new Absence { Id = Guid.NewGuid(), Code = "sakit-default", Name = "Sakit", CodePresensi = 10, RowStatus = true };
            var a2 = new Absence { Id = Guid.NewGuid(), Code = "up-sakit", Name = "Sakit", CodePresensi = 10, RowStatus = true };
            // Singleton (ikut tampil)
            var a3 = new Absence { Id = Guid.NewGuid(), Code = "izin", Name = "Izin", CodePresensi = 20, RowStatus = true };
            // CodePresensi null (harus terfilter)
            var a4 = new Absence { Id = Guid.NewGuid(), Code = "cuti", Name = "Cuti", CodePresensi = null, RowStatus = true };
            // Singleton lain (boleh ada "up-" tapi bukan duplikat)
            var a5 = new Absence { Id = Guid.NewGuid(), Code = "up-lain", Name = "Lain", CodePresensi = 30, RowStatus = true };

            var data = new List<Absence> { a1, a2, a3, a4, a5 }.AsQueryable();
            _absenceRepo.Setup(r => r.Fetch()).Returns(data);

            // Act
            var result = _service.GetDefaultCategories().ToList();

            // Assert: hanya a2 (pilihan dari duplikat), a3 (singleton), a5 (singleton) => total 3
            Assert.Equal(3, result.Count);

            var codes = result.Select(x => x.Code).ToList();
            Assert.Contains("up-sakit", codes);
            Assert.Contains("izin", codes);
            Assert.Contains("up-lain", codes);

            Assert.DoesNotContain("sakit-default", codes); // terfilter oleh preferensi "up-"
            Assert.DoesNotContain("cuti", codes);          // CodePresensi null -> terfilter
        }

        [Fact]
        public void GetAbsencesBySummaryCategory_ShouldJoinPresenceCodes_AndReturnDictionary()
        {
            // Arrange: Summary category "SICK" punya presence codes 10 & 30 (RowStatus=true)
            var summaries = new List<AbsenceSummary>
            {
                new AbsenceSummary { Id = Guid.NewGuid(), SummaryCategoryCode = "SICK", PresenceCode = 10, RowStatus = true },
                new AbsenceSummary { Id = Guid.NewGuid(), SummaryCategoryCode = "SICK", PresenceCode = 30, RowStatus = true },
                new AbsenceSummary { Id = Guid.NewGuid(), SummaryCategoryCode = "OTHER", PresenceCode = 99, RowStatus = true },
                new AbsenceSummary { Id = Guid.NewGuid(), SummaryCategoryCode = "SICK", PresenceCode = 40, RowStatus = false }, // ignore: RowStatus=false
            }.AsQueryable();
            _summaryRepo.Setup(r => r.Fetch()).Returns(summaries);

            var absences = new List<Absence>
            {
                new Absence { Id = Guid.NewGuid(), Code = "up-sakit", Name = "Sakit", CodePresensi = 10, RowStatus = true },
                new Absence { Id = Guid.NewGuid(), Code = "izin",     Name = "Izin",  CodePresensi = 20, RowStatus = true },
                new Absence { Id = Guid.NewGuid(), Code = "up-lain",  Name = "Lain",  CodePresensi = 30, RowStatus = true },
            }.AsQueryable();
            _absenceRepo.Setup(r => r.Fetch()).Returns(absences);

            // Act
            var dict = _service.GetAbsencesBySummaryCategory("SICK");

            // Assert: hanya 10 ("Sakit") dan 30 ("Lain")
            Assert.Equal(2, dict.Count);
            Assert.True(dict.ContainsKey(10));
            Assert.True(dict.ContainsKey(30));
            Assert.Equal("Sakit", dict[10]);
            Assert.Equal("Lain", dict[30]);
            Assert.False(dict.ContainsKey(20));
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenIdExists()
        {
            var idExists = Guid.NewGuid();
            var data = new List<Absence>
            {
                new Absence { Id = idExists, Code = "up-sakit", Name = "Sakit", CodePresensi = 10, RowStatus = true },
                new Absence { Id = Guid.NewGuid(), Code = "izin", Name = "Izin", CodePresensi = 20, RowStatus = true },
            }.AsQueryable();
            _absenceRepo.Setup(r => r.Fetch()).Returns(data);

            Assert.True(_service.Contains(idExists));
            Assert.False(_service.Contains(Guid.NewGuid()));
        }
    }
}
