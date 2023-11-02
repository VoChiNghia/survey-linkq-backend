using LinkQ.Api.factories;
using LinkQ.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace LinkQ.Api.Controllers
{
    [Route("api/survey")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        private ISurveyModelFactory _surveyModelFactory;

        #region contructor
        public SurveyController(ISurveyModelFactory surveyModelFactory)
        {
            _surveyModelFactory = surveyModelFactory;
        }
        #endregion

        #region method

        [HttpGet, Route("quesion")]
        public async Task<object> LoadDataDNNSourceAsync([Required] string surveyId) => await _surveyModelFactory.GetQuestionAsync(surveyId);

        [HttpGet, Route("column-name")]
        public async Task<object> GetAllColumn() => await _surveyModelFactory.GetAllColumnName();

        [HttpGet, Route("customer")]
        public async Task<object> GetCustomer([Required] string mst) => await _surveyModelFactory.GetCustomerByMST(mst);

        [HttpGet, Route("customer-surveyed")]
        public async Task<object> GetInfoCustomerSurveyedByMST([Required] string mst) => await _surveyModelFactory.GetInfoCustomerSurveyedByMST(mst);
        [HttpPost, Route("result")]
        public async Task<object> CreateResult([Required] ServeyModel model) => await _surveyModelFactory.CreateResult(model);

        [HttpGet, Route("check-surveyed")]
        public async Task<object> CheckSurveyedCustomer([Required] string mst, [Required] string surveyId) => await _surveyModelFactory.CheckSurveyedCustomer(mst, surveyId);
        #endregion
        [HttpGet, Route("")]
        public async Task<object> GetSurveyInfo([Required] string surveyId) => await _surveyModelFactory.GetSurveyInfo(surveyId);

        [HttpGet, Route("result")]
        public async Task<object> GetSurveyInfo([Required] string mst, [Required] string surveyId) => await _surveyModelFactory.GetResultSurvey(mst, surveyId);
        [HttpGet, Route("send-mail")]
        public async Task<object> SendMail([Required] string surveyId, string email) => await _surveyModelFactory.SendMail(surveyId,email);
    }

    public class ServeyModel {
        public string model { get; set; }
    }


}
