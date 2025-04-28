using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
// Added as service - הוספת שירותי שרת MYSQL
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql("server=localhost;user=root;password=Lia120406;database=practicode", ServerVersion.AutoDetect("server=localhost;user=root;password=Lia120406;database=practicode")));


//הוספת סוואגר 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוסף את CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();


var app = builder.Build();

// הפעלת Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
    c.RoutePrefix = string.Empty; // אם את רוצה שה-Swagger UI יהיה ב-root
});

// השתמש במדיניות CORS
app.UseCors("AllowAllOrigins");


//בקשת GET - שולף מתוך הSQL דרך ההזרקה את כל הנתונים שיש שם, ומחבר אותם לליסט אחדט
app.MapGet("/get", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
});




app.MapPost("/post", async (Item todoItem, ToDoDbContext db) =>
{
    db.Items.Add(todoItem);
    await db.SaveChangesAsync();

    return Results.Created($"/post/{todoItem.Id}", todoItem);
});



app.MapPut("/put/{id}", async (int id, Item dto, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);

    if (item is null) return Results.NotFound();

    item.Name = dto.Name;
    item.IsComplete = dto.IsComplete; // הנח שהשדה הזה קיים במחלקת Item

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/delete/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item != null)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.Ok(item); // מחזיר את האובייקט שנמחק
    }

    return Results.NotFound(); // מחזיר NotFound אם האובייקט לא נמצא
});


app.Run();



