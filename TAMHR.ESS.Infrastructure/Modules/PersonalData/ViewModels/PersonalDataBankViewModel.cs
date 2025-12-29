using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class PersonalDataBankViewModel
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string BankKey { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? CreatedOn { get; set; }

        public static PersonalDataBankViewModel CreateFrom(User user, PersonalDataBankAccount personalDataBankAccount, Bank bank)
        {
            var output = new PersonalDataBankViewModel
            {
                NoReg = personalDataBankAccount.NoReg,
                Name = user.Name,
                AccountName = personalDataBankAccount.AccountName,
                AccountNumber = personalDataBankAccount.AccountNumber,
                BankCode = personalDataBankAccount.BankCode,
                StartDate = personalDataBankAccount.StartDate,
                EndDate = personalDataBankAccount.EndDate,
                CreatedOn = personalDataBankAccount.CreatedOn,
                BankName = bank.BankName,
                BankKey = bank.BankKey,
                Branch = bank.Branch
            };

            return output;
        }
    }
}
