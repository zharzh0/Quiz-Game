using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class QuizService
{
    private readonly HttpClient _httpClient;

    private List<(string Question, string[] Options, string Answer)> _remainingQuestions;

    public QuizService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _remainingQuestions = new List<(string Question, string[] Options, string Answer)>();
    }

    public async Task<List<(string Question, string[] Options, string Answer)>> LoadQuestionsAsync()
    {
        var questions = new List<(string Question, string[] Options, string Answer)>();

        var csvContent = await _httpClient.GetStringAsync("tietovisakysymykset.csv");

        using (var reader = new StringReader(csvContent))
        {
            string? line;
            bool isFirstLine = true; // Skip headers
            while ((line = reader.ReadLine()) != null)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                string[] values = line.Split(',');
                if (values.Length >= 6)
                {
                    string question = values[0];
                    string[] options = new string[4];
                    options[0] = values[1];
                    options[1] = values[2];
                    options[2] = values[3];
                    options[3] = values[4];
                    string answer = values[5];

                    questions.Add((question, options, answer));
                }
            }
        }

        var rng = new Random();
        int n = questions.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = questions[k];
            questions[k] = questions[n];
            questions[n] = value;
        }

        return questions;
    }

    public (string Question, string[] Options, string Answer)? GetRandomQuestion()
    {
        if (_remainingQuestions.Count == 0)
        {
            return null;
        }

        var rng = new Random();
        int index = rng.Next(_remainingQuestions.Count);

        var selectedQuestion = _remainingQuestions[index];

        _remainingQuestions.RemoveAt(index);

        return selectedQuestion;
    }
}
