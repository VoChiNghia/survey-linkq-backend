using LinkQ.Api.model;
using LinkQ.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LinkQ.Api.Controllers;
using LinkQ.Api.Framework.Models;
using LinkQ.Core;
using Microsoft.Data.SqlClient;
using System.Reflection;
using LinkQ.Data.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using LinkQ.Api.Framework.MailKit;
using Microsoft.Extensions.FileProviders;
using LinkQ.Core.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Transactions;
using Microsoft.IdentityModel.Tokens;

namespace LinkQ.Api.factories
{
    public class SurveyModelFactory : ISurveyModelFactory
    {
        private ILinkQDataProvider _dataProvider;
        private readonly Func<MailConfig?, IMailService> _mailServiceFactory;
        private readonly ILinkQFileProvider _fileProvider;
        public SurveyModelFactory(ILinkQDataProvider dataProvider, Func<MailConfig?, IMailService> mailServiceFactory, ILinkQFileProvider fileProvider)
        {
            _dataProvider = dataProvider;
            _mailServiceFactory = mailServiceFactory;
            _fileProvider = fileProvider;
        }

        public async Task<object> GetQuestionAsync(string surveyId)
        {
            try
            {
                string queryQuestion = "select q.Question_Id,q.Display_Order,q.Question_Text,q.Question_Code,q.Is_Multi,q.Description,q.Type_Question_Group" +
                    " from L17SURVEY_QUESION_MAPPING as m full join L17SURVEY as s on m.Survey_Id = s.Survey_Id" +
                    $" full join L17SURVEY_QUESION as q on m.Question_Id = q.Question_Id where m.Survey_Id = {surveyId}";
                var quesions = await _dataProvider.QueryAsync(queryQuestion, commandType: System.Data.CommandType.Text);
                var answers = await _dataProvider.QueryAsync("select * from L17SURVEY_QUESION_ANSWERS", commandType: System.Data.CommandType.Text);

                var mappedList = quesions.Select(q => new
                {
                    question = q,
                    answers = answers.Where(a => a?.Question_Id == q?.Question_Id).ToList()
                }).ToList();

                return mappedList;
            }
            catch (Exception ex) { }
            return null;

        }

        public async Task<dynamic> GetAllColumnName()
        {
            var resultObject = new Dictionary<string, object>();
            string queryStr = "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'l17survey_result'";
            var columnName = await _dataProvider.QueryAsync(queryStr, commandType: System.Data.CommandType.Text);
            foreach (var item in columnName)
            {
                string dataType = item.data_type;

                // Dựa vào kiểu dữ liệu của cột, gán giá trị mặc định
                object defaultValue = GetDefaultValueForDataType(dataType);
                resultObject.Add(item.column_name, defaultValue);
            }
            return resultObject;
        }


        public async Task<dynamic> GetCustomerByMST(string mst)
        {
            string queryStr = $"SELECT * FROM L81DMDT WHERE MA_SO_THUE = '{mst}'";
            return await _dataProvider.QueryFirstOrDefaultAsync(queryStr, commandType: System.Data.CommandType.Text);

        }

        public async Task<LinkQResponse<dynamic>> GetInfoCustomerSurveyedByMST(string mst)
        {
            string queryStr = $"SELECT TOP 1 * FROM L17SURVEY_RESULT WHERE Created_Date = (SELECT MAX(Created_Date) FROM L17SURVEY_RESULT) and TAX_CODE = '{mst}'";
            var result = await _dataProvider.QueryFirstOrDefaultAsync(queryStr, commandType: System.Data.CommandType.Text);
            return result;
        }


        public async Task<LinkQResponse<INoContentModel>> CreateResult(ServeyModel listModel)
        {
            List<dynamic> jObjectList = JsonConvert.DeserializeObject<List<dynamic>>(listModel.model);

            using (var scope = new TransactionScope())
            {
                try
                {
                    foreach (dynamic item in jObjectList)
                    {
                        _dataProvider.InsertWithTableName("L17SURVEY_RESULT", item);
                    }
                    scope.Complete();
                    return new LinkQResponse<INoContentModel> { successful = true, msg = "Đã gửi" };
                }
                catch (Exception ex)
                {
                    return new LinkQResponse<INoContentModel> { successful = false, msg = "Có lỗi xãy ra trong quá trình gửi" };

                }
            }

            //try
            //{
            //    string template = @"
            //        <!DOCTYPE html>
            //        <html>
            //        <head>
            //            <meta charset='utf-8' />
            //            <title></title>
            //        </head>
            //        <body>
            //            <h1>Xin chào {1},</h1>
            //            <h1>Kết quả khảo sát:</h1>
            //            {0}
            //            <p>Trân trọng,</p>
            //        </body>
            //        </html>
            //        ";


            //    string bodyEmail = "";
            //    var name = jObjectList[0].Name;
            //    var email = jObjectList[0].Email;
            //    List<string> emails = new List<string>();
            //    int stt = 1;
            //    foreach (dynamic item in jObjectList)
            //    {
            //        _dataProvider.InsertWithTableName("L17SURVEY_RESULT", item);

            //        string questionId = item["Question_Id"];
            //        var quetion = await getQuestionById(questionId);
            //        var questionText = quetion.Question_Text;
            //        bodyEmail += $"<h2 style=\"margin: 0;\"><strong>Câu hỏi: {stt} </strong>{questionText}</h2><br>";

            //        if (item.Answers_Id != null)
            //        {
            //            JValue jValue = new JValue(item.Answers_Id);
            //            string stringValue = jValue.ToString();
            //            var answers = stringValue.Split(",");
            //            foreach (dynamic answerId in answers)
            //            {
            //                var anwser = await getAnswerById(answerId);
            //                string answerText = anwser.Answer_Text;
            //                bodyEmail += $"<p style=\"margin: 0;\"><strong>Câu trả lời:</strong> {answerText}</p><br>";

            //            }
            //        }

            //        if (item.Note != null | item.Note != "")
            //        {
            //            var a = item.Note;
            //            bodyEmail += $"<p><strong>Ý kến bổ sung:</strong> {item.Note}</p><br>";
            //        }
            //        stt++;
            //    }

            //    string templateBodyEmail = string.Format(template, bodyEmail, name);

            //    var mailService = _mailServiceFactory(new MailConfig
            //    {
            //        Address = "nghiakg11234@gmail.com",
            //        Password = "timgbkwmcxzmqdpq",
            //        DisplayName = "Addmin",
            //        UseSSL = false
            //    });

            //    await mailService.SendAsync(new MailRequest()
            //    {
            //        ToEmail = email,
            //        Body = templateBodyEmail,
            //        Subject = "Kết quả khảo sát"

            //    });

            //    return new LinkQResponse<INoContentModel> { successful = true, msg = "Đã gửi" };

            //}
            //catch (Exception ex)
            //{
            //    return new LinkQResponse<INoContentModel> { successful = false, msg = "Có lỗi xãy ra trong quá trình gửi" };

            //}




        }


        public async Task<dynamic> SendMail(string surveyId, string? email = null)
        {
            string baseUrl = "https://linkq@123.com";
            string emailContent = await _fileProvider.ReadAllTextAsync("Content/EmailTemplate.html", System.Text.Encoding.UTF8);

            string[] emailArray = email.Split(',');
            string emailList = string.Join("','", emailArray);

            var mailService = _mailServiceFactory(new MailConfig
            {
                Address = "nghiakg11234@gmail.com",
                Password = "timgbkwmcxzmqdpq",
                DisplayName = "Addmin",
                UseSSL = false
            });

            string queryStr = String.IsNullOrEmpty(email) ?  $"SELECT email, ma_so_thue, ten_dt FROM L81DMDT WHERE email IS NOT NULL AND email != ''" : $"SELECT email, ma_so_thue,  ten_dt FROM L81DMDT WHERE email IS NOT NULL AND email != '' and email in('{emailList}')";
            var allEmail = await _dataProvider.QueryAsync(queryStr, commandType: System.Data.CommandType.Text);

            foreach(dynamic item in allEmail)
            {
                emailContent = emailContent.Replace("{{survey.link}}", baseUrl + $"/{surveyId}" + $"/{item.ma_so_thue}")
                                            .Replace("{{survey.name}}",$"{item.ten_dt}");
                await mailService.SendAsync(new MailRequest()
                {
                    ToEmail = item.email,
                    Body = emailContent,
                    Subject = "Công Ty Phần Mềm LinkQ"

                });
            }

            return new LinkQResponse<INoContentModel> { successful = true, msg = "Đã gửi" };
        }

        public async Task<dynamic> GetResultSurvey(string mst, string surveyId)
        {
            string queryStr = $"SELECT * FROM L17SURVEY_RESULT WHERE TAX_CODE = '{mst}' AND SURVEY_ID = '{surveyId}'";
            var result = await _dataProvider.QueryAsync(queryStr, commandType: System.Data.CommandType.Text);
            return result;
        }

        private async Task<dynamic> getQuestionById(string id)
        {
            string queryStr = $"SELECT * FROM L17SURVEY_QUESION WHERE QUESTION_ID = '{id}'";
            var result = await _dataProvider.QueryFirstOrDefaultAsync(queryStr, commandType: System.Data.CommandType.Text);
            if (result.Count == 0)
            {
                return null;
            }
            return result;
        }

        private async Task<dynamic> getAnswerById(string id)
        {
            string queryStr = $"SELECT * FROM L17SURVEY_QUESION_ANSWERS WHERE ANSWER_ID = '{id}'";
            var result = await _dataProvider.QueryFirstOrDefaultAsync(queryStr, commandType: System.Data.CommandType.Text);
            if (result.Count == 0)
            {
                return null;
            }
            return result;
        }

        public async Task<LinkQResponse<INoContentModel>> CheckSurveyedCustomer(string mst, string surveyId)
        {
            string queryStr = $"SELECT * FROM L17SURVEY_RESULT WHERE TAX_CODE = '{mst}' AND SURVEY_ID = '{surveyId}'";
            var result = await _dataProvider.QueryAsync(queryStr, commandType: System.Data.CommandType.Text);
            if (result.Count != 0)
            {
                return new LinkQResponse<INoContentModel> { successful = true, msg = "Bạn đã thực hiện khảo sát rồi" };
            }
            return new LinkQResponse<INoContentModel> { successful = false, msg = "Bạn chưa đã thực hiện khảo sát rồi" };
        }

        public async Task<dynamic> GetSurveyInfo(string surveyId)
        {
            string queryStr = $"SELECT * FROM L17SURVEY WHERE SURVEY_ID = '{surveyId}'";
            var result = await _dataProvider.QueryAsync(queryStr, commandType: System.Data.CommandType.Text);
            if (result.Count != 0)
            {
                return result.First();
            }
            return null;
        }

        private object GetDefaultValueForDataType(string dataType)
        {
            if (dataType == "int")
            {
                return 0;
            }
            else if (dataType == "nvarchar" || dataType == "varchar")
            {
                return string.Empty;
            }
            else if (dataType == "datetime")
            {
                return DateTime.Now;
            }
            return null;
        }
    }
}
