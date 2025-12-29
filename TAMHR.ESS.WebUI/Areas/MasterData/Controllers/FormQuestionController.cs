using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Requests;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Form question API controller.
    /// </summary>
    [Route("api/form-question")]
    [Permission(PermissionKey.ManageFormQuestion)]
    public class FormQuestionApiController : GenericApiControllerBase<FormQuestionService, FormQuestion>
    {
        protected override string[] ComparerKeys => new[] { "Id" };

        #region Public Methods
        /// <summary>
        /// Get list of questions by form id.
        /// </summary>
        /// <param name="formId">This form id.</param>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("get-roots")]
        public async Task<DataSourceResult> GetRoots([FromForm] Guid formId, [DataSourceRequest] DataSourceRequest request)
        {
            // Get list of questions from given form id.
            return await CommonService.GetRoots(formId)
                .ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of questions by parent id.
        /// </summary>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            // Get and set parent id from form data.
            var parentId = Guid.Parse(Request.Form["parentId"]);

            // Get list of questions from given parent id.
            return await CommonService.GetQuestions(parentId)
                .ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of questions by form question id.
        /// </summary>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("get-details")]
        public async Task<DataSourceResult> GetFormQuestionDetail([FromForm] Guid formQuestionId,[DataSourceRequest] DataSourceRequest request)
        {
            // Get and set parent id from form data.
            //var formQuestionId = Guid.Parse(Request.Form["formQuestionId"]);

            // Get list of questions from given parent id.
            return await CommonService.GetQuestionDetails(formQuestionId)
                .ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of question groups by form id and category.
        /// </summary>
        /// <param name="formId">This form id.</param>
        /// <param name="category">This category.</param>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("get-groups")]
        public async Task<DataSourceResult> GetGroups([FromForm] Guid formId, [FromForm] string category, [DataSourceRequest] DataSourceRequest request)
        {
            // Get list of question groups from given form id and category.
            return await CommonService.GetFormQuestionGroupAnswers(category, formId)
                .ToDataSourceResultAsync(request);
        }

        [HttpPost("reorder")]
        public IActionResult Reorder([FromBody] ReorderRequest reorderRequest)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            var output = CommonService.Reorder(noreg, reorderRequest.Id, reorderRequest.ReferenceId, reorderRequest.OrderIndex);

            return Ok(output);
        }

        [HttpPost("upsert-rules")]
        public IActionResult UpsertRules([FromBody] GenericCategoryRequest<Guid> genericCategoryRequest)
        {
            var output = CommonService.UpsertQuestionGroup(genericCategoryRequest.Category, genericCategoryRequest.Ids);

            return Ok(output);
        }

        [HttpPost("upsert-formquestiondetail")]
        public IActionResult CreateQuestionDetail([FromBody] FormQuestionDetail formQuestionDetail)
        {
            var output = UpsertQuestionDetail(formQuestionDetail);
            return Ok(output);
        }

        [HttpPut("upsert-formquestiondetail")]
        public IActionResult UpdateQuestionDetail([FromBody] FormQuestionDetail formQuestionDetail)
        {

            var output = UpsertQuestionDetail(formQuestionDetail);
            return Ok(output);
        }

        public bool UpsertQuestionDetail(FormQuestionDetail formQuestionDetail)
        {
            if (formQuestionDetail.MinVal != "")
            {
                formQuestionDetail.MinVal = formQuestionDetail.MinVal.Replace("00:00 AM", "");
            }
            if (formQuestionDetail.MaxVal != "")
            {
                formQuestionDetail.MaxVal = formQuestionDetail.MaxVal.Replace("00:00 AM", "");
            }

            if (formQuestionDetail.MinTrue != "")
            {
                formQuestionDetail.MinTrue = formQuestionDetail.MinTrue.Replace("00:00 AM", "");
            }
            if (formQuestionDetail.MaxTrue != "")
            {
                formQuestionDetail.MaxTrue = formQuestionDetail.MaxTrue.Replace("00:00 AM", "");
            }
            return CommonService.UpsertDetail(formQuestionDetail);
        }

        [HttpDelete("delete-formquestiondetail")]
        public IActionResult DeleteDetailById([FromForm] Guid id)
        {

            var output = CommonService.DeleteDetailById(id);

            return Ok(output);
        }
        #endregion
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Form question page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewFormQuestion)]
    public class FormQuestionController : GenericMvcControllerBase<FormQuestionService, FormQuestion>
    {
        /// <summary>
        /// Override load data method.
        /// </summary>
        /// <param name="id">This form question id.</param>
        public override IActionResult Load(Guid id)
        {
            // Get form question data by id.
            var commonData = CommonService.GetById(id);

            if(commonData == null)
            {
                commonData = new FormQuestion();
            }

            // If is new then set the parent id from given form data if any.
            if (id == Guid.Empty)
            {
                // Get and set parent id from form data if any.
                var parentId = Request.Form.ContainsKey("pid")
                    ? Guid.Parse(Request.Form["pid"])
                    : new Guid?();

                var parent = CommonService.GetQuery()
                    .FirstOrDefault(x => x.Id == parentId);

                // Get and set form id from form data if any.
                var formId = Request.Form.ContainsKey("fid")
                    ? Guid.Parse(Request.Form["fid"])
                    : (parent != null ? parent.FormId : new Guid?());

                // Update form id.
                if (formId.HasValue)
                {
                    commonData.FormId = formId.Value;
                }

                // Update the parent id.
                commonData.ParentFormQuestionId = parentId;
            }

            // Return the view model.
            return GetViewData(commonData);
        }

        public IActionResult LoadRules(Guid formId)
        {
            return PartialView("_RulesForm");
        }

        public IActionResult LoadDetail(Guid Id,Guid FormQuestionId)
        {
            FormQuestionDetail dataDetail = new FormQuestionDetail();
            dataDetail.FormQuestionId = FormQuestionId;
            var data = CommonService.GetQuestionDetailById(Id);
            if (data != null)
            {
                dataDetail = data;
            }

            return PartialView("_FormQuestionDetailForm", dataDetail);
        }
    }
    #endregion
}