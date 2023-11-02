namespace LinkQ.Api.model
{
    public class SurveyModel
    {

    }

    public class QuesionModel
    {
        public string Quesion_Id { get; set; }
        public string Stt { get; set; }
        public string quesion_Text { get; set; }
        public string quesion_Code { get; set; }
        public bool isMulti { get; set; }


    }

    public class AnswersModel
    {
        public string Answers_Id { get; set; }
        public string Stt { get; set; }
        public string Answers_Text { get; set; }
        public string Answers_Value { get; set; }
        public string Quesion_Id { get; set; }

    }


}
