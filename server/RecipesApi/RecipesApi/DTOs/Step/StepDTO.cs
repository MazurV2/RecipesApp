namespace RecipesApi.DTOs.Step
{
    public class StepDTO
    {
        public int Id { get; set; }
        public int StepNumber { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
    }
}
