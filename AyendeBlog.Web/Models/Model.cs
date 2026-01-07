using Microsoft.AspNetCore.Mvc;

namespace AyendeBlog.Web.Models;

public class Model
{
    [HiddenInput]
    public string Id { get; set; }
}