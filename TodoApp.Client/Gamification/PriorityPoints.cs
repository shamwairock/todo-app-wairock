using TodoApp.Client.Models;

namespace TodoApp.Client.Gamification;

public static class PriorityPoints
{
    public static int BasePoints(Priority priority) => priority switch
    {
        Priority.Low => 5,
        Priority.Medium => 10,
        Priority.High => 20,
        _ => 10,
    };
}
