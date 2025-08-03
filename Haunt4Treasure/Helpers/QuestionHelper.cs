namespace Haunt4Treasure.Helpers;
public static class QuestionHelper
{
    private static readonly Random rng = new();

    public static List<Question> ShuffleQuestionOptions(List<Question> questions)
    {
        foreach (var question in questions)
        {
            question.Options = ShuffleList(question.Options);
        }

        return questions;
    }

    private static List<string> ShuffleList(List<string> list)
    {
        // Fisher-Yates shuffle
        var shuffled = list.ToList(); // Make a copy
        int n = shuffled.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (shuffled[k], shuffled[n]) = (shuffled[n], shuffled[k]);
        }

        return shuffled;
    }
}
