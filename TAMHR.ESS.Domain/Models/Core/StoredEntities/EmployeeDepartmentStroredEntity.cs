using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMPLOYEE_BY_DEPT_ID", DatabaseObjectType.StoredProcedure)]
    public class EmployeeDepartmentStroredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int Classification { get; set; }
    }

    [DatabaseObject("SP_GET_EMPLOYEE_BY_DEPT_ID_PTA", DatabaseObjectType.StoredProcedure)]
    public class EmployeeDepartmentPtaStroredEntity
    {

        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int Classification { get; set; }
    }

    [DatabaseObject("SP_GET_EMPLOYEE_BY_DIV_ID_PTA", DatabaseObjectType.StoredProcedure)]
    public class EmployeeDivisionPtaStroredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int Classification { get; set; }
    }

    [DatabaseObject("SP_GET_EMPLOYEE_BY_DEPT_ID_VACATION", DatabaseObjectType.StoredProcedure)]
    public class EmployeeDepartmentVacationStroredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public bool HasVacation { get; set; }
        public int Classification { get; set; }
    }

    [DatabaseObject("SP_GET_EMPLOYEE_BY_DIV_ID_VACATION", DatabaseObjectType.StoredProcedure)]
    public class EmployeeDivisionVacationStroredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public bool HasVacation { get; set; }
        public int Classification { get; set; }
    }

    [DatabaseObject("SP_GET_EMPLOYEE_BY_DEPT_ID_SHIFT", DatabaseObjectType.StoredProcedure)]
    public class EmployeeDepartmentShiftStroredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int Classification { get; set; }
    }

    [DatabaseObject("SP_GET_EMPLOYEE_ALLOWANCE_BY_DEPT_ID", DatabaseObjectType.StoredProcedure)]
    public class EmployeeAllowanceStroredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int Classification { get; set; }
        public decimal Amount { get; set; }
    }

    [DatabaseObject("[SP_GET_EMPLOYEE_SHIFT_MEAL_ALLOWANCE_BY_DEPT_ID]", DatabaseObjectType.StoredProcedure)]
    public class EmployeeShiftMealAllowanceStroredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int Classification { get; set; }
        public decimal Amount { get; set; }
        public string AttendanceDate { get; set; }
        public string ShiftCode { get; set; }
    }
}
