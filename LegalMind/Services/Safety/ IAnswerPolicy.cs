namespace LegalMind.Web.Services.Safety;

public interface IAnswerPolicy
{
    bool ShouldRefuse(string userQuestion, out string reason);
}
