using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() 
              .AllowAnyMethod()  
              .AllowAnyHeader(); 
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

List<Order> repo = new List<Order>()
{
    new Order(1, 03, 03, 2005, "Тостер", "Сгорел", "Включил и сгорел", "Кирилл", "В процессе")

};

foreach (var ord in repo)
{
    ord.EndDate = DateTime.Now;
    ord.Status = "Завершено";
}


string message = "";
bool isUpdatedStatus = false;

app.MapGet("/", () => {
    if (isUpdatedStatus)
    {
        string buffer = message;
        isUpdatedStatus = false; 
        message = "";
        return Results.Json(new OrderUpdateStatusDTO(repo, buffer));
    }
    else
        return Results.Json(repo);
    });
app.MapPost("/", (Order ord) =>
{
    repo.Add(ord);
    return Results.Ok(ord); 
});
app.MapPut("/{id}", (int number, OrderUpdateDTO dto) =>
{
    Order buffer = repo.Find(ord => ord.Number == number);
    if (buffer == null)
        return Results.StatusCode(404);
    if (buffer.Description != dto.Description)
        buffer.Description = dto.Description;
    if (buffer.Master != dto.Master)
        buffer.Master = dto.Master;
    if (buffer.Status != dto.Status)
    {
        buffer.Status = dto.Status;
        isUpdatedStatus = true;
        message += "Статус заявки номер" + buffer.Number + "Изменён\n";
        if(buffer.Status == "Завершено")
            buffer.EndDate = DateTime.Now;
    }
    if (dto.Comments != null || dto.Comments != "")
        buffer.Comments.Add(dto.Comments);
    return Results.Json(buffer);
});
app.MapGet("/{num}", (int number) => repo.Find(ord => ord.Number == number));
app.MapGet("/filter/{param}", (string param) => repo.FindAll(ord =>
ord.Device == param ||
ord.Problem == param ||
ord.Description == param ||
ord.Client == param ||
ord.Status == param ||
ord.Master == param));
app.MapGet("/stat/complcount", () => repo.FindAll(ord => ord.Status == "Завершено").Count);
app.MapGet("/stat/problemTypes", () =>{
    Dictionary<string, int> result = [];
    foreach (var ord in repo)
        if (result.ContainsKey(ord.Problem)) result[ord.Problem]++;
        else  result[ord.Problem] = 1;
    return result;
});
app.MapGet("/stat/avrg", () => {
    double timeSum = 0;
    int ordCount = 0;
    foreach (var ord in repo)
        if (ord.Status == "Завершено")
        {
            timeSum+= ord.TimeInDay();
            ordCount++;
        }
    return ordCount > 0 ? timeSum/ordCount : 0;
});
app.Run();

record class OrderUpdateDTO(string Status, string Description, string Master, string Comments);
record class OrderUpdateStatusDTO(List<Order> repo, string message);
class Order
{
    int number;
    string device;
    string problem;
    string description;
    string client;
    string status;
    string master;

    public Order(int number, int day, int month, int year, string device, string problem, string description, string client, string status)
    {
        Number = number;
        StartDate = new DateTime(year,month,day);
        EndDate = null;
        Device = device;
        Problem = problem;
        Description = description;
        Client = client;
        Status = status;
        Master = "Не назначен";
    }

    public int Number { get => number; set => number = value; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Device { get => device; set => device = value; }
    public string Problem { get => problem; set => problem = value; }
    public string Description { get => description; set => description = value; }
    public string Client { get => client; set => client = value; }
    public string Status { get => status; set => status = value; }
    public string Master { get => master; set => master = value; }
    public List<string> Comments { get; set; } = [];
    public double TimeInDay() => (EndDate - StartDate).Value.TotalDays;

}