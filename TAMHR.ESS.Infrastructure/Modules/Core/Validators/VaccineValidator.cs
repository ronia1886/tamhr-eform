using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class VaccineValidator : AbstractValidator<Vaccine>
    {
        public VaccineValidator(ConfigService configService, FormService formService,VaccineService vaccineService, IStringLocalizer<VaccineValidator> localizer)
        {
            RuleFor(x => x.BirthDate).NotEmpty()
                .WithMessage(localizer["Tanggal Lahir harus diisi"].Value);

            RuleFor(x => x.PhoneNumber)
            .Must((data, x) =>
            {
                var output = true;
                if (data.PhoneNumber == "" && data.FamilyStatus.ToLower() == "employee")
                {
                    output = false;
                }
                return output;
            })
                .WithMessage(localizer["No Telepon harus diisi"].Value);

            RuleFor(x => x.Domicile)
            .Must((data, x) =>
            {
                var output = true;
                if (data.Domicile == "" && data.FamilyStatus.ToLower() == "employee")
                {
                    output = false;
                }
                return output;
            })
                .WithMessage(localizer["Domisili harus diisi"].Value);
            
            RuleFor(x => x.Address)
            .Must((data, x) =>
            {
                var output = true;
                if (data.Address == "" && data.FamilyStatus.ToLower() == "employee")
                {
                    output = false;
                }
                return output;
            })
                .WithMessage(localizer["Alamat harus diisi"].Value);

            RuleFor(x => x.IdentityId).NotEmpty()
                .WithMessage(localizer["No KTP/KK harus diisi"].Value)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.IdentityId != null)
                    {
                        if (data.IdentityId.Replace("_", "").Length != 16)
                        {
                            output = false;
                        }
                    }
                    
                    return output;
                }).WithMessage(localizer["Harus 16 Karakter Spesifik Angka"].Value)
                .Must(x => new Regex(@"^-?[0-9][0-9,\.]+$").IsMatch(x))
                .WithMessage(localizer["Harus bernilai angka"].Value);
                

            RuleFor(x => x.IdentityImage).NotEmpty()
                .WithMessage(localizer["KTP/KK harus diupload"].Value);
                
            //RuleFor(x => x.IsAllergies).NotEmpty()
            //    .WithMessage(localizer["Alergi harus diisi"].Value);
            
            //RuleFor(x => x.Allergies)
            //    .Must((data, x) => 
            //    {
            //        var output = true;
            //        if(data.IsAllergies==true && data.Allergies == "")
            //        {
            //            output = false;
            //        }
            //        return output;
            //    })
            //    .WithMessage(localizer["Deskripsi alergi harus diisi"].Value);

            //RuleFor(x => x.IsPositive).NotEmpty()
            //    .WithMessage(localizer["Diagnosa COVID-19 harus diisi"].Value);
            //RuleFor(x => x.LastNegativeSwabDate)
            //    .Must((data,x) =>
            //    {
            //        var output = true;
            //        if(data.IsPositive==true && data.LastNegativeSwabDate == null)
            //        {
            //            output = false;
            //        }
            //        return output;
            //    })
            //    .WithMessage(localizer["Tanggal negatif SWAB harus diisi"].Value);


            RuleFor(x => x.VaccineQuestionList).Custom((data, context) =>
            {
                var formQuestions = formService.GetFormQuestions(ApplicationForm.Vaccine)
                   .Where(x => x.RowStatus);
                
                var roots = formQuestions.Where(x => x.ParentFormQuestionId == null && x.IsActive == true);
                var questionIds = data.Select(x => x.FormQuestionId).ToList();
                foreach (var root in roots)
                {
                    List<FormQuestion> item = new List<FormQuestion>();
                    int totItems = 0;
                    if (data.Count > 0)
                    {
                        if (!data[0].Gender.ToLower().Contains("laki"))
                        {
                            item = formQuestions.Where(x => x.ParentFormQuestionId == root.Id && root.IsActive == true).ToList();
                        }
                        else
                        {
                            item = formQuestions.Where(x => x.ParentFormQuestionId == root.Id && root.IsActive == true && !x.Title.ToLower().Contains("hamil")).ToList();
                        }
                        
                    }
                    else
                    {
                        item = formQuestions.Where(x => x.ParentFormQuestionId == root.Id && root.IsActive == true).ToList();
                    }

                    totItems = item.Count();
                    var dataVal = data.Where(x => item.Find(y => y.Id == x.FormQuestionId) != null).ToList();
                    int totData = dataVal.Count();

                    if (totData != totItems)
                    {
                        context.AddFailure("FormAnswers." + root.Id, localizer["* Mohon untuk diisi di setiap pertanyaan ini"].Value);
                    }
                    else
                    {
                        bool error = false;
                        foreach (var detData in dataVal)
                        {
                            if (detData.Answer.ToLower() == "true")
                            {
                                int countDetData = detData.VaccineQuestionDetailList.Count;

                                var itemDetail = formService.GetFormQuestionDetails(detData.FormQuestionId).ToList();
                                foreach (var detDataItem in detData.VaccineQuestionDetailList)
                                {
                                    if (detDataItem.Answer == "" || detDataItem.Answer == null)
                                    {
                                        error = true;
                                    }
                                }
                            }

                        }

                        if (error)
                        {
                            context.AddFailure("FormAnswers." + root.Id, localizer["* Mohon untuk diisi di setiap pertanyaan ini"].Value);
                        }
                    }
                }
            });

            //RuleFor(x => x.HaveVaccine)
            //    .Must((data, x) =>
            //    {
            //        var output = true;
            //        if (data.Id != Guid.Empty && data.SHAStatus == true && data.HaveVaccine == null)
            //        {
            //            output = false;
            //        }
            //        return output;
            //    })
            //    .WithMessage(localizer["Field ini harus diisi"].Value);

            RuleFor(x => x.TAMVaccineAgreement)
                .NotEmpty()
                .WithMessage(localizer["Persetujuan vaksin di TAM harus diisi"].Value);
            RuleFor(x => x.VaccineCard1)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.VaccineCard1 != null && data.VaccineCard1 != "" && (data.VaccineDate1 == null || data.VaccineHospital1 == null))
                    {
                        output = false;
                    }
                    return output;
                })
                .WithMessage(localizer["Jadwal vaksin harus diisi"].Value);

            //RuleFor(x => x.IsSideEffects1)
            //    .Must((data, x) =>
            //    {
            //        var output = true;
            //        if (data.IsSideEffects1 == null && (data.VaccineCard1 == null || data.VaccineCard1 == ""))
            //        {
            //            output = false;
            //        }
            //        return output;
            //    })
            //    .WithMessage(localizer["Efek samping harus diisi"].Value);

            RuleFor(x => x.SideEffects1)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.IsSideEffects1 == true && data.SideEffects1 == "")
                    {
                        output = false;
                    }
                    return output;
                })
                .WithMessage(localizer["Deskripsi efek samping harus diisi"].Value);

            RuleFor(x => x.VaccineCard2)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.VaccineCard2 != null && data.VaccineCard2 != "" && (data.VaccineDate2 == null || data.VaccineHospital2 == null))
                    {
                        output = false;
                    }
                    return output;
                })
                .WithMessage(localizer["Jadwal vaksin harus diisi"].Value);

            //RuleFor(x => x.IsSideEffects2)
            //    .Must((data, x) =>
            //    {
            //        var output = true;
            //        if (data.IsSideEffects2 == null && (data.VaccineCard2 == null || data.VaccineCard2 == ""))
            //        {
            //            output = false;
            //        }
            //        return output;
            //    })
            //    .WithMessage(localizer["Efek samping harus diisi"].Value);

            RuleFor(x => x.SideEffects2)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.IsSideEffects2 == true && data.SideEffects2 == "")
                    {
                        output = false;
                    }
                    return output;
                })
                .WithMessage(localizer["Deskripsi efek samping harus diisi"].Value);
        }
    }
}
