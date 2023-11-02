using LinkQ.Api.Controllers;
using LinkQ.Api.Framework.Models;
using LinkQ.Api.model;
using LinkQ.Core;

namespace LinkQ.Api.factories
{
    public interface ISurveyModelFactory
    {
        Task<object> GetQuestionAsync(string surveyId);
        Task<dynamic> GetAllColumnName();
        Task<dynamic> GetCustomerByMST(string mst);
        Task<LinkQResponse<INoContentModel>> CreateResult(ServeyModel listModel);
        Task<LinkQResponse<INoContentModel>> CheckSurveyedCustomer(string mst, string surveyId);
        Task<dynamic> GetSurveyInfo(string surveyId);
        Task<LinkQResponse<dynamic>> GetInfoCustomerSurveyedByMST(string mst);
        Task<dynamic> GetResultSurvey(string mst, string surveyId);
        Task<dynamic> SendMail(string surveyId, string? email);
    }
}
