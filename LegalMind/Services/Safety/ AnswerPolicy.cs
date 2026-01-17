namespace LegalMind.Web.Services.Safety;

public class AnswerPolicy : IAnswerPolicy
{
    public bool ShouldRefuse(string userQuestion, out string reason)
    {
        reason = "";

        // Minimal guardrails (expand later)
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            reason = "Please enter a question.";
            return true;
        }

        return false;
    }
}
