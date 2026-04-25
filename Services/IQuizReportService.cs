using FootballQuestions.Ui.Models;

namespace FootballQuestions.Ui.Services
{
    public interface IQuizReportService
    {
        Task<UserReportSummary?> GetUserReport(string userId);
        Task<List<QuizReport>?> GetQuizHistory(string userId);
    }
}
