using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.WebUI.Areas.OHS;
using Xunit;

namespace TAMHR.ESS.UnitTest.OHS
{
    public class TanyaOhsTest
    {
        public class Data
        {
            public static string TanyaOhsId = "67820F59-C446-4332-AC6B-90A053C2A9DB";
        }
        [Fact]
        public void CreateTanyaOhsTest()
        {
            var result = TanyaOhsHelper.Orm.CreateTanyaOhs(new Domain.TanyaOhs { 
                Keluhan = "Unit Test",
                KategoriLayanan = "Konsultasi Kesehatan",
                Solve = "",
                Feedback = "",
                Rating = "",
                ReplyFeedback = "",
                Status = "On-Going",
                DoctorId = "771BD4BA-4E3E-466E-85F0-E68EB21C0CAD"
            }, "40512FD0-10DE-4146-9771-0E2AFEB2AC0B", "jenal");
            Assert.NotNull(result);
            TanyaOhsHelper.Orm.DeleteTanyaOhs(result);
        }
        [Fact]
        public void GetTanyaOhsTest()
        {
            var result = TanyaOhsHelper.Orm.GetTanyaOhs(Data.TanyaOhsId);
            Assert.Equal(Data.TanyaOhsId, result.Id.ToString().ToUpper());
        }
        [Fact]
        public async void GetChatTest()
        {
            var result = await TanyaOhsHelper.Service.GetChat(Data.TanyaOhsId);
            Assert.NotNull(result);
        }
    }
}
