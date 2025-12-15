using Microsoft.AspNetCore.Mvc;

namespace RaccoonBlog.Web.Core.Models;

public class Model
{
    [HiddenInput]
    public string Id { get; set; }
}