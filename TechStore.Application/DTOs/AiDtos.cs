namespace TechStore.Application.DTOs
{
    public class GenerateDescriptionRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
    }

    public class GenerateDescriptionResponseDto
    {
        public string Description { get; set; } = string.Empty;
    }
}
