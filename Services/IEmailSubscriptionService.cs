namespace FootballQuestions.Ui.Services
{ 
    public interface IEmailSubscriptionService
    {
        Task<bool> Subscribe(string email);
        Task<bool> Unsubscribe(string email);
    }
}