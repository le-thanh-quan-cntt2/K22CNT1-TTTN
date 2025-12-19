using System;
using System.Collections.Generic;

namespace MenFashionProject.Models;

public partial class Post
{
    public int PostId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? Image { get; set; }

    public DateTime? CreatedDate { get; set; }
}
