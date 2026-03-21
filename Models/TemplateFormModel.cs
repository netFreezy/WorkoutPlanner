using System.ComponentModel.DataAnnotations;

namespace BlazorApp2.Models;

public class TemplateFormModel
{
    [Required(ErrorMessage = "Template name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = "";

    public string? Description { get; set; }
}
