using FootballQuestions.Ui.Models;

namespace FootballQuestions.Ui.Services
{
    public static class QuizState
    {
        public static List<UserAnswer> UserQuizAnswers { get; set; } = [];
        public static List<QuizQuestion> AllQuestions { get; set; } = [];
    }
}