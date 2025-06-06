using FootballQuestions.Ui.Models;

namespace FootballQuestions.Ui.Services
{
    public interface IFootballQuestionService
    {
        Task<FootballQuestion?> GetTodaysQuestion();
        
        Task<List<QuizQuestion>?> GetQuizFootballQuestions();
    }
}