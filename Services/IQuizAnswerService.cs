using FootballQuestions.Ui.Models;

namespace FootballQuestions.Ui.Services
{
    public interface IQuizAnswerService
    {
        Task<bool> SaveQuizAnswers(List<UserAnswer> answers, string userId);
    }
}
