using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class HealthDeclarationViewModel
    {
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string EmergencyFamilyStatus { get; set; }
        public string EmergencyName { get; set; }
        public string EmergencyPhoneNumber { get; set; }
        public string WorkTypeCode { get; set; }
        public bool? HealthDeclarationFilled { get; set; }
        public bool? HaveFever { get; set; }
        public string BodyTemperature { get; set; }
        public string BodyTemperatureOtherValue { get; set; }
        public string Remarks { get; set; }
        public FormAnswerViewModel[] FormAnswers { get; set; } = Array.Empty<FormAnswerViewModel>();
    }

    public class HealthDeclarationUpdateRemarksRequest
    {
        public Guid DocumentApprovalId { get; set; }
        public string Remarks { get; set; }
    }
}
