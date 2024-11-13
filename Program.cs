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
    new Order(1, 03, 03, 2005, "������", "������", "������� � ������", "������", "� ��������")

};


bool isUpdatedStatus = false;
string message = "";

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
        message += "������ ������ �����" + buffer.Number + "�������\n";
    }
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
app.Run();

record class OrderUpdateDTO(string Status, string Description, string Master);
record class OrderUpdateStatusDTO(List<Order> repo, string message);
class Order
{
    int number;
    int day;
    int month;
    int year;
    string device;
    string problem;
    string description;
    string client;
    string status;
    string master;

    public Order(int number, int day, int month, int year, string device, string problem, string description, string client, string status)
    {
        Number = number;
        Day = day;
        Month = month;
        Year = year;
        Device = device;
        Problem = problem;
        Description = description;
        Client = client;
        Status = status;
        Master = "�� ��������";
    }

    public int Number { get => number; set => number = value; }
    public int Day { get => day; set => day = value; }
    public int Month { get => month; set => month = value; }
    public int Year { get => year; set => year = value; }
    public string Device { get => device; set => device = value; }
    public string Problem { get => problem; set => problem = value; }
    public string Description { get => description; set => description = value; }
    public string Client { get => client; set => client = value; }
    public string Status { get => status; set => status = value; }
    public string Master { get => master; set => master = value; }
}